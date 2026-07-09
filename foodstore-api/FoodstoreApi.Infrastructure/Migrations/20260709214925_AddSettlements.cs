using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "minimum_payout_threshold",
                table: "finance_policies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "settlement_calendar",
                table: "finance_policies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "tax_rate",
                table: "finance_policies",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "tax_treatment",
                table: "finance_policies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "settlement_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    period_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gross_sales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    refunds = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    provider_fees = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    commissions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    adjustments = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    holds = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payout_reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    paid_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settlement_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_intent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    provider_fee_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_settlement_lines_settlement_batches_settlement_batch_id",
                        column: x => x.settlement_batch_id,
                        principalTable: "settlement_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_settlement_batches_idempotency_key",
                table: "settlement_batches",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_batches_organization_id_period_date_currency",
                table: "settlement_batches",
                columns: new[] { "organization_id", "period_date", "currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_lines_payment_intent_id",
                table: "settlement_lines",
                column: "payment_intent_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_lines_settlement_batch_id",
                table: "settlement_lines",
                column: "settlement_batch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "settlement_lines");

            migrationBuilder.DropTable(
                name: "settlement_batches");

            migrationBuilder.DropColumn(
                name: "minimum_payout_threshold",
                table: "finance_policies");

            migrationBuilder.DropColumn(
                name: "settlement_calendar",
                table: "finance_policies");

            migrationBuilder.DropColumn(
                name: "tax_rate",
                table: "finance_policies");

            migrationBuilder.DropColumn(
                name: "tax_treatment",
                table: "finance_policies");
        }
    }
}
