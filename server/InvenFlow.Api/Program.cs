using InvenFlow.Api.Data;
using InvenFlow.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var useSqlite = builder.Configuration.GetValue("InvenFlow:UseSqlite", false);
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")!;
    if (useSqlite) opt.UseSqlite(cs);
    else opt.UseSqlServer(cs);
});

builder.Services.AddScoped<AccountingService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Auto migrate / seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var acct = scope.ServiceProvider.GetRequiredService<AccountingService>();
    if (builder.Configuration.GetValue("InvenFlow:AutoMigrate", true))
        await db.Database.EnsureCreatedAsync();
    if (builder.Configuration.GetValue("InvenFlow:AutoSeed", true))
        await SeedData.EnsureSeededAsync(db, acct);
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
