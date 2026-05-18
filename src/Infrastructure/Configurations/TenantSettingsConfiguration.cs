using FinFlow.Domain.Entities;
using FinFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinFlow.Infrastructure.Configurations;

internal sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("tenant_settings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.IdTenant).HasColumnName("id_tenant").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.Version).HasColumnName("xmin").IsRowVersion();

        // ─── Branding ───
        builder.Property(x => x.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        builder.Property(x => x.FaviconUrl).HasColumnName("favicon_url").HasMaxLength(500);
        builder.Property(x => x.PrimaryColor).HasColumnName("primary_color").HasMaxLength(7);
        builder.Property(x => x.CompanyDisplayName).HasColumnName("company_display_name").HasMaxLength(150);
        builder.Property(x => x.Locale).HasColumnName("locale").HasMaxLength(10).HasDefaultValue("vi-VN").IsRequired();
        builder.Property(x => x.Timezone).HasColumnName("timezone").HasMaxLength(50).HasDefaultValue("Asia/Ho_Chi_Minh").IsRequired();

        // ─── Approval Policy ───
        builder.Property(x => x.AutoApproveThreshold).HasColumnName("auto_approve_threshold").HasDefaultValue(0m);
        builder.Property(x => x.EscalationThreshold).HasColumnName("escalation_threshold").HasDefaultValue(999_999_999m);
        builder.Property(x => x.EscalationApproverRole).HasColumnName("escalation_approver_role").HasConversion<string>().HasMaxLength(20).HasDefaultValue(RoleType.TenantAdmin);
        builder.Property(x => x.RequireDifferentApprover).HasColumnName("require_different_approver").HasDefaultValue(true);
        builder.Property(x => x.MaxApprovalAgeHours).HasColumnName("max_approval_age_hours").HasDefaultValue(72);
        builder.Property(x => x.IsEscalationEnabled).HasColumnName("is_escalation_enabled").HasDefaultValue(false);

        // ─── Budget Policy ───
        builder.Property(x => x.DefaultEnforcementMode).HasColumnName("default_enforcement_mode").HasConversion<string>().HasMaxLength(20).HasDefaultValue(BudgetEnforcementMode.SoftBlock);
        builder.Property(x => x.DefaultCarryOverPercent).HasColumnName("default_carry_over_percent").HasDefaultValue(0m);
        builder.Property(x => x.WarningThreshold1).HasColumnName("warning_threshold_1").HasDefaultValue(85m);
        builder.Property(x => x.WarningThreshold2).HasColumnName("warning_threshold_2").HasDefaultValue(95m);

        // ─── Reimbursement Policy ───
        builder.Property(x => x.MaxClaimAmount).HasColumnName("max_claim_amount").HasDefaultValue(0m);
        builder.Property(x => x.ReceiptRequiredAbove).HasColumnName("receipt_required_above").HasDefaultValue(0m);

        // ─── Notification Preferences ───
        builder.Property(x => x.EmailDigestEnabled).HasColumnName("email_digest_enabled").HasDefaultValue(false);
        builder.Property(x => x.EmailDigestFrequency).HasColumnName("email_digest_frequency").HasMaxLength(20).HasDefaultValue("daily");

        // ─── Indexes ───
        builder.HasIndex(x => x.IdTenant).IsUnique();
        builder.HasOne<Tenant>().WithOne().HasForeignKey<TenantSettings>(x => x.IdTenant).OnDelete(DeleteBehavior.Cascade);
    }
}
