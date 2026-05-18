using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tenant = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    favicon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    primary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    company_display_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "vi-VN"),
                    timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Asia/Ho_Chi_Minh"),
                    auto_approve_threshold = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    escalation_threshold = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 999999999m),
                    escalation_approver_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "TenantAdmin"),
                    require_different_approver = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    max_approval_age_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 72),
                    is_escalation_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    default_enforcement_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "SoftBlock"),
                    default_carry_over_percent = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    warning_threshold_1 = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 85m),
                    warning_threshold_2 = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 95m),
                    max_claim_amount = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    receipt_required_above = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    email_digest_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_digest_frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "daily")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_settings_tenant_id_tenant",
                        column: x => x.id_tenant,
                        principalTable: "tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_id_tenant",
                table: "tenant_settings",
                column: "id_tenant",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_settings");
        }
    }
}
