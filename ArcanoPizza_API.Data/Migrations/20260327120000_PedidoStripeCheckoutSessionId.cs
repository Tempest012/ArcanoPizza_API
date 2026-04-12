using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PedidoStripeCheckoutSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeCheckoutSessionId",
                table: "pedidos",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_StripeCheckoutSessionId",
                table: "pedidos",
                column: "StripeCheckoutSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pedidos_StripeCheckoutSessionId",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "StripeCheckoutSessionId",
                table: "pedidos");
        }
    }
}
