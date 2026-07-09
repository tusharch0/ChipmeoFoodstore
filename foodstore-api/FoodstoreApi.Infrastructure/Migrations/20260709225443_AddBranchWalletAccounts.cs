using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchWalletAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ledger_accounts_organization_id_code_currency",
                table: "ledger_accounts");

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "ledger_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ledger_accounts account
                SET branch_id = (
                    SELECT branch.id
                    FROM branches branch
                    WHERE branch.organization_id = account.organization_id
                    ORDER BY branch.created_at, branch.id
                    LIMIT 1
                )
                WHERE account.code = 'RESTAURANT_WALLET'
                  AND account.organization_id IS NOT NULL
                  AND account.branch_id IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ledger_accounts_organization_id_branch_id_code_currency",
                table: "ledger_accounts",
                columns: new[] { "organization_id", "branch_id", "code", "currency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ledger_accounts_organization_id_branch_id_code_currency",
                table: "ledger_accounts");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "ledger_accounts");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_accounts_organization_id_code_currency",
                table: "ledger_accounts",
                columns: new[] { "organization_id", "code", "currency" },
                unique: true);
        }
    }
}
