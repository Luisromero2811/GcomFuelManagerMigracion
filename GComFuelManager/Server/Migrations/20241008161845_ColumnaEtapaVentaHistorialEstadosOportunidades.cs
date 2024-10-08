using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class ColumnaEtapaVentaHistorialEstadosOportunidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidadEstadoHistoriales_EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales",
                column: "EtapaVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMOportunidadEstadoHistoriales_CRMCatalogoValores_EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales",
                column: "EtapaVentaId",
                principalTable: "CRMCatalogoValores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRMOportunidadEstadoHistoriales_CRMCatalogoValores_EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales");

            migrationBuilder.DropIndex(
                name: "IX_CRMOportunidadEstadoHistoriales_EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales");

            migrationBuilder.DropColumn(
                name: "EtapaVentaId",
                table: "CRMOportunidadEstadoHistoriales");
        }
    }
}
