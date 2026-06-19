using System.Text.Json;
using InvenFlow.BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InvenFlow.BuildingBlocks.Auditing;

/// <summary>
/// 與業務交易同源的稽核攔截器（對齊 docs/02 §3.7、docs/03 §稽核）。
/// 在 SaveChanges 前，把實作 <see cref="IAuditable"/> 的變更實體寫成 <see cref="AuditLog"/>，
/// 與業務變更同交易提交，避免稽核遺失。
/// 註：新增實體在存檔前主鍵可能尚未產生，EntityId 以當下值為準（後續可改 SavedChanges 補正）。
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUser currentUser) => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            var pk = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
            var keyValues = entry.State == EntityState.Deleted ? entry.OriginalValues : entry.CurrentValues;
            var entityId = pk is null ? "" : keyValues[pk]?.ToString() ?? "";

            string? before = null;
            string? after = null;
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                before = JsonSerializer.Serialize(
                    entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]));
            if (entry.State is EntityState.Added or EntityState.Modified)
                after = JsonSerializer.Serialize(
                    entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));

            context.Add(new AuditLog
            {
                CompanyId = _currentUser.CompanyId,
                UserId = _currentUser.UserId,
                Action = action,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entityId,
                Before = before,
                After = after,
                At = DateTime.UtcNow
            });
        }
    }
}
