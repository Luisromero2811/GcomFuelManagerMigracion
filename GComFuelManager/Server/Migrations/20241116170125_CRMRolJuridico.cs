using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class CRMRolJuridico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('201a556a-221b-4384-b5a4-f7f012134558','VER_DOCUMENTOS_JURIDICO','VER_DOCUMENTOS_JURIDICO')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('7c627fb9-c199-42fc-8c60-f32dc261b91b','VER_DETALLE_DOCUMENTO_JURIDICO','VER_DETALLE_DOCUMENTO_JURIDICO')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('c6d40936-2164-434f-bc80-eb536e442fd6','SUBIR_CORRECCION_DOCUMENTO','SUBIR_CORRECCION_DOCUMENTO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '201a556a-221b-4384-b5a4-f7f012134558'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '7c627fb9-c199-42fc-8c60-f32dc261b91b'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'c6d40936-2164-434f-bc80-eb536e442fd6'");
        }
    }
}
