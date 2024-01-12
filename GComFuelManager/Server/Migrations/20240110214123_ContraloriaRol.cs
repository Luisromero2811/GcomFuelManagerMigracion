using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class ContraloriaRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id, Name, NormalizedName)
            values('567cad67-5452-47bb-b254-01d4fde4cfbe','Contraloria','CONTRALORIA')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = '567cad67-5452-47bb-b254-01d4fde4cfbe'");
        }
    }
}
