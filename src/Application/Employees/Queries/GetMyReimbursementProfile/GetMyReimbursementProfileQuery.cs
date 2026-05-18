using FinFlow.Application.Common;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;

namespace FinFlow.Application.Employees.Queries.GetMyReimbursementProfile;

public sealed record GetMyReimbursementProfileQuery(
    Guid TenantId,
    Guid MembershipId
) : IQuery<Result<ReimbursementProfileResponse?>>;
