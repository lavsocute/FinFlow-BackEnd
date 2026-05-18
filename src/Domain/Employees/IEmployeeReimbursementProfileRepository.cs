namespace FinFlow.Domain.Employees;

public interface IEmployeeReimbursementProfileRepository
{
    /// <summary>
    /// Find profile by membership. Returns null if not yet created
    /// (employee hasn't visited their settings page yet).
    /// </summary>
    Task<EmployeeReimbursementProfile?> GetByMembershipIdAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked load for update operations. Caller MUST be operating in the
    /// owning tenant scope (enforced by EF query filter).
    /// </summary>
    Task<EmployeeReimbursementProfile?> GetByIdForUpdateAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-load profiles for a list of memberships. Used by batch operations
    /// (e.g. CSV export) that need to validate bank info for many employees at once.
    /// AsNoTracking.
    /// </summary>
    Task<IReadOnlyList<EmployeeReimbursementProfile>> GetByMembershipIdsAsync(
        IReadOnlyList<Guid> membershipIds,
        CancellationToken cancellationToken = default);

    void Add(EmployeeReimbursementProfile profile);
    void Update(EmployeeReimbursementProfile profile);
}
