using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class NewRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('c7360b46-fec3-4048-960a-4becfe3a0aea','CierrePedido','CIERREPEDIDO')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('a263bd63-df12-4636-b71a-356af4b4a7c5','Usuarios','USUARIOS')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('e6002565-2863-4c5b-bf4f-fbbfe460c22b','AsignarContacto','ASIGNARCONTACTO')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('78e58f40-3a9f-4b56-be2c-5c46d880e642','AsignarCliDest','ASIGNARCLIDEST')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('2b83c966-9568-4c87-bfb7-069dd97aff90','AsignarUnidad','ASIGNARUNIDAD')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('d10d5038-eb42-4cd5-a4ad-8d21599d3995','EditarOrden','EDITARORDEN')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('5ef3cfe8-a45f-4fab-b2eb-380e3527ba5b','EnviarOrdenSyn','ENVIARORDENSYN')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('89aee781-9408-423d-a6c4-ac0089eedac5','MostrarOrdExp','MOSTRARORDEXP')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('301a7197-c6ce-4132-a0e8-a7f5f8143e8a','ExpHistorial','EXPHISTORIAL')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('13277418-855b-42a1-a376-8fc71b757f14','Eta','ETA')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('89cdfef1-4ace-4c28-bdf6-973de73858f2','EtaReal','ETAREAL')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('731cf4e4-f011-4ed6-a14c-5a3bc3d5541c','ExpEta','EXPETA')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('e480ab6d-f49a-414e-a2f9-f71870ffc5d4','ActTransp','ACTTRANSP')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('673330ad-dbec-4246-a33d-872ca1d16fdd','ActOp','ACTOP')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('fa1ecdf5-14d1-4027-b1ef-8cf34aefd2fc','ActUni','ACTUNI')");

            migrationBuilder.Sql(@"insert into AspNetRoles (Id,Name,NormalizedName)
            values('15a2a442-126c-4a5e-bcf5-a1067a9dd7e0','ActCliDest','ACTCLIDEST')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete AspNetRoles where Id = 'c7360b46-fec3-4048-960a-4becfe3a0aea'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'a263bd63-df12-4636-b71a-356af4b4a7c5'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'e6002565-2863-4c5b-bf4f-fbbfe460c22b'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '78e58f40-3a9f-4b56-be2c-5c46d880e642'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '2b83c966-9568-4c87-bfb7-069dd97aff90'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'd10d5038-eb42-4cd5-a4ad-8d21599d3995'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '5ef3cfe8-a45f-4fab-b2eb-380e3527ba5b'");
            migrationBuilder.Sql("delete AsPNetRoles where Id = '89aee781-9408-423d-a6c4-ac0089eedac5'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '301a7197-c6ce-4132-a0e8-a7f5f8143e8a'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '13277418-855b-42a1-a376-8fc71b757f14'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '89cdfef1-4ace-4c28-bdf6-973de73858f2'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '731cf4e4-f011-4ed6-a14c-5a3bc3d5541c'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'e480ab6d-f49a-414e-a2f9-f71870ffc5d4'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '673330ad-dbec-4246-a33d-872ca1d16fdd'");
            migrationBuilder.Sql("delete AspNetRoles where Id = 'fa1ecdf5-14d1-4027-b1ef-8cf34aefd2fc'");
            migrationBuilder.Sql("delete AspNetRoles where Id = '15a2a442-126c-4a5e-bcf5-a1067a9dd7e0'");
        }
    }
}
