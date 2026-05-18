using FinFlow.Domain.Employees;
using FinFlow.Domain.Expenses;
using Xunit;

namespace FinFlow.UnitTests.Domain.Employees;

public class EmployeeReimbursementProfileTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid MembershipId = Guid.NewGuid();
    private static readonly byte[] FakeCipher = new byte[32];

    [Fact]
    public void Create_ValidIds_Succeeds()
    {
        var result = EmployeeReimbursementProfile.Create(TenantId, MembershipId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantId, result.Value.IdTenant);
        Assert.Equal(MembershipId, result.Value.MembershipId);
        Assert.False(result.Value.HasBankInfo);
    }

    [Fact]
    public void Create_EmptyTenant_Fails()
    {
        var result = EmployeeReimbursementProfile.Create(Guid.Empty, MembershipId);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.TenantRequired, result.Error);
    }

    [Fact]
    public void Create_EmptyMembership_Fails()
    {
        var result = EmployeeReimbursementProfile.Create(TenantId, Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.MembershipRequired, result.Error);
    }

    [Fact]
    public void UpdateBankInfo_ValidVcb_Succeeds()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("VCB", FakeCipher, "1234", "NGUYEN VAN A", "Hanoi Branch");

        Assert.True(result.IsSuccess);
        Assert.Equal("VCB", profile.BankCode);
        Assert.Equal(FakeCipher, profile.BankAccountNumberEncrypted);
        Assert.Equal("1234", profile.BankAccountLast4);
        Assert.Equal("NGUYEN VAN A", profile.BankAccountHolderName);
        Assert.Equal("Hanoi Branch", profile.BankBranch);
        Assert.True(profile.HasBankInfo);
    }

    [Fact]
    public void UpdateBankInfo_LowercaseCode_NormalizesToUpper()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("vcb", FakeCipher, "1234", "Holder", null);

        Assert.True(result.IsSuccess);
        Assert.Equal("VCB", profile.BankCode);
    }

    [Fact]
    public void UpdateBankInfo_AllEmpty_ClearsBankInfo()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;
        profile.UpdateBankInfo("VCB", FakeCipher, "1234", "Holder", null);
        Assert.True(profile.HasBankInfo);

        var result = profile.UpdateBankInfo(null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.False(profile.HasBankInfo);
        Assert.Null(profile.BankCode);
        Assert.Null(profile.BankAccountNumberEncrypted);
    }

    [Fact]
    public void UpdateBankInfo_UnknownBankCode_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("XYZ", FakeCipher, "1234", "Holder", null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.UnsupportedBankCode, result.Error);
    }

    [Fact]
    public void UpdateBankInfo_MissingAccountNumber_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("VCB", null, null, "Holder", null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.AccountNumberRequired, result.Error);
    }

    [Fact]
    public void UpdateBankInfo_MissingHolder_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("VCB", FakeCipher, "1234", "  ", null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.HolderNameRequired, result.Error);
    }

    [Fact]
    public void UpdateBankInfo_HolderTooLong_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("VCB", FakeCipher, "1234", new string('A', 201), null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.HolderNameTooLong, result.Error);
    }

    [Fact]
    public void UpdateBankInfo_Last4InvalidLength_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateBankInfo("VCB", FakeCipher, "12", "Holder", null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.AccountNumberInvalidLength, result.Error);
    }

    [Fact]
    public void UpdateContactInfo_ValidPhoneEmailTax_Succeeds()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateContactInfo(
            PaymentMethod.BankTransfer,
            "0912345678",
            "user@finflow.test",
            "1234567890");

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.BankTransfer, profile.PreferredPaymentMethod);
        Assert.Equal("0912345678", profile.ContactPhone);
        Assert.Equal("user@finflow.test", profile.ReimbursementEmail);
        Assert.Equal("1234567890", profile.TaxId);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc123")]
    [InlineData("12-345")]
    public void UpdateContactInfo_InvalidPhone_Fails(string phone)
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateContactInfo(null, phone, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.PhoneInvalid, result.Error);
    }

    [Fact]
    public void UpdateContactInfo_InvalidEmail_Fails()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateContactInfo(null, null, "not-an-email", null);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.EmailInvalid, result.Error);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12345678901")]   // 11 digits — invalid
    public void UpdateContactInfo_InvalidTaxId_Fails(string taxId)
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateContactInfo(null, null, null, taxId);

        Assert.True(result.IsFailure);
        Assert.Equal(ProfileErrors.TaxIdInvalid, result.Error);
    }

    [Fact]
    public void UpdateContactInfo_TaxIdWithDashes_Normalizes()
    {
        var profile = EmployeeReimbursementProfile.Create(TenantId, MembershipId).Value;

        var result = profile.UpdateContactInfo(null, null, null, "1234567890-001");

        Assert.True(result.IsSuccess);
        Assert.Equal("1234567890001", profile.TaxId);
    }

    [Fact]
    public void VietnamBanks_AllListedCodesAreValid()
    {
        foreach (var bank in VietnamBanks.All)
        {
            Assert.True(VietnamBanks.IsValidCode(bank.Code));
            Assert.Equal(bank, VietnamBanks.Find(bank.Code));
        }
    }

    [Fact]
    public void VietnamBanks_UnknownCode_NotValid()
    {
        Assert.False(VietnamBanks.IsValidCode("XYZ"));
        Assert.False(VietnamBanks.IsValidCode(""));
        Assert.False(VietnamBanks.IsValidCode(null));
    }
}
