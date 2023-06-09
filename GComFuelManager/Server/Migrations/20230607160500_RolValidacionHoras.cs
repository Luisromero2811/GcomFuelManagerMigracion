using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class RolValidacionHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
            values('d2936910-e74a-4f1c-aec7-2546c3a00918', 'Autorizador Vigencia Pedidos', 'AUTORIZADOR VIGENCIA PEDIDOS')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'd2936910-e74a-4f1c-aec7-2546c3a00918'");
        }
    }
}
