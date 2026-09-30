using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FplPlanner.Domain.Common;
using FplPlanner.Service.Interface;

namespace FplPlanner.Web.Interceptor;

// Fills the audit fields of every BaseAuditableEntity just before EF saves it.
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var user = _currentUser.GetUserName();

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = user;
                entry.Entity.DateCreated = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedBy = user;
                entry.Entity.DateLastModified = now;
            }
        }
    }
}
