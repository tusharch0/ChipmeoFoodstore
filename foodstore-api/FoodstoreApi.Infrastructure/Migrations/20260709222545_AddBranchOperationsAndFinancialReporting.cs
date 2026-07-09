using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchOperationsAndFinancialReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kitchen_routing",
                table: "branches",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "opening_hours",
                table: "branches",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_settings",
                table: "branches",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "report_access_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_export = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_access_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_access_logs_organization_id_created_at",
                table: "report_access_logs",
                columns: new[] { "organization_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "report_access_logs");

            migrationBuilder.DropColumn(
                name: "kitchen_routing",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "opening_hours",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "tax_settings",
                table: "branches");
        }
    }
}
