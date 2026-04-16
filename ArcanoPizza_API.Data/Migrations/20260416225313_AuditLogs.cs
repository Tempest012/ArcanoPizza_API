using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    IdAuditLog = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OcurrioEn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FkIdUsuario = table.Column<int>(type: "integer", nullable: true),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    MetodoHttp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Ruta = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CodigoEstado = table.Column<int>(type: "integer", nullable: true),
                    Detalle = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.IdAuditLog);
                    table.ForeignKey(
                        name: "FK_audit_logs_usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_FkIdUsuario",
                table: "audit_logs",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_OcurrioEn",
                table: "audit_logs",
                column: "OcurrioEn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
