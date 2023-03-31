using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    public partial class AddRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('0c19c0c3-f4b4-4d21-83f5-7003e1016cfa','Admin','ADMIN')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('b3c8990d-a0c3-487a-91e4-8843f025a24e','User','USER')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '0c19c0c3-f4b4-4d21-83f5-7003e1016cfa'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'b3c8990d-a0c3-487a-91e4-8843f025a24e'");
        }
    }
}
