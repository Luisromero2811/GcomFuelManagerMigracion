using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class OficcialRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('ceb15026-df03-11ed-b5ea-0242ac120002','Administrador Sistema','ADMINISTRADOR SISTEMA')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('da3c724a-df03-11ed-b5ea-0242ac120002','Administrador de Usuarios','ADMINISTRADOR DE USUARIOS')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('e34bd628-df03-11ed-b5ea-0242ac120002','Direccion','DIRECCION')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('e93e687a-df03-11ed-b5ea-0242ac120002','Gerencia','GERENCIA')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('eec24f50-df03-11ed-b5ea-0242ac120002','Gestion de Transporte','GESTION DE TRANSPORTE')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('14ece24e-df04-11ed-b5ea-0242ac120002','Ejecutivo de Cuenta Comercial','EJECUTIVO DE CUENTA COMERCIAL')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('1bcd5774-df04-11ed-b5ea-0242ac120002','Programador','PROGRAMADOR')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('238d126a-df04-11ed-b5ea-0242ac120002','Coordinador','COORDINADOR')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('2a5238c8-df04-11ed-b5ea-0242ac120002','Analista Credito','ANALISTA CREDITO')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('2f16db5c-df04-11ed-b5ea-0242ac120002','Contador','CONTADOR')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('34f261d6-df04-11ed-b5ea-0242ac120002','Analista Suministros','ANALISTA SUMINISTROS')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('3a4f752e-df04-11ed-b5ea-0242ac120002','Auditor','AUDITOR')");
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('4b7c6cc6-df04-11ed-b5ea-0242ac120002','Comprador','COMPRADOR')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'ceb15026-df03-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'da3c724a-df03-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'e34bd628-df03-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'e93e687a-df03-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'eec24f50-df03-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '14ece24e-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '1bcd5774-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '238d126a-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '2a5238c8-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '2f16db5c-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '34f261d6-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '3a4f752e-df04-11ed-b5ea-0242ac120002'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '4b7c6cc6-df04-11ed-b5ea-0242ac120002'");
        }
    }
}
