using FinFlow.Application.Common;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;

namespace FinFlow.Application.Employees.Commands.ConfirmBankInfoUpdate;

public sealed record ConfirmBankInfoUpdateCommand(
    Guid TenantId,
    Guid AccountId,
    Guid MembershipId,
    Guid ChallengeId,
    string Otp,
    // null/empty in all 3 = clear bank info path
    string? BankCode,
    string? BankAccountNumber,
    string? BankAccountHolderName,
    string? BankBranch
) : ICommand<Result<ReimbursementProfileResponse>>;
