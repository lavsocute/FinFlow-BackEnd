using FinFlow.Application.Employees.DTOs;
using FinFlow.Domain.Employees;

namespace FinFlow.Application.Employees.Mapping;

internal static class ReimbursementProfileMapper
{
    public static ReimbursementProfileResponse ToResponse(EmployeeReimbursementProfile profile) => new(
        MembershipId: profile.MembershipId,
        BankCode: profile.BankCode,
        BankName: profile.BankCode is null ? null : VietnamBanks.Find(profile.BankCode)?.Name,
        BankAccountLast4: profile.BankAccountLast4,
        BankAccountHolderName: profile.BankAccountHolderName,
        BankBranch: profile.BankBranch,
        PreferredPaymentMethod: profile.PreferredPaymentMethod?.ToString(),
        ContactPhone: profile.ContactPhone,
        ReimbursementEmail: profile.ReimbursementEmail,
        TaxId: profile.TaxId,
        HasBankInfo: profile.HasBankInfo,
        UpdatedAt: profile.UpdatedAt);

    public static ReimbursementProfileSummaryResponse ToSummary(EmployeeReimbursementProfile profile) => new(
        MembershipId: profile.MembershipId,
        BankCode: profile.BankCode,
        BankName: profile.BankCode is null ? null : VietnamBanks.Find(profile.BankCode)?.Name,
        BankAccountLast4: profile.BankAccountLast4,
        HasBankInfo: profile.HasBankInfo,
        PreferredPaymentMethod: profile.PreferredPaymentMethod?.ToString());
}
