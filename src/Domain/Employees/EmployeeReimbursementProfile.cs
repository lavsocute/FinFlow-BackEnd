using System.Text.RegularExpressions;
using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Expenses;
using FinFlow.Domain.Interfaces;

namespace FinFlow.Domain.Employees;

/// <summary>
/// Per-membership reimbursement profile carrying the bank account that the company
/// transfers to when paying back the employee, plus contact preferences.
///
/// Lifecycle:
///  1. Created lazily on first edit — caller passes empty bank info if just storing
///     non-bank fields (preferred method, phone, etc.).
///  2. Bank info changes are PII-sensitive — domain only accepts already-encrypted
///     ciphertext. Encryption happens in the application layer via
///     <c>IPiiEncryptionService</c>.
///  3. Account number plaintext is never stored. Only the encrypted blob and the
///     last 4 digits (for display) are persisted.
/// </summary>
public sealed class EmployeeReimbursementProfile : Entity, IMultiTenant
{
    private const int MaxHolderNameLength = 200;
    private const int MaxBranchLength = 200;

    // VN mobile: leading 0 or +84, followed by 9 or 10 digits.
    private static readonly Regex VietnamMobileRegex =
        new(@"^(\+84|0)[0-9]{9,10}$", RegexOptions.Compiled);

    // VN personal MST: 10 digits or 13 digits (10 + dash + 3 sometimes; we strip dash).
    private static readonly Regex TaxIdRegex =
        new(@"^[0-9]{10}([0-9]{3})?$", RegexOptions.Compiled);

    private EmployeeReimbursementProfile(
        Guid id,
        Guid idTenant,
        Guid membershipId,
        DateTime createdAt)
    {
        Id = id;
        IdTenant = idTenant;
        MembershipId = membershipId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private EmployeeReimbursementProfile() { }

    public Guid IdTenant { get; private set; }
    public Guid MembershipId { get; private set; }

    public string? BankCode { get; private set; }
    public byte[]? BankAccountNumberEncrypted { get; private set; }
    public string? BankAccountLast4 { get; private set; }
    public string? BankAccountHolderName { get; private set; }
    public string? BankBranch { get; private set; }

    public PaymentMethod? PreferredPaymentMethod { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ReimbursementEmail { get; private set; }
    public string? TaxId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>Concurrency token (xmin) — guards against lost updates.</summary>
    public uint Version { get; private set; }

    public bool HasBankInfo =>
        BankAccountNumberEncrypted is { Length: > 0 } && !string.IsNullOrWhiteSpace(BankCode);

    public static Result<EmployeeReimbursementProfile> Create(Guid idTenant, Guid membershipId)
    {
        if (idTenant == Guid.Empty)
            return Result.Failure<EmployeeReimbursementProfile>(ProfileErrors.TenantRequired);
        if (membershipId == Guid.Empty)
            return Result.Failure<EmployeeReimbursementProfile>(ProfileErrors.MembershipRequired);

        return Result.Success(new EmployeeReimbursementProfile(
            Guid.NewGuid(), idTenant, membershipId, DateTime.UtcNow));
    }

    /// <summary>
    /// Replace bank info atomically. Pass null/empty for all 5 args to clear the bank info
    /// (employee removed their bank). Otherwise all five must be present and consistent.
    ///
    /// <paramref name="encryptedAccountNumber"/> must already be ciphertext from
    /// <c>IPiiEncryptionService.Encrypt</c>; domain never sees plaintext.
    /// <paramref name="last4"/> is the last 4 digits of the original plaintext for
    /// display purposes — the caller derives this before calling.
    /// </summary>
    public Result UpdateBankInfo(
        string? bankCode,
        byte[]? encryptedAccountNumber,
        string? last4,
        string? holderName,
        string? branch)
    {
        var allEmpty = string.IsNullOrWhiteSpace(bankCode)
            && (encryptedAccountNumber is null || encryptedAccountNumber.Length == 0)
            && string.IsNullOrWhiteSpace(holderName);

        if (allEmpty)
        {
            // Clear path
            BankCode = null;
            BankAccountNumberEncrypted = null;
            BankAccountLast4 = null;
            BankAccountHolderName = null;
            BankBranch = null;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        // All-or-nothing: bank info must be complete or empty.
        if (string.IsNullOrWhiteSpace(bankCode))
            return Result.Failure(ProfileErrors.UnsupportedBankCode);

        var normalizedCode = bankCode.Trim().ToUpperInvariant();
        if (!VietnamBanks.IsValidCode(normalizedCode))
            return Result.Failure(ProfileErrors.UnsupportedBankCode);

        if (encryptedAccountNumber is null || encryptedAccountNumber.Length == 0)
            return Result.Failure(ProfileErrors.AccountNumberRequired);

        if (string.IsNullOrWhiteSpace(last4) || last4.Length != 4)
            return Result.Failure(ProfileErrors.AccountNumberInvalidLength);

        var trimmedHolder = holderName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedHolder))
            return Result.Failure(ProfileErrors.HolderNameRequired);
        if (trimmedHolder.Length > MaxHolderNameLength)
            return Result.Failure(ProfileErrors.HolderNameTooLong);

        var trimmedBranch = branch?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedBranch) && trimmedBranch.Length > MaxBranchLength)
            return Result.Failure(ProfileErrors.BranchTooLong);

        BankCode = normalizedCode;
        BankAccountNumberEncrypted = encryptedAccountNumber;
        BankAccountLast4 = last4;
        BankAccountHolderName = trimmedHolder;
        BankBranch = string.IsNullOrWhiteSpace(trimmedBranch) ? null : trimmedBranch;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Update preferences and contact metadata. Does NOT touch bank fields — those
    /// require <see cref="UpdateBankInfo"/> behind an OTP-gated mutation.
    /// </summary>
    public Result UpdateContactInfo(
        PaymentMethod? preferredMethod,
        string? contactPhone,
        string? reimbursementEmail,
        string? taxId)
    {
        var trimmedPhone = NormalizeOrNull(contactPhone);
        if (trimmedPhone is not null && !VietnamMobileRegex.IsMatch(trimmedPhone))
            return Result.Failure(ProfileErrors.PhoneInvalid);

        var trimmedEmail = NormalizeOrNull(reimbursementEmail);
        if (trimmedEmail is not null && !IsValidEmail(trimmedEmail))
            return Result.Failure(ProfileErrors.EmailInvalid);

        var trimmedTaxId = NormalizeOrNull(taxId)?.Replace("-", string.Empty);
        if (trimmedTaxId is not null && !TaxIdRegex.IsMatch(trimmedTaxId))
            return Result.Failure(ProfileErrors.TaxIdInvalid);

        PreferredPaymentMethod = preferredMethod;
        ContactPhone = trimmedPhone;
        ReimbursementEmail = trimmedEmail;
        TaxId = trimmedTaxId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static string? NormalizeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }
}
