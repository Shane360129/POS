using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvenFlow.BuildingBlocks.Modules;

/// <summary>
/// 模組化單體的模組契約。每個模組自行註冊服務（含自有 DbContext / schema）與端點，
/// 模組間僅經 in-process 介面與領域事件互動（不跨模組直接 JOIN）。
/// 日後要抽離為獨立服務時，介面與端點契約可平移。
/// </summary>
public interface IModule
{
    string Name { get; }

    IServiceCollection Register(IServiceCollection services, IConfiguration configuration);

    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>啟動時初始化（建立資料庫結構 / 種子）。預設不做事。</summary>
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
