using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchScopedOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_discounts_code",
                table: "discounts");

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "payment_settings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "menu_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "discounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "combos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "addons",
                type: "uuid",
                nullable: true);

            // Preserve legacy single-restaurant data by assigning unscoped operational
            // configuration to the earliest branch. Multi-branch installations can then
            // move/copy records explicitly through the branch administration UI.
            migrationBuilder.Sql("""
                DO $$
                DECLARE default_branch uuid;
                BEGIN
                    SELECT id INTO default_branch FROM branches ORDER BY created_at, id LIMIT 1;
                    IF default_branch IS NOT NULL THEN
                        UPDATE categories SET branch_id = default_branch WHERE branch_id IS NULL;
                        UPDATE menu_items SET branch_id = default_branch WHERE branch_id IS NULL;
                        UPDATE addons SET branch_id = default_branch WHERE branch_id IS NULL;
                        UPDATE combos SET branch_id = default_branch WHERE branch_id IS NULL;
                        UPDATE discounts SET branch_id = default_branch WHERE branch_id IS NULL;
                        UPDATE payment_settings SET branch_id = default_branch WHERE branch_id IS NULL;
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_report_access_logs_branch_id",
                table: "report_access_logs",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_settings_branch_id",
                table: "payment_settings",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_branch_id",
                table: "menu_items",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_branch_id_code",
                table: "discounts",
                columns: new[] { "branch_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_combos_branch_id",
                table: "combos",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_branch_id",
                table: "categories",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_addons_branch_id",
                table: "addons",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_addons_branches_branch_id",
                table: "addons",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_branches_branch_id",
                table: "categories",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_combos_branches_branch_id",
                table: "combos",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_discounts_branches_branch_id",
                table: "discounts",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_branches_branch_id",
                table: "menu_items",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_settings_branches_branch_id",
                table: "payment_settings",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_addons_branches_branch_id",
                table: "addons");

            migrationBuilder.DropForeignKey(
                name: "FK_categories_branches_branch_id",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_combos_branches_branch_id",
                table: "combos");

            migrationBuilder.DropForeignKey(
                name: "FK_discounts_branches_branch_id",
                table: "discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_branches_branch_id",
                table: "menu_items");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_settings_branches_branch_id",
                table: "payment_settings");

            migrationBuilder.DropIndex(
                name: "IX_report_access_logs_branch_id",
                table: "report_access_logs");

            migrationBuilder.DropIndex(
                name: "IX_payment_settings_branch_id",
                table: "payment_settings");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_branch_id",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_discounts_branch_id_code",
                table: "discounts");

            migrationBuilder.DropIndex(
                name: "IX_combos_branch_id",
                table: "combos");

            migrationBuilder.DropIndex(
                name: "IX_categories_branch_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_addons_branch_id",
                table: "addons");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "payment_settings");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "combos");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "addons");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_code",
                table: "discounts",
                column: "code",
                unique: true);
        }
    }
}
