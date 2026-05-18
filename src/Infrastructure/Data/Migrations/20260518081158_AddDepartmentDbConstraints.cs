using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentDbConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // D-4: Unique (tenant, name) for active departments — case-insensitive.
            // Partial index ensures we only enforce uniqueness among active rows;
            // deactivated departments can share names freely.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_department_tenant_name_active
                ON department (id_tenant, lower(name))
                WHERE is_active = true;
                """);

            // Helper unique index required for the composite FK below.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_department_id_tenant
                ON department ("Id", id_tenant);
                """);

            // Same-tenant parent FK — prevents cross-tenant parent references.
            migrationBuilder.Sql("""
                ALTER TABLE department
                ADD CONSTRAINT fk_department_parent_same_tenant
                FOREIGN KEY (parent_id, id_tenant) REFERENCES department("Id", id_tenant);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE department DROP CONSTRAINT IF EXISTS fk_department_parent_same_tenant;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_department_id_tenant;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_department_tenant_name_active;");
        }
    }
}
