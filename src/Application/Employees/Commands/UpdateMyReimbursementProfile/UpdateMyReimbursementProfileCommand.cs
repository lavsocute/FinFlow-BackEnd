using FinFlow.Application.Common;
using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Expenses;

namespace FinFlow.Application.Employees.Commands.UpdateMyReimbursementProfile;

public sealed record UpdateMyReimbursementProfileCommand(
    Guid TenantId,
    Guid MembershipId,
    PaymentMethod? PreferredPaymentMethod,
    string? ContactPhone,
    string? ReimbursementEmail,
    string? TaxId
) : ICommand<Result<ReimbursementProfileResponse>>;
