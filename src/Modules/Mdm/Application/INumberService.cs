namespace InvenFlow.Modules.Mdm.Application;

/// <summary>
/// 單據編號服務。供其他模組（procure/sales/fin…）經 in-process 介面取號。
/// </summary>
public interface INumberService
{
    Task<string> NextAsync(string key, CancellationToken ct = default);
}
