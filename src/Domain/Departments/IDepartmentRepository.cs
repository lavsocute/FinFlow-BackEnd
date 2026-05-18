namespace FinFlow.Domain.Departments;

public record DepartmentSummary(Guid Id, string Name, Guid IdTenant, Guid? ParentId, bool IsActive);

public interface IDepartmentRepository
{
    // ─── Read (DTO) ───
    Task<DepartmentSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentSummary>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Default department for a tenant — first ACTIVE root, oldest first.
    /// Used during onboarding/registration. Excludes inactive rows so new
    /// data never gets attached to a soft-deleted department.
    /// </summary>
    Task<DepartmentSummary?> GetDefaultByTenantIdAsync(Guid idTenant, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentSummary>> GetByTenantIdAsync(Guid idTenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lightweight (Id, ParentId) pairs for a tenant. Used by cycle detection
    /// in the ChangeParent flow — single SQL round-trip vs walking N entities.
    /// </summary>
    Task<IReadOnlyList<(Guid Id, Guid? ParentId)>> GetParentMapAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveChildrenAsync(Guid parentId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same-tenant uniqueness check for Create/Rename. Case-insensitive match.
    /// </summary>
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeDepartmentId = null, CancellationToken cancellationToken = default);

    // ─── Read (Entity) ───
    Task<FinFlow.Domain.Entities.Department?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load the entity even when it's been soft-deleted. Used by the activate
    /// path which by definition needs to load an inactive row.
    /// </summary>
    Task<FinFlow.Domain.Entities.Department?> GetEntityByIdIncludingInactiveAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    // ─── Write ───
    void Add(FinFlow.Domain.Entities.Department department);
    void Update(FinFlow.Domain.Entities.Department department);
    void Remove(FinFlow.Domain.Entities.Department department);
}
