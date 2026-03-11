using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArcanoPizza_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class EsquemaCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_producto",
                columns: table => new
                {
                    IdCategoriasProductos = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_producto", x => x.IdCategoriasProductos);
                });

            migrationBuilder.CreateTable(
                name: "extras",
                columns: table => new
                {
                    IdExtra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extras", x => x.IdExtra);
                });

            migrationBuilder.CreateTable(
                name: "tamanos_pizza",
                columns: table => new
                {
                    IdPizza = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModificadorPrecio = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tamanos_pizza", x => x.IdPizza);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecioBase = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FkIdCategoria = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.IdProducto);
                    table.ForeignKey(
                        name: "FK_productos_categorias_producto_FkIdCategoria",
                        column: x => x.FkIdCategoria,
                        principalTable: "categorias_producto",
                        principalColumn: "IdCategoriasProductos",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "direcciones",
                columns: table => new
                {
                    IdDireccion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Calle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Colonia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoPostal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FkIdUsuario = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_direcciones", x => x.IdDireccion);
                    table.ForeignKey(
                        name: "FK_direcciones_usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    IdPedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TipoEntrega = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Impuestos = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FkIdDireccion = table.Column<int>(type: "integer", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.IdPedido);
                    table.ForeignKey(
                        name: "FK_pedidos_direcciones_FkIdDireccion",
                        column: x => x.FkIdDireccion,
                        principalTable: "direcciones",
                        principalColumn: "IdDireccion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Proveedor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProveedorPagoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FkIdPedido = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_pagos_pedidos_FkIdPedido",
                        column: x => x.FkIdPedido,
                        principalTable: "pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedidos_item",
                columns: table => new
                {
                    IdPedidoItem = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FkIdPedido = table.Column<int>(type: "integer", nullable: false),
                    FkIdProducto = table.Column<int>(type: "integer", nullable: false),
                    FkIdTamanoPizza = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos_item", x => x.IdPedidoItem);
                    table.ForeignKey(
                        name: "FK_pedidos_item_pedidos_FkIdPedido",
                        column: x => x.FkIdPedido,
                        principalTable: "pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_item_productos_FkIdProducto",
                        column: x => x.FkIdProducto,
                        principalTable: "productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_item_tamanos_pizza_FkIdTamanoPizza",
                        column: x => x.FkIdTamanoPizza,
                        principalTable: "tamanos_pizza",
                        principalColumn: "IdPizza",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedidos_item_extras",
                columns: table => new
                {
                    IdPedidoItemExtra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FkIdPedidoItem = table.Column<int>(type: "integer", nullable: false),
                    FkIdExtra = table.Column<int>(type: "integer", nullable: false),
                    PrecioExtra = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos_item_extras", x => x.IdPedidoItemExtra);
                    table.ForeignKey(
                        name: "FK_pedidos_item_extras_extras_FkIdExtra",
                        column: x => x.FkIdExtra,
                        principalTable: "extras",
                        principalColumn: "IdExtra",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_item_extras_pedidos_item_FkIdPedidoItem",
                        column: x => x.FkIdPedidoItem,
                        principalTable: "pedidos_item",
                        principalColumn: "IdPedidoItem",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_FkIdUsuario",
                table: "direcciones",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_FkIdPedido",
                table: "pagos",
                column: "FkIdPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdDireccion",
                table: "pedidos",
                column: "FkIdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_FkIdUsuario",
                table: "pedidos",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_item_FkIdPedido",
                table: "pedidos_item",
                column: "FkIdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_item_FkIdProducto",
                table: "pedidos_item",
                column: "FkIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_item_FkIdTamanoPizza",
                table: "pedidos_item",
                column: "FkIdTamanoPizza");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_item_extras_FkIdExtra",
                table: "pedidos_item_extras",
                column: "FkIdExtra");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_item_extras_FkIdPedidoItem",
                table: "pedidos_item_extras",
                column: "FkIdPedidoItem");

            migrationBuilder.CreateIndex(
                name: "IX_productos_FkIdCategoria",
                table: "productos",
                column: "FkIdCategoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "pedidos_item_extras");

            migrationBuilder.DropTable(
                name: "extras");

            migrationBuilder.DropTable(
                name: "pedidos_item");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "tamanos_pizza");

            migrationBuilder.DropTable(
                name: "direcciones");

            migrationBuilder.DropTable(
                name: "categorias_producto");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
