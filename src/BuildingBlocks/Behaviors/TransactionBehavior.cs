using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.BuildingBlocks.Behaviors;

/// <summary>
/// 在單一 DB 交易內執行命令並「只提交一次」（含業務變更 + Outbox + 稽核），
/// 結構性消除既有「多次 SaveChanges、無 BeginTransaction」的帳實不符風險（docs/03 §一致性一）。
/// Handler 內不應自行 <c>SaveChanges</c>；由本行為統一存檔。
///
/// 每個模組以一個薄子類別把 <typeparamref name="TContext"/> 綁定到自己的 DbContext，
/// 並以模組標記介面約束 <typeparamref name="TRequest"/>，避免跨模組誤套（Phase 1 起套用）。
/// </summary>
public class TransactionBehavior<TRequest, TResponse, TContext> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TContext : DbContext
{
    private readonly TContext _db;

    public TransactionBehavior(TContext db) => _db = db;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 已在交易中（巢狀）則直接執行，由最外層提交。
        if (_db.Database.CurrentTransaction is not null)
            return await next();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var response = await next();
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        });
    }
}
