using FinFlow.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace FinFlow.Infrastructure.Repositories;

internal sealed class EmployeeReimbursementProfileRepository : IEmployeeReimbursementProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmployeeReimbursementProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmployeeReimbursementProfile?> GetByMembershipIdAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        _dbContext.EmployeeReimbursementProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.MembershipId == membershipId, cancellationToken);

    public Task<EmployeeReimbursementProfile?> GetByIdForUpdateAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        _dbContext.EmployeeReimbursementProfiles
            .FirstOrDefaultAsync(p => p.Id == id && p.IdTenant == tenantId, cancellationToken);

    public async Task<IReadOnlyList<EmployeeReimbursementProfile>> GetByMembershipIdsAsync(
        IReadOnlyList<Guid> membershipIds,
        CancellationToken cancellationToken = default) =>
        await _dbContext.EmployeeReimbursementProfiles
            .AsNoTracking()
            .Where(p => membershipIds.Contains(p.MembershipId))
            .ToListAsync(cancellationToken);

    public void Add(EmployeeReimbursementProfile profile) =>
        _dbContext.EmployeeReimbursementProfiles.Add(profile);

    public void Update(EmployeeReimbursementProfile profile) =>
        _dbContext.EmployeeReimbursementProfiles.Update(profile);
}
