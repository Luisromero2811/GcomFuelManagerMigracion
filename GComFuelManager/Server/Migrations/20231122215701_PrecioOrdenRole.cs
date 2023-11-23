using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class PrecioOrdenRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('881d7bcc-2341-433b-9437-609a9b241813','Revision Precio','REVISION PRECIO')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('55d29d6a-da57-4166-ba2b-cce0b5a7b0a9','Lectura Asignacion','LECTURA ASIGNACION')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '881d7bcc-2341-433b-9437-609a9b241813'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '55d29d6a-da57-4166-ba2b-cce0b5a7b0a9'");
        }
    }
}
