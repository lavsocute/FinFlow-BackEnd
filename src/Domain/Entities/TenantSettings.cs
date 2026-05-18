using FinFlow.Domain.Abstractions;
using FinFlow.Domain.Enums;
using FinFlow.Domain.Interfaces;

namespace FinFlow.Domain.Entities;

public sealed class TenantSettings : Entity, IMultiTenant
{
    private TenantSettings(Guid id, Guid idTenant)
    {
        Id = id;
        IdTenant = idTenant;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private TenantSettings() { }

    public Guid IdTenant { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public uint Version { get; private set; }

    // ─── Branding ───
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? CompanyDisplayName { get; private set; }
    public string Locale { get; private set; } = "vi-VN";
    public string Timezone { get; private set; } = "Asia/Ho_Chi_Minh";

    // ─── Approval Policy ───
    public decimal AutoApproveThreshold { get; private set; }
    public decimal EscalationThreshold { get; private set; } = 999_999_999m;
    public RoleType EscalationApproverRole { get; private set; } = RoleType.TenantAdmin;
    public bool RequireDifferentApprover { get; private set; } = true;
    public int MaxApprovalAgeHours { get; private set; } = 72;
    public bool IsEscalationEnabled { get; private set; }

    // ─── Budget Policy ───
    public BudgetEnforcementMode DefaultEnforcementMode { get; private set; } = BudgetEnforcementMode.SoftBlock;
    public decimal DefaultCarryOverPercent { get; private set; }
    public decimal WarningThreshold1 { get; private set; } = 85m;
    public decimal WarningThreshold2 { get; private set; } = 95m;

    // ─── Reimbursement Policy ───
    public decimal MaxClaimAmount { get; private set; }          // 0 = unlimited
    public decimal ReceiptRequiredAbove { get; private set; }    // 0 = always required

    // ─── Notification Preferences ───
    public bool EmailDigestEnabled { get; private set; }
    public string EmailDigestFrequency { get; private set; } = "daily";

    // ─── Factory ───
    public static TenantSettings CreateDefault(Guid tenantId)
    {
        return new TenantSettings(Guid.NewGuid(), tenantId);
    }

    // ─── Branding Mutations ───
    public Result UpdateBranding(
        string? logoUrl,
        string? faviconUrl,
        string? primaryColor,
        string? companyDisplayName,
        string? locale,
        string? timezone)
    {
        if (logoUrl is not null && logoUrl.Length > 500)
            return Result.Failure(TenantSettingsErrors.LogoUrlTooLong);
        if (faviconUrl is not null && faviconUrl.Length > 500)
            return Result.Failure(TenantSettingsErrors.FaviconUrlTooLong);
        if (primaryColor is not null && !IsValidHexColor(primaryColor))
            return Result.Failure(TenantSettingsErrors.InvalidColor);
        if (companyDisplayName is not null && companyDisplayName.Length > 150)
            return Result.Failure(TenantSettingsErrors.CompanyDisplayNameTooLong);
        if (locale is not null && locale.Length > 10)
            return Result.Failure(TenantSettingsErrors.InvalidLocale);
        if (timezone is not null && timezone.Length > 50)
            return Result.Failure(TenantSettingsErrors.InvalidTimezone);

        LogoUrl = logoUrl ?? LogoUrl;
        FaviconUrl = faviconUrl ?? FaviconUrl;
        PrimaryColor = primaryColor ?? PrimaryColor;
        CompanyDisplayName = companyDisplayName ?? CompanyDisplayName;
        Locale = locale ?? Locale;
        Timezone = timezone ?? Timezone;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Approval Policy Mutations ───
    public Result UpdateApprovalPolicy(
        decimal? autoApproveThreshold,
        decimal? escalationThreshold,
        RoleType? escalationApproverRole,
        bool? requireDifferentApprover,
        int? maxApprovalAgeHours,
        bool? isEscalationEnabled)
    {
        if (autoApproveThreshold.HasValue && autoApproveThreshold.Value < 0)
            return Result.Failure(TenantSettingsErrors.ThresholdNegative);
        if (escalationThreshold.HasValue && escalationThreshold.Value < 0)
            return Result.Failure(TenantSettingsErrors.ThresholdNegative);
        if (maxApprovalAgeHours.HasValue && maxApprovalAgeHours.Value < 1)
            return Result.Failure(TenantSettingsErrors.InvalidApprovalAge);
        if (escalationApproverRole.HasValue &&
            escalationApproverRole.Value != RoleType.TenantAdmin &&
            escalationApproverRole.Value != RoleType.Accountant)
            return Result.Failure(TenantSettingsErrors.InvalidEscalationRole);

        AutoApproveThreshold = autoApproveThreshold ?? AutoApproveThreshold;
        EscalationThreshold = escalationThreshold ?? EscalationThreshold;
        EscalationApproverRole = escalationApproverRole ?? EscalationApproverRole;
        RequireDifferentApprover = requireDifferentApprover ?? RequireDifferentApprover;
        MaxApprovalAgeHours = maxApprovalAgeHours ?? MaxApprovalAgeHours;
        IsEscalationEnabled = isEscalationEnabled ?? IsEscalationEnabled;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Budget Policy Mutations ───
    public Result UpdateBudgetPolicy(
        BudgetEnforcementMode? defaultEnforcementMode,
        decimal? defaultCarryOverPercent,
        decimal? warningThreshold1,
        decimal? warningThreshold2)
    {
        if (defaultCarryOverPercent.HasValue && (defaultCarryOverPercent.Value < 0 || defaultCarryOverPercent.Value > 100))
            return Result.Failure(TenantSettingsErrors.CarryOverOutOfRange);
        if (warningThreshold1.HasValue && (warningThreshold1.Value < 0 || warningThreshold1.Value > 100))
            return Result.Failure(TenantSettingsErrors.ThresholdOutOfRange);
        if (warningThreshold2.HasValue && (warningThreshold2.Value < 0 || warningThreshold2.Value > 100))
            return Result.Failure(TenantSettingsErrors.ThresholdOutOfRange);

        DefaultEnforcementMode = defaultEnforcementMode ?? DefaultEnforcementMode;
        DefaultCarryOverPercent = defaultCarryOverPercent ?? DefaultCarryOverPercent;
        WarningThreshold1 = warningThreshold1 ?? WarningThreshold1;
        WarningThreshold2 = warningThreshold2 ?? WarningThreshold2;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Reimbursement Policy Mutations ───
    public Result UpdateReimbursementPolicy(
        decimal? maxClaimAmount,
        decimal? receiptRequiredAbove)
    {
        if (maxClaimAmount.HasValue && maxClaimAmount.Value < 0)
            return Result.Failure(TenantSettingsErrors.ThresholdNegative);
        if (receiptRequiredAbove.HasValue && receiptRequiredAbove.Value < 0)
            return Result.Failure(TenantSettingsErrors.ThresholdNegative);

        MaxClaimAmount = maxClaimAmount ?? MaxClaimAmount;
        ReceiptRequiredAbove = receiptRequiredAbove ?? ReceiptRequiredAbove;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Notification Preferences Mutations ───
    public Result UpdateNotificationPreferences(
        bool? emailDigestEnabled,
        string? emailDigestFrequency)
    {
        if (emailDigestFrequency is not null &&
            emailDigestFrequency is not "daily" and not "weekly" and not "off")
            return Result.Failure(TenantSettingsErrors.InvalidDigestFrequency);

        EmailDigestEnabled = emailDigestEnabled ?? EmailDigestEnabled;
        EmailDigestFrequency = emailDigestFrequency ?? EmailDigestFrequency;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    private static bool IsValidHexColor(string color) =>
        color.Length == 7 && color[0] == '#' &&
        color[1..].All(c => char.IsAsciiHexDigit(c));
}
