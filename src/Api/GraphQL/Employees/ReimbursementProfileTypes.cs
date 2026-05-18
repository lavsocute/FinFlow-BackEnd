namespace FinFlow.Api.GraphQL.Employees;

public sealed class ReimbursementProfilePayload
{
    public Guid MembershipId { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountLast4 { get; set; }
    public string? BankAccountHolderName { get; set; }
    public string? BankBranch { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? ContactPhone { get; set; }
    public string? ReimbursementEmail { get; set; }
    public string? TaxId { get; set; }
    public bool HasBankInfo { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static ReimbursementProfilePayload? FromResponse(
        Application.Employees.DTOs.ReimbursementProfileResponse? response)
    {
        if (response is null) return null;
        return new ReimbursementProfilePayload
        {
            MembershipId = response.MembershipId,
            BankCode = response.BankCode,
            BankName = response.BankName,
            BankAccountLast4 = response.BankAccountLast4,
            BankAccountHolderName = response.BankAccountHolderName,
            BankBranch = response.BankBranch,
            PreferredPaymentMethod = response.PreferredPaymentMethod,
            ContactPhone = response.ContactPhone,
            ReimbursementEmail = response.ReimbursementEmail,
            TaxId = response.TaxId,
            HasBankInfo = response.HasBankInfo,
            UpdatedAt = response.UpdatedAt
        };
    }
}

public sealed class ReimbursementProfilePayoutPayload
{
    public Guid MembershipId { get; set; }
    public string BankCode { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string BankAccountNumber { get; set; } = null!;
    public string BankAccountHolderName { get; set; } = null!;
    public string? BankBranch { get; set; }

    public static ReimbursementProfilePayoutPayload FromResponse(
        Application.Employees.DTOs.ReimbursementProfilePayoutResponse response) => new()
    {
        MembershipId = response.MembershipId,
        BankCode = response.BankCode,
        BankName = response.BankName,
        BankAccountNumber = response.BankAccountNumber,
        BankAccountHolderName = response.BankAccountHolderName,
        BankBranch = response.BankBranch
    };
}

public sealed class BankCodePayload
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

public sealed class OtpDispatchPayload
{
    public Guid ChallengeId { get; set; }
    public int CooldownSeconds { get; set; }
}

public sealed record UpdateMyReimbursementProfileInput(
    string? PreferredPaymentMethod,
    string? ContactPhone,
    string? ReimbursementEmail,
    string? TaxId);

public sealed record RequestBankInfoUpdateOtpInput();

public sealed record ConfirmBankInfoUpdateInput(
    Guid ChallengeId,
    string Otp,
    string? BankCode,
    string? BankAccountNumber,
    string? BankAccountHolderName,
    string? BankBranch);
