using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class NewRolVigenciaAut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
            values('de975e32-997f-49a2-9283-c17922026a17', 'Autorizador Vigencia Completo', 'AUTORIZADOR VIGENCIA COMPLETO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'de975e32-997f-49a2-9283-c17922026a17'");
        }
    }
}
//de975e32-997f-49a2-9283-c17922026a17