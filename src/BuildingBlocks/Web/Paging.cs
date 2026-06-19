namespace InvenFlow.BuildingBlocks.Web;

/// <summary>伺服器端分頁請求。PageSize 夾在 1..200。</summary>
public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Math.Max(Page, 1) - 1) * Math.Clamp(PageSize, 1, 200);
    public int Take => Math.Clamp(PageSize, 1, 200);
}

/// <summary>分頁結果。</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
