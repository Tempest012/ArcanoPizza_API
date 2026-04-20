using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromocionProductoCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FkIdProductoCombo",
                table: "promociones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_promociones_FkIdProductoCombo",
                table: "promociones",
                column: "FkIdProductoCombo");

            migrationBuilder.AddForeignKey(
                name: "FK_promociones_productos_FkIdProductoCombo",
                table: "promociones",
                column: "FkIdProductoCombo",
                principalTable: "productos",
                principalColumn: "IdProducto",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_promociones_productos_FkIdProductoCombo",
                table: "promociones");

            migrationBuilder.DropIndex(
                name: "IX_promociones_FkIdProductoCombo",
                table: "promociones");

            migrationBuilder.DropColumn(
                name: "FkIdProductoCombo",
                table: "promociones");
        }
    }
}
