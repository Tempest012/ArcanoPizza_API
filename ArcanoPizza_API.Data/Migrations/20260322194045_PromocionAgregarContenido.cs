using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromocionAgregarContenido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contenido",
                table: "promociones",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contenido",
                table: "promociones");
        }
    }
}
