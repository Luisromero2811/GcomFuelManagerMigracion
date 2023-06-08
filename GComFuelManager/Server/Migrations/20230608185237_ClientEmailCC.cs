using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class ClientEmailCC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('2dcf5271-dcc2-4811-8c91-4cb60cce70f7','Correo de Clientes','CORREO DE CLIENTES')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AspNetRoles where Id = '2dcf5271-dcc2-4811-8c91-4cb60cce70f7'");
        }
    }
}
