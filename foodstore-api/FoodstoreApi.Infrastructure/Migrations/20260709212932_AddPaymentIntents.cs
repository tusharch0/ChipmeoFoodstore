using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIntents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_intents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "KES"),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    checkout_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_intents", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_intents_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payment_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    provider_event_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    signature_valid = table.Column<bool>(type: "boolean", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    processing_outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_events_payment_intents_payment_intent_id",
                        column: x => x.payment_intent_id,
                        principalTable: "payment_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_payment_intent_id",
                table: "payment_events",
                column: "payment_intent_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_processing_outcome",
                table: "payment_events",
                column: "processing_outcome");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_provider_provider_event_id",
                table: "payment_events",
                columns: new[] { "provider", "provider_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_branch_id",
                table: "payment_intents",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_idempotency_key",
                table: "payment_intents",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_order_id",
                table: "payment_intents",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_provider_provider_reference",
                table: "payment_intents",
                columns: new[] { "provider", "provider_reference" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_events");

            migrationBuilder.DropTable(
                name: "payment_intents");
        }
    }
}
