using FinFlow.Application.Common;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;

namespace FinFlow.Application.Employees.Queries.GetReimbursementProfileForPayout;

/// <summary>
/// Accountant-only payout query that returns the decrypted bank account number.
/// EVERY successful read is recorded by the audit pipeline so that compliance can
/// trace which accountant accessed which employee's bank info.
/// </summary>
public sealed record GetReimbursementProfileForPayoutQuery(
    Guid TenantId,
    Guid AccountantAccountId,
    Guid MembershipId
) : IQuery<Result<ReimbursementProfilePayoutResponse>>;
