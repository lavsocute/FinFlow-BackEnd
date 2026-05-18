using FinFlow.Domain.Entities;
using FinFlow.Domain.TenantSettings;
using Microsoft.EntityFrameworkCore;

namespace FinFlow.Infrastructure.Repositories;

internal sealed class TenantSettingsRepository : ITenantSettingsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TenantSettingsRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public Task<TenantSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _dbContext.Set<TenantSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdTenant == tenantId, cancellationToken);

    public Task<TenantSettings?> GetByTenantIdForUpdateAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _dbContext.Set<TenantSettings>()
            .FirstOrDefaultAsync(s => s.IdTenant == tenantId, cancellationToken);

    public void Add(TenantSettings settings) => _dbContext.Set<TenantSettings>().Add(settings);
    public void Update(TenantSettings settings) => _dbContext.Set<TenantSettings>().Update(settings);
}
