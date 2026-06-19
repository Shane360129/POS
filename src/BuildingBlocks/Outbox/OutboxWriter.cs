using System.Text.Json;
using InvenFlow.BuildingBlocks.Abstractions;
using InvenFlow.BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.BuildingBlocks.Outbox;

public interface IOutboxWriter
{
    void Add(IIntegrationEvent @event, long aggregateId, string aggregateType);
}

/// <summary>
/// 交易性 Outbox 寫入器。把整合事件以 <see cref="OutboxMessage"/> 加入「本模組的」
/// DbContext，與業務變更同交易提交（ADR-03）。<c>EventId</c> 作為消費端冪等鍵。
/// </summary>
public sealed class OutboxWriter<TContext> : IOutboxWriter where TContext : DbContext
{
    private readonly TContext _db;
    private readonly ICurrentUser _currentUser;

    public OutboxWriter(TContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public void Add(IIntegrationEvent @event, long aggregateId, string aggregateType)
    {
        var type = @event.GetType();
        _db.Set<OutboxMessage>().Add(new OutboxMessage
        {
            EventId = @event.EventId,
            CompanyId = _currentUser.CompanyId,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Type = type.FullName ?? type.Name,
            Payload = JsonSerializer.Serialize(@event, type),
            OccurredAt = @event.OccurredAt
        });
    }
}
