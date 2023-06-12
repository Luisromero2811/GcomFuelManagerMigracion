using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class NewVigenciaRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
            values('8223408e-0935-48ad-a3c3-d11d9a76af66','Autorizador Vigencia Pedidos Completo','AUTORIZADOR VIGENCIA COMPLETO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '8223408e-0935-48ad-a3c3-d11d9a76af66'");
        }
    }
}
