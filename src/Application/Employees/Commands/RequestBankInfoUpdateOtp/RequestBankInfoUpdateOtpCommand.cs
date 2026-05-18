using FinFlow.Application.Common;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;

namespace FinFlow.Application.Employees.Commands.RequestBankInfoUpdateOtp;

public sealed record RequestBankInfoUpdateOtpCommand(
    Guid TenantId,
    Guid AccountId,
    Guid MembershipId
) : ICommand<Result<OtpDispatchResponse>>;
