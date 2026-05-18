using FinFlow.Domain.Abstractions;

namespace FinFlow.Domain.Employees;

public static class ProfileErrors
{
    public static readonly Error NotFound =
        new("Profile.NotFound", "Reimbursement profile not found.");

    public static readonly Error TenantRequired =
        new("Profile.TenantRequired", "Tenant ID is required.");

    public static readonly Error MembershipRequired =
        new("Profile.MembershipRequired", "Membership ID is required.");

    public static readonly Error UnsupportedBankCode =
        new("Profile.UnsupportedBankCode", "Bank code is not in the supported list.");

    public static readonly Error AccountNumberInvalidLength =
        new("Profile.AccountNumberInvalidLength", "Bank account number must be 6-20 digits.");

    public static readonly Error AccountNumberRequired =
        new("Profile.AccountNumberRequired", "Bank account number is required when bank code is provided.");

    public static readonly Error HolderNameRequired =
        new("Profile.HolderNameRequired", "Account holder name is required when bank info is provided.");

    public static readonly Error HolderNameTooLong =
        new("Profile.HolderNameTooLong", "Account holder name cannot exceed 200 characters.");

    public static readonly Error BranchTooLong =
        new("Profile.BranchTooLong", "Bank branch cannot exceed 200 characters.");

    public static readonly Error PhoneInvalid =
        new("Profile.PhoneInvalid", "Phone number must be a valid Vietnamese mobile number.");

    public static readonly Error EmailInvalid =
        new("Profile.EmailInvalid", "Reimbursement email is invalid.");

    public static readonly Error TaxIdInvalid =
        new("Profile.TaxIdInvalid", "Tax ID must be 10 or 13 digits.");

    public static readonly Error BankEditRateLimited =
        new("Profile.BankEditRateLimited", "Too many bank info changes. Please try again later.");
}
