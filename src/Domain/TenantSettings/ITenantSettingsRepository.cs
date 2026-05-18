namespace FinFlow.Domain.TenantSettings;

public interface ITenantSettingsRepository
{
    Task<Entities.TenantSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Entities.TenantSettings?> GetByTenantIdForUpdateAsync(Guid tenantId, CancellationToken cancellationToken = default);
    void Add(Entities.TenantSettings settings);
    void Update(Entities.TenantSettings settings);
}
