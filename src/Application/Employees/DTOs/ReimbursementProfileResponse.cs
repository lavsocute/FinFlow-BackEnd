namespace FinFlow.Application.Employees.DTOs;

/// <summary>
/// Self-view of profile. Bank account number is masked (last 4 only). Used by
/// employee viewing their own profile and by manager/accountant in summary list view.
/// </summary>
public sealed record ReimbursementProfileResponse(
    Guid MembershipId,
    string? BankCode,
    string? BankName,
    string? BankAccountLast4,
    string? BankAccountHolderName,
    string? BankBranch,
    string? PreferredPaymentMethod,
    string? ContactPhone,
    string? ReimbursementEmail,
    string? TaxId,
    bool HasBankInfo,
    DateTime UpdatedAt);

/// <summary>
/// Accountant-only payload exposing the decrypted bank account number for CSV export.
/// EVERY read of this DTO must be audited.
/// </summary>
public sealed record ReimbursementProfilePayoutResponse(
    Guid MembershipId,
    string BankCode,
    string BankName,
    string BankAccountNumber,
    string BankAccountHolderName,
    string? BankBranch);

/// <summary>
/// Summary list item — last 4 only, no decryption needed. For Manager/Accountant
/// list views (vd: "ai chưa setup bank để chuyển khoản?").
/// </summary>
public sealed record ReimbursementProfileSummaryResponse(
    Guid MembershipId,
    string? BankCode,
    string? BankName,
    string? BankAccountLast4,
    bool HasBankInfo,
    string? PreferredPaymentMethod);

public sealed record OtpDispatchResponse(Guid ChallengeId, int CooldownSeconds);
