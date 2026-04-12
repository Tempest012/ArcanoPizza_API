using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations;

/// <inheritdoc />
public partial class PedidoMetodoPago : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MetodoPago",
            table: "pedidos",
            type: "character varying(40)",
            maxLength: 40,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MetodoPago",
            table: "pedidos");
    }
}
