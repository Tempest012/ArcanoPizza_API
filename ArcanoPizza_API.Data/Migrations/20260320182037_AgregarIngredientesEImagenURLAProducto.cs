using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIngredientesEImagenURLAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenURL",
                table: "productos",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredientes",
                table: "productos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenURL",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Ingredientes",
                table: "productos");
        }
    }
}
