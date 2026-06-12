using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WedNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStripeToZarinPal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripeSessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "StripeSessionId",
                table: "Payments",
                newName: "Authority");

            migrationBuilder.AddColumn<string>(
                name: "RefId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Authority",
                table: "Payments",
                column: "Authority",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_Authority",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Authority",
                table: "Payments",
                newName: "StripeSessionId");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripeSessionId",
                table: "Payments",
                column: "StripeSessionId");
        }
    }
}
