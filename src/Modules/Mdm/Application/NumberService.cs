using InvenFlow.Modules.Mdm.Domain;
using InvenFlow.Modules.Mdm.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Modules.Mdm.Application;

/// <summary>
/// 以「每 (Key, Period) 一列計數器 + 交易遞增 + 單次 SaveChanges」產號，
/// 取代既有 <c>Count()+1</c>（併發/作廢會重號）。SQL Server 上之後可加 RowVersion +
/// EnableRetryOnFailure 強化高併發；批號規則（字軌）後續由設定驅動。
/// </summary>
public sealed class NumberService : INumberService
{
    private readonly MdmDbContext _db;

    public NumberService(MdmDbContext db) => _db = db;

    public async Task<string> NextAsync(string key, CancellationToken ct = default)
    {
        var period = DateTime.UtcNow.ToString("yyyyMMdd");
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var seq = await _db.NumberSequences
                .FirstOrDefaultAsync(s => s.Key == key && s.Period == period, ct);

            if (seq is null)
            {
                seq = new NumberSequence { Key = key, Period = period, NextValue = 1 };
                _db.NumberSequences.Add(seq);
            }

            var value = seq.NextValue;
            seq.NextValue = value + 1;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return $"{key}{period}{value:D4}";
        });
    }
}
