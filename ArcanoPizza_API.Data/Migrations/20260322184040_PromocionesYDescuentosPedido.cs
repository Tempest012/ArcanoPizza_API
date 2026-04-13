using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromocionesYDescuentosPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoTotal",
                table: "pedidos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FkIdPromocion",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "promociones",
                columns: table => new
                {
                    IdPromocion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImagenURL = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PrecioOriginal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PrecioPromocional = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TipoVigencia = table.Column<int>(type: "integer", nullable: false),
                    FechaValidaHasta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DiaSemanaRecurrente = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promociones", x => x.IdPromocion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdPromocion",
                table: "pedidos",
                column: "FkIdPromocion");

            migrationBuilder.CreateIndex(
                name: "IX_promociones_Activo",
                table: "promociones",
                column: "Activo");

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_promociones_FkIdPromocion",
                table: "pedidos",
                column: "FkIdPromocion",
                principalTable: "promociones",
                principalColumn: "IdPromocion",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_promociones_FkIdPromocion",
                table: "pedidos");

            migrationBuilder.DropTable(
                name: "promociones");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_FkIdPromocion",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "DescuentoTotal",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "FkIdPromocion",
                table: "pedidos");
        }
    }
}
