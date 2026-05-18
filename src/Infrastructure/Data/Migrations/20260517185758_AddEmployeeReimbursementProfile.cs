using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeReimbursementProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee_reimbursement_profile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tenant = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    bank_account_number_encrypted = table.Column<byte[]>(type: "bytea", nullable: true),
                    bank_account_last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    bank_account_holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    preferred_payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reimbursement_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_reimbursement_profile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_tenant",
                table: "employee_reimbursement_profile",
                column: "id_tenant");

            migrationBuilder.CreateIndex(
                name: "uq_profile_membership",
                table: "employee_reimbursement_profile",
                column: "membership_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_reimbursement_profile");
        }
    }
}
