using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class Cierres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
                values('97b2812c-c866-4919-b44b-b4229442fe3a','Cierre Pedidos','CIERRE PEDIDOS')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '97b2812c-c866-4919-b44b-b4229442fe3a'");
        }
    }
}
