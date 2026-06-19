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
        var cs = configuration.GetConnectionString("Mdm") ?? "Data Source=invenflow_mdm.db";
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<MdmDbContext>((sp, opt) =>
            opt.UseSqlite(cs)
               .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
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
}
