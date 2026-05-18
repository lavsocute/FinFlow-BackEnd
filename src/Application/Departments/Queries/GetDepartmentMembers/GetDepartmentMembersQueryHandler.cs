using FinFlow.Application.Common;
using FinFlow.Application.Departments.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Accounts;
using FinFlow.Domain.Departments;
using FinFlow.Domain.TenantMemberships;

namespace FinFlow.Application.Departments.Queries.GetDepartmentMembers;

public sealed class GetDepartmentMembersQueryHandler : IQueryHandler<GetDepartmentMembersQuery, Result<IReadOnlyList<MemberDto>>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly IAccountRepository _accountRepository;

    public GetDepartmentMembersQueryHandler(
        IDepartmentRepository departmentRepository,
        ITenantMembershipRepository membershipRepository,
        IAccountRepository accountRepository)
    {
        _departmentRepository = departmentRepository;
        _membershipRepository = membershipRepository;
        _accountRepository = accountRepository;
    }

    public async Task<Result<IReadOnlyList<MemberDto>>> Handle(GetDepartmentMembersQuery request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null || department.IdTenant != request.TenantId)
            return Result.Failure<IReadOnlyList<MemberDto>>(Domain.Entities.DepartmentErrors.NotFound);

        var memberships = await _membershipRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        var departmentMemberships = memberships
            .Where(m => m.DepartmentId == request.DepartmentId)
            .ToList();

        if (departmentMemberships.Count == 0)
            return Result.Success<IReadOnlyList<MemberDto>>([]);

        // Batch-load accounts in a single round-trip instead of N+1.
        var accountIds = departmentMemberships.Select(m => m.AccountId).Distinct().ToList();
        var accounts = await _accountRepository.GetByIdsAsync(accountIds, cancellationToken);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var memberDtos = new List<MemberDto>(departmentMemberships.Count);
        foreach (var membership in departmentMemberships)
        {
            if (accountMap.TryGetValue(membership.AccountId, out var account))
            {
                memberDtos.Add(new MemberDto(
                    membership.Id,
                    account.Email,
                    membership.Role.ToString(),
                    membership.IsActive));
            }
        }

        return Result.Success((IReadOnlyList<MemberDto>)memberDtos);
    }
}
