using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalonMesasOperadorNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FkIdMesa",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FkIdOperador",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mesas",
                columns: table => new
                {
                    IdMesa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas", x => x.IdMesa);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FkIdUsuario = table.Column<int>(type: "integer", nullable: false),
                    FkIdPedido = table.Column<int>(type: "integer", nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_notificaciones_pedidos_FkIdPedido",
                        column: x => x.FkIdPedido,
                        principalTable: "pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notificaciones_usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdMesa",
                table: "pedidos",
                column: "FkIdMesa");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdOperador",
                table: "pedidos",
                column: "FkIdOperador");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_Numero",
                table: "mesas",
                column: "Numero",
                unique: true);

            var seedNow = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc);
            for (var n = 1; n <= 10; n++)
            {
                migrationBuilder.InsertData(
                    table: "mesas",
                    columns: new[] { "Numero", "Estado", "CreatedAt", "UpdatedAt" },
                    values: new object[] { n, "Disponible", seedNow, seedNow });
            }

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_Fecha",
                table: "notificaciones",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_FkIdPedido",
                table: "notificaciones",
                column: "FkIdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_FkIdUsuario",
                table: "notificaciones",
                column: "FkIdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_mesas_FkIdMesa",
                table: "pedidos",
                column: "FkIdMesa",
                principalTable: "mesas",
                principalColumn: "IdMesa",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_usuarios_FkIdOperador",
                table: "pedidos",
                column: "FkIdOperador",
                principalTable: "usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_mesas_FkIdMesa",
                table: "pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_usuarios_FkIdOperador",
                table: "pedidos");

            migrationBuilder.DropTable(
                name: "mesas");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_FkIdMesa",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_FkIdOperador",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "FkIdMesa",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "FkIdOperador",
                table: "pedidos");
        }
    }
}
