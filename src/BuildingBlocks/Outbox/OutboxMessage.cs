namespace InvenFlow.BuildingBlocks.Outbox;

/// <summary>
/// 交易性 Outbox（對齊 docs/02 §2.1）。單體階段即先建，與業務變更同交易寫入；
/// 日後拆分時由背景派送器投遞至訊息匯流排。<c>EventId</c> 為消費端冪等鍵。
/// </summary>
public sealed class OutboxMessage
{
    public long Id { get; set; }
    public Guid EventId { get; set; } = Guid.NewGuid();
    public long CompanyId { get; set; }
    public string AggregateType { get; set; } = default!;
    public long AggregateId { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}
