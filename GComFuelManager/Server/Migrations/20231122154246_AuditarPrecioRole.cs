using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AuditarPrecioRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('b981efa8-cd4f-4f61-9054-2c25f418f7f7','Revision Precios', 'REVISION PRECIOS')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'b981efa8-cd4f-4f61-9054-2c25f418f7f7'");
        }
    }
}
