namespace InvenFlow.BuildingBlocks.Abstractions;

/// <summary>
/// 目前操作者與租戶（公司）來源。Phase 1（iam）會以 JWT/HttpContext 實作；
/// 在此之前以 <see cref="SystemCurrentUser"/> 作為預設（系統公司）。
/// </summary>
public interface ICurrentUser
{
    long CompanyId { get; }
    long? UserId { get; }
}

public sealed class SystemCurrentUser : ICurrentUser
{
    public long CompanyId => 1;
    public long? UserId => null;
}
