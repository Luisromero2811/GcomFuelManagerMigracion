using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class CuentaOperativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
                values('32f0ebde-438e-4e85-8a0c-4e3534041d16','Ejecutivo de Cuenta Operativo','EJECUTIVO DE CUENTA OPERATIVO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '32f0ebde-438e-4e85-8a0c-4e3534041d16'");
        }
    }
}
