using FinFlow.Domain.Enums;

namespace FinFlow.Api.GraphQL.TenantSettings;

// ─── Query payloads ───
public sealed record BrandingPayload(
    string? LogoUrl,
    string? FaviconUrl,
    string? PrimaryColor,
    string? CompanyDisplayName,
    string Locale,
    string Timezone);

public sealed record ApprovalPolicyPayload(
    decimal AutoApproveThreshold,
    decimal EscalationThreshold,
    string EscalationApproverRole,
    bool RequireDifferentApprover,
    int MaxApprovalAgeHours,
    bool IsEscalationEnabled);

public sealed record BudgetPolicyPayload(
    string DefaultEnforcementMode,
    decimal DefaultCarryOverPercent,
    decimal WarningThreshold1,
    decimal WarningThreshold2);

public sealed record ReimbursementPolicyPayload(
    decimal MaxClaimAmount,
    decimal ReceiptRequiredAbove);

public sealed record NotificationPreferencesPayload(
    bool EmailDigestEnabled,
    string EmailDigestFrequency);

public sealed record TenantSettingsPayload(
    Guid Id,
    BrandingPayload Branding,
    ApprovalPolicyPayload ApprovalPolicy,
    BudgetPolicyPayload BudgetPolicy,
    ReimbursementPolicyPayload ReimbursementPolicy,
    NotificationPreferencesPayload NotificationPreferences,
    DateTime UpdatedAt);

// ─── Mutation inputs ───
public sealed record UpdateBrandingInput(
    string? LogoUrl,
    string? FaviconUrl,
    string? PrimaryColor,
    string? CompanyDisplayName,
    string? Locale,
    string? Timezone);

public sealed record UpdateApprovalPolicyInput(
    decimal? AutoApproveThreshold,
    decimal? EscalationThreshold,
    RoleType? EscalationApproverRole,
    bool? RequireDifferentApprover,
    int? MaxApprovalAgeHours,
    bool? IsEscalationEnabled);

public sealed record UpdateBudgetPolicyInput(
    BudgetEnforcementMode? DefaultEnforcementMode,
    decimal? DefaultCarryOverPercent,
    decimal? WarningThreshold1,
    decimal? WarningThreshold2);

public sealed record UpdateReimbursementPolicyInput(
    decimal? MaxClaimAmount,
    decimal? ReceiptRequiredAbove);

public sealed record UpdateNotificationPreferencesInput(
    bool? EmailDigestEnabled,
    string? EmailDigestFrequency);
