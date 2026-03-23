using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePromocionPorcentajeDescuento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PorcentajeDescuento",
                table: "promociones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeDescuento",
                table: "promociones",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }
    }
}
