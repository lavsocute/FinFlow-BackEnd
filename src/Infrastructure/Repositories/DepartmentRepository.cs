using FinFlow.Domain.Departments;
using FinFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinFlow.Infrastructure.Repositories;

internal sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DepartmentRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<DepartmentSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentSummary(d.Id, d.Name, d.IdTenant, d.ParentId, d.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<DepartmentSummary>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Select(d => new DepartmentSummary(d.Id, d.Name, d.IdTenant, d.ParentId, d.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<DepartmentSummary?> GetDefaultByTenantIdAsync(Guid idTenant, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(d => d.IdTenant == idTenant && d.IsActive)
            .OrderBy(d => d.ParentId.HasValue)   // root first
            .ThenBy(d => d.CreatedAt)
            .Select(d => new DepartmentSummary(d.Id, d.Name, d.IdTenant, d.ParentId, d.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<DepartmentSummary>> GetByTenantIdAsync(Guid idTenant, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .AsNoTracking()
            .Where(d => d.IdTenant == idTenant)
            .Select(d => new DepartmentSummary(d.Id, d.Name, d.IdTenant, d.ParentId, d.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(Guid Id, Guid? ParentId)>> GetParentMapAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Pull (id, parent_id) for ALL departments in tenant — including
        // inactive — because the cycle-detection walk should stop only on
        // null parent, not on activity gates.
        var rows = await _dbContext.Set<Department>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(d => d.IdTenant == tenantId)
            .Select(d => new { d.Id, d.ParentId })
            .ToListAsync(cancellationToken);
        return rows.Select(r => (r.Id, r.ParentId)).ToList();
    }

    public Task<bool> HasActiveChildrenAsync(Guid parentId, Guid tenantId, CancellationToken cancellationToken = default) =>
        _dbContext.Set<Department>()
            .AsNoTracking()
            .AnyAsync(d => d.ParentId == parentId && d.IdTenant == tenantId && d.IsActive, cancellationToken);

    public Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeDepartmentId = null, CancellationToken cancellationToken = default)
    {
        var trimmed = (name ?? string.Empty).Trim().ToLower();
        return _dbContext.Set<Department>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(d =>
                    d.IdTenant == tenantId
                    && d.IsActive
                    && d.Name.ToLower() == trimmed
                    && (excludeDepartmentId == null || d.Id != excludeDepartmentId),
                cancellationToken);
    }

    public async Task<Department?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Department?> GetEntityByIdIncludingInactiveAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id && d.IdTenant == tenantId, cancellationToken);

    public void Add(Department department) => _dbContext.Set<Department>().Add(department);
    public void Update(Department department) => _dbContext.Set<Department>().Update(department);
    public void Remove(Department department) => _dbContext.Set<Department>().Remove(department);
}
