using InvenFlow.BuildingBlocks.Auditing;
using InvenFlow.BuildingBlocks.Modules;
using InvenFlow.BuildingBlocks.Outbox;
using InvenFlow.Modules.Mdm.Application;
using InvenFlow.Modules.Mdm.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvenFlow.Modules.Mdm;

public sealed class MdmModule : IModule
{
    public string Name => "mdm";

    public IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var cs = configuration.GetConnectionString("Mdm") ?? "Data Source=invenflow_mdm.db";

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<MdmDbContext>((sp, opt) =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                opt.UseSqlServer(cs, sql => sql.EnableRetryOnFailure());
            else
                opt.UseSqlite(cs);
            opt.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<INumberService, NumberService>();
        services.AddScoped<IOutboxWriter, OutboxWriter<MdmDbContext>>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mdm").WithTags("mdm");

        // 取下一個單據號（示範端點，亦供煙霧測試）。
        group.MapGet("/numbers/{key}", async (string key, INumberService numbers, CancellationToken ct) =>
            Results.Ok(new { number = await numbers.NextAsync(key, ct) }));

        return endpoints;
    }

    public async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // 骨架：建立結構（EnsureCreated）。正式環境改用 EF migrations（待本機 SDK 後產生）。
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MdmDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
