using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodstoreApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementProviderReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "settlement_lines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_reference",
                table: "settlement_lines",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE settlement_lines AS line
                SET provider = intent.provider,
                    provider_reference = intent.provider_reference
                FROM payment_intents AS intent
                WHERE intent.id = line.payment_intent_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provider",
                table: "settlement_lines");

            migrationBuilder.DropColumn(
                name: "provider_reference",
                table: "settlement_lines");
        }
    }
}
