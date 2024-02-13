using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class ConsultaOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
            values('b0a08400-d27f-4bb2-a00b-d9fa75c2a96c', 'Consulta Precio Orden', 'CONSULTA PRECIO ORDEN')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'b0a08400-d27f-4bb2-a00b-d9fa75c2a96c'");
        }
    }
}
