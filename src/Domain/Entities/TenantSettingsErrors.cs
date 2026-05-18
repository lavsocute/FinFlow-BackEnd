using FinFlow.Domain.Abstractions;

namespace FinFlow.Domain.Entities;

public static class TenantSettingsErrors
{
    public static readonly Error NotFound = new("TenantSettings.NotFound", "Tenant settings not found.");
    public static readonly Error LogoUrlTooLong = new("TenantSettings.LogoUrlTooLong", "Logo URL cannot exceed 500 characters.");
    public static readonly Error FaviconUrlTooLong = new("TenantSettings.FaviconUrlTooLong", "Favicon URL cannot exceed 500 characters.");
    public static readonly Error InvalidColor = new("TenantSettings.InvalidColor", "Primary color must be a valid hex color (e.g. #2563EB).");
    public static readonly Error CompanyDisplayNameTooLong = new("TenantSettings.CompanyDisplayNameTooLong", "Company display name cannot exceed 150 characters.");
    public static readonly Error InvalidLocale = new("TenantSettings.InvalidLocale", "Locale must be a valid BCP-47 tag (max 10 chars).");
    public static readonly Error InvalidTimezone = new("TenantSettings.InvalidTimezone", "Timezone identifier is invalid or too long.");
    public static readonly Error ThresholdNegative = new("TenantSettings.ThresholdNegative", "Threshold values cannot be negative.");
    public static readonly Error ThresholdOutOfRange = new("TenantSettings.ThresholdOutOfRange", "Threshold must be between 0 and 100.");
    public static readonly Error InvalidApprovalAge = new("TenantSettings.InvalidApprovalAge", "Max approval age must be at least 1 hour.");
    public static readonly Error InvalidEscalationRole = new("TenantSettings.InvalidEscalationRole", "Escalation approver must be TenantAdmin or Accountant.");
    public static readonly Error CarryOverOutOfRange = new("TenantSettings.CarryOverOutOfRange", "Carry-over percent must be between 0 and 100.");
    public static readonly Error InvalidDigestFrequency = new("TenantSettings.InvalidDigestFrequency", "Email digest frequency must be 'daily', 'weekly', or 'off'.");
}
