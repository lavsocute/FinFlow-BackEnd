using FinFlow.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinFlow.Infrastructure.Configurations;

internal sealed class EmployeeReimbursementProfileConfiguration
    : IEntityTypeConfiguration<EmployeeReimbursementProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeReimbursementProfile> builder)
    {
        builder.ToTable("employee_reimbursement_profile");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.IdTenant).HasColumnName("id_tenant").IsRequired();
        builder.Property(x => x.MembershipId).HasColumnName("membership_id").IsRequired();

        builder.Property(x => x.BankCode).HasColumnName("bank_code").HasMaxLength(10);
        builder.Property(x => x.BankAccountNumberEncrypted).HasColumnName("bank_account_number_encrypted").HasColumnType("bytea");
        builder.Property(x => x.BankAccountLast4).HasColumnName("bank_account_last4").HasMaxLength(4);
        builder.Property(x => x.BankAccountHolderName).HasColumnName("bank_account_holder_name").HasMaxLength(200);
        builder.Property(x => x.BankBranch).HasColumnName("bank_branch").HasMaxLength(200);

        builder.Property(x => x.PreferredPaymentMethod)
            .HasColumnName("preferred_payment_method")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
        builder.Property(x => x.ReimbursementEmail).HasColumnName("reimbursement_email").HasMaxLength(200);
        builder.Property(x => x.TaxId).HasColumnName("tax_id").HasMaxLength(14);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(x => x.MembershipId)
            .IsUnique()
            .HasDatabaseName("uq_profile_membership");

        builder.HasIndex(x => x.IdTenant)
            .HasDatabaseName("ix_profile_tenant");
    }
}
