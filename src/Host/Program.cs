using InvenFlow.BuildingBlocks.Abstractions;
using InvenFlow.BuildingBlocks.Modules;
using InvenFlow.BuildingBlocks.Web;
using InvenFlow.Modules.Mdm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

// 認證（Keycloak OIDC / JWT Bearer）骨架。Phase 1 起於端點掛 [Authorize]/policy。
var authority = builder.Configuration["Auth:Authority"];
var audience = builder.Configuration["Auth:Audience"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(audience);
    });
builder.Services.AddAuthorization();

// 可觀測性（OpenTelemetry：tracing + metrics）。設定 OTLP endpoint 才匯出。
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("InvenFlow.Host"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

foreach (var module in modules)
    module.MapEndpoints(app);

// 啟動時建立各模組資料庫結構（骨架：EnsureCreated；正式環境改 EF migrations）。
foreach (var module in modules)
    await module.InitializeAsync(app.Services);

app.Run();
