using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations;

/// <inheritdoc />
public partial class PedidoDireccionOpcionalRecoger : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_pedidos_direcciones_FkIdDireccion",
            table: "pedidos");

        migrationBuilder.AlterColumn<int>(
            name: "FkIdDireccion",
            table: "pedidos",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AddForeignKey(
            name: "FK_pedidos_direcciones_FkIdDireccion",
            table: "pedidos",
            column: "FkIdDireccion",
            principalTable: "direcciones",
            principalColumn: "IdDireccion",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_pedidos_direcciones_FkIdDireccion",
            table: "pedidos");

        migrationBuilder.AlterColumn<int>(
            name: "FkIdDireccion",
            table: "pedidos",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_pedidos_direcciones_FkIdDireccion",
            table: "pedidos",
            column: "FkIdDireccion",
            principalTable: "direcciones",
            principalColumn: "IdDireccion",
            onDelete: ReferentialAction.Restrict);
    }
}
