namespace InvenFlow.BuildingBlocks.Auditing;

/// <summary>
/// 稽核軌跡（對齊 docs/02 §3.7）。預計由 EF SaveChanges 攔截器寫入（後續任務），
/// 與業務交易同源以避免稽核遺失。
/// </summary>
public sealed class AuditLog
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? Before { get; set; }
    public string? After { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
}
