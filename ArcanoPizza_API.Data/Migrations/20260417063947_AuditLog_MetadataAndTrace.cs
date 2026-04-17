using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditLog_MetadataAndTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuracionMs",
                table: "audit_logs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                table: "audit_logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "audit_logs",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CodigoEstado",
                table: "audit_logs",
                column: "CodigoEstado");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Nivel",
                table: "audit_logs",
                column: "Nivel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_logs_CodigoEstado",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_Nivel",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "DuracionMs",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "TraceId",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "audit_logs");
        }
    }
}
