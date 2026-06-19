namespace InvenFlow.BuildingBlocks.Messaging;

/// <summary>跨模組整合事件。寫入 Outbox 後由背景派送器（未來）投遞。</summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
