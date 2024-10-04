using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class EliminacionTablasNoUsadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accion");

            migrationBuilder.DropTable(
                name: "Catalogo_Fijo");

            migrationBuilder.DropTable(
                name: "Cliente_Tad");

            migrationBuilder.DropTable(
                name: "Contacto");

            migrationBuilder.DropTable(
                name: "GrupoUsuario");

            migrationBuilder.DropTable(
                name: "Metas_Vendedor");

            migrationBuilder.DropTable(
                name: "Usuario_Tad");

            migrationBuilder.DropTable(
                name: "Vendedor_Originador");

            migrationBuilder.DropTable(
                name: "Tad");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "Originadores");

            migrationBuilder.DropTable(
                name: "Vendedores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accion",
                columns: table => new
                {
                    Cod = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estatus = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accion", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "Catalogo_Fijo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abreviacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Catalogo = table.Column<short>(type: "smallint", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo_Fijo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrupoUsuario",
                columns: table => new
                {
                    cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codgru = table.Column<int>(type: "int", nullable: false),
                    codusu = table.Column<int>(type: "int", nullable: false),
                    fch = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoUsuario", x => x.cod);
                });

            migrationBuilder.CreateTable(
                name: "Originadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Originadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tad",
                columns: table => new
                {
                    Cod = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CodigoOrdenes = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Den = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Nro = table.Column<short>(type: "smallint", nullable: true),
                    Tipo_Vale = table.Column<int>(type: "int", nullable: true),
                    Ultima_Actualizacion_Catalogo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tad", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "Usuario_Tad",
                columns: table => new
                {
                    Id_Usuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id_Terminal = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario_Tad", x => new { x.Id_Usuario, x.Id_Terminal });
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Originador = table.Column<int>(type: "int", nullable: false),
                    Id_Vendedor = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CodCte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codforpag = table.Column<int>(type: "int", nullable: true),
                    Codgru = table.Column<short>(type: "smallint", nullable: true),
                    Codsyn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Codtad = table.Column<short>(type: "smallint", nullable: true),
                    Codusu = table.Column<int>(type: "int", nullable: true),
                    Con = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Consecutivo = table.Column<int>(type: "int", nullable: true),
                    Den = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Esenergas = table.Column<bool>(type: "bit", nullable: true),
                    Id_Tad = table.Column<short>(type: "smallint", nullable: true),
                    Identificador_Externo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MdVenta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tipven = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    precioSemanal = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Cod);
                    table.ForeignKey(
                        name: "FK_Cliente_Originadores_Id_Originador",
                        column: x => x.Id_Originador,
                        principalTable: "Originadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Vendedores_Id_Vendedor",
                        column: x => x.Id_Vendedor,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Metas_Vendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Mes = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Meta = table.Column<double>(type: "float", nullable: true),
                    Referencia = table.Column<double>(type: "float", nullable: true),
                    Venta_Real = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas_Vendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metas_Vendedor_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendedor_Originador",
                columns: table => new
                {
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    OriginadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedor_Originador", x => new { x.VendedorId, x.OriginadorId });
                    table.ForeignKey(
                        name: "FK_Vendedor_Originador_Originadores_OriginadorId",
                        column: x => x.OriginadorId,
                        principalTable: "Originadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendedor_Originador_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente_Tad",
                columns: table => new
                {
                    Id_Cliente = table.Column<int>(type: "int", nullable: false),
                    Id_Terminal = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente_Tad", x => new { x.Id_Cliente, x.Id_Terminal });
                    table.ForeignKey(
                        name: "FK_Cliente_Tad_Cliente_Id_Cliente",
                        column: x => x.Id_Cliente,
                        principalTable: "Cliente",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Tad_Tad_Id_Terminal",
                        column: x => x.Id_Terminal,
                        principalTable: "Tad",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contacto",
                columns: table => new
                {
                    Cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodCte = table.Column<int>(type: "int", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacto", x => x.Cod);
                    table.ForeignKey(
                        name: "FK_Contacto_Cliente_CodCte",
                        column: x => x.CodCte,
                        principalTable: "Cliente",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Id_Originador",
                table: "Cliente",
                column: "Id_Originador");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Id_Vendedor",
                table: "Cliente",
                column: "Id_Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Tad_Id_Terminal",
                table: "Cliente_Tad",
                column: "Id_Terminal");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_CodCte",
                table: "Contacto",
                column: "CodCte");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_Vendedor_VendedorId",
                table: "Metas_Vendedor",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedor_Originador_OriginadorId",
                table: "Vendedor_Originador",
                column: "OriginadorId");
        }
    }
}
