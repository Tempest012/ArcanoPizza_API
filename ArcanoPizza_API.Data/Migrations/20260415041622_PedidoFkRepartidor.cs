using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PedidoFkRepartidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FkIdRepartidor",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdRepartidor",
                table: "pedidos",
                column: "FkIdRepartidor");

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_usuarios_FkIdRepartidor",
                table: "pedidos",
                column: "FkIdRepartidor",
                principalTable: "usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_usuarios_FkIdRepartidor",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_FkIdRepartidor",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "FkIdRepartidor",
                table: "pedidos");
        }
    }
}
