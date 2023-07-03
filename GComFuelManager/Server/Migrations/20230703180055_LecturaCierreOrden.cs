using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class LecturaCierreOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
                values('615d3663-83bf-465e-a44a-6ae9295f7a24','Lectura de Cierre de Orden','LECTURA DE CIERRE DE ORDEN')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where '615d3663-83bf-465e-a44a-6ae9295f7a24'");
        }
    }
}
