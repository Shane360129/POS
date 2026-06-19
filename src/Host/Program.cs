using InvenFlow.BuildingBlocks.Abstractions;
using InvenFlow.BuildingBlocks.Modules;
using InvenFlow.BuildingBlocks.Web;
using InvenFlow.Modules.Mdm;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(cfg => cfg
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console());

// 模組註冊：每個模組自帶 DbContext/schema 與端點（模組化單體）。
IModule[] modules = [new MdmModule()];
foreach (var module in modules)
    module.Register(builder.Services, builder.Configuration);

// 目前操作者/租戶來源（Phase 1 iam 會以 JWT/HttpContext 取代 SystemCurrentUser）。
builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

const string corsPolicy = "spa";
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options => options.AddPolicy(corsPolicy, policy =>
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

foreach (var module in modules)
    module.MapEndpoints(app);

app.Run();
