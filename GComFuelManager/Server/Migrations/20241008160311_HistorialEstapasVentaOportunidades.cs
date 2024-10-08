using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class HistorialEstapasVentaOportunidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRMRoles_CRMDivisiones_DivisionId",
                table: "CRMRoles");

            migrationBuilder.DropIndex(
                name: "IX_CRMRoles_DivisionId",
                table: "CRMRoles");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "CRMRoles");

            migrationBuilder.CreateTable(
                name: "CRMOportunidadEstadoHistoriales",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OportunidadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMOportunidadEstadoHistoriales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMOportunidadEstadoHistoriales_CRMOportunidades_OportunidadId",
                        column: x => x.OportunidadId,
                        principalTable: "CRMOportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidadEstadoHistoriales_OportunidadId",
                table: "CRMOportunidadEstadoHistoriales",
                column: "OportunidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CRMOportunidadEstadoHistoriales");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "CRMRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CRMRoles_DivisionId",
                table: "CRMRoles",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMRoles_CRMDivisiones_DivisionId",
                table: "CRMRoles",
                column: "DivisionId",
                principalTable: "CRMDivisiones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
