using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoNuevoRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'01eff2f3-37b8-4de9-9f4e-ee1bb8bb051d', N'VER_MODULO_COMERCIALES', N'VER_MODULO_COMERCIALES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'078e1e55-a68e-4710-904a-b6e2be9a0599', N'CREAR_EQUIPO', N'CREAR_EQUIPO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'07dea0d8-4548-4533-9352-cf99168b937d', N'VER_COMERCIALES', N'VER_COMERCIALES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'096a149c-d609-4b71-adb9-66f659d584db', N'EDITAR_CONTACTO', N'EDITAR_CONTACTO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'12e9f808-0cd6-4502-a37f-0133986c1370', N'ELIMINAR_ROL', N'ELIMINAR_ROL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'1bfe4739-b550-4377-a411-da879cc7f32b', N'EDITAR_VALOR_CATALOGO', N'EDITAR_VALOR_CATALOGO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'1ed10588-8f20-4fd8-93eb-a4bab939716a', N'VER_DETALLE_CONTACTOS', N'VER_DETALLE_CONTACTOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'1f31008a-a76d-4233-b782-c84f705ab300', N'VER_DOCUMENTOS', N'VER_DOCUMENTOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'23369738-f022-42d2-8b88-66b185681c80', N'CREAR_VENDEDOR', N'CREAR_VENDEDOR', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'2525042d-f14a-4fea-bee3-a0f5a8268456', N'VER_DETALLE_CUENTA', N'VER_DETALLE_CUENTA', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'2c57f926-24ac-4ac5-8125-3ed39fe171c3', N'EDITAR_EQUIPO', N'EDITAR_EQUIPO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'32ddad12-52ce-418b-9737-955673c0f685', N'EDITAR_CUENTA', N'EDITAR_CUENTA', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'3b44c0ab-b713-4f94-a6c6-220a73a2bc31', N'CREAR_CONTACTO', N'CREAR_CONTACTO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'3d7c88c0-977c-4bed-bd38-cecc54cd794c', N'ELIMINAR_CONTACTO', N'ELIMINAR_CONTACTO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'400fe836-3386-408d-963a-938ecd73e971', N'VER_DETALLE_VENDEDORES', N'VER_DETALLE_VENDEDORES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'40f8c861-a6a7-47a3-82da-6312d8bb4254', N'VER_EQUIPOS', N'VER_EQUIPOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'43c11e1c-248f-47fd-9716-ff1c8056ed88', N'VER_MODULO_OPORTUNIDADES', N'VER_MODULO_OPORTUNIDADES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'4776679a-bf09-4066-86cd-31431265546f', N'ELIMINAR_OPORTUNIDAD', N'ELIMINAR_OPORTUNIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'4cb2c594-00b3-482b-adc5-457e1548bc30', N'EDITAR_COMERCIAL', N'EDITAR_COMERCIAL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'4ed778ed-c10c-4949-a3c2-01fe240f1899', N'CREAR_COMERCIAL', N'CREAR_COMERCIAL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'567890ab-cdef-ghij-klmn-opqrstuvwx', N'EDITAR_ACTIVIDAD', N'EDITAR_ACTIVIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'59316cb0-c95f-4bc8-8e30-be42897538d2', N'VER_DETALLE_COMERCIAL', N'VER_DETALLE_COMERCIAL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'5f1508a6-943c-4eb2-90fe-8a2738a1e650', N'ELIMINAR_CUENTA', N'ELIMINAR_CUENTA', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'65151529-bf91-4c1b-96b3-4e694e50ccf3', N'ELIMINAR_VENDEDOR', N'ELIMINAR_VENDEDOR', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'694e9d70-c1d8-41e3-86e1-27d8cad478da', N'VER_MODULO_CATALOGOS', N'VER_MODULO_CATALOGOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'6fcebc8c-f58b-4a7c-ae26-04ec68d5dd1d', N'VER_CONTACTOS', N'VER_CONTACTOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'78ff7e81-5440-4229-9474-5f9b0b35c4cc', N'VER_MODULO_VENDEDORES', N'VER_MODULO_VENDEDORES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'87654321-0987-6543-2109-876543210987', N'VER_MODULO_ACTIVIDADES', N'VER_MODULO_ACTIVIDADES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'8973a145-0a93-4a1e-a8e0-82332e13abd2', N'ELIMINAR_EQUIPO', N'ELIMINAR_EQUIPO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'8e177bbf-53d5-4092-bef2-042e89182580', N'ELIMINAR_COMERCIAL', N'ELIMINAR_COMERCIAL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'8feebd0d-2832-437e-8af1-3d57a9c92d7b', N'VER_MODULO_EQUIPOS', N'VER_MODULO_EQUIPOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'94914d80-d8f5-447f-8819-799017fb171b', N'VER_MODULO_VALORES_CATALOGO', N'VER_MODULO_VALORES_CATALOGO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'99cd763e-27a0-442d-9a79-06855dd7546a', N'VER_VALORES_CATALOGO', N'VER_VALORES_CATALOGO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'9db239f2-3de7-4470-b5cd-8d963680032a', N'CREAR_CUENTA', N'CREAR_CUENTA', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'9e44ee3a-1af3-4aac-a9de-1991634bfb52', N'CREAR_VALOR_CATALOGO', N'CREAR_VALOR_CATALOGO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'a3b2c1d0-e4f5-g6h7-i8j9-k0l9m8n7', N'CREAR_ACTIVIDAD', N'CREAR_ACTIVIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'a6358f8c-1c0f-431a-ad23-c0158ebf90e1', N'EDITAR_ROL', N'EDITAR_ROL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'b2dc1d67-2d88-453c-9e89-b8d43e3f46e2', N'VER_MODULO_CUENTAS', N'VER_MODULO_CUENTAS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'b40e8311-b0e4-4519-9071-174846562f83', N'ELIMINAR_VALOR_CATALOGO', N'ELIMINAR_VALOR_CATALOGO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'b5a2c3d1-e6f7-g8h9-i0j1-k2l3m4n5', N'CREAR_USUARIO', N'CREAR_USUARIO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'ba5708d1-d33c-4f4b-86b7-def470ec27ad', N'VER_DETALLE_ROL', N'VER_DETALLE_ROL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'bef219c4-1545-41dc-916d-85aad5687c62', N'VER_CUENTAS', N'VER_CUENTAS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'c0825109-ca76-44fb-9c61-8b563c7ffe12', N'CREAR_ROL', N'CREAR_ROL', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'c86b6c29-a14a-4732-ac1e-25398bcaf866', N'VER_DETALLE_EQUIPO', N'VER_DETALLE_EQUIPO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'c9860657-df71-4df7-8120-6ca899ee32a6', N'VER_MODULO_ROLES', N'VER_MODULO_ROLES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'cb1a8dc5-882f-4755-a9b4-f535ae03443c', N'EDITAR_VENDEDOR', N'EDITAR_VENDEDOR', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'd4809173-8698-44b3-a593-8470f29298a7', N'LIDER_DE_EQUIPO', N'LIDER_DE_EQUIPO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'd68314e7-8406-4e6c-9a94-af6ccf74597c', N'EDITAR_OPORTUNIDAD', N'EDITAR_OPORTUNIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'd7c5a4b3-e8f9-g0h1-i2j3-k4l5m6n7', N'VER_MODULO_USUARIOS', N'VER_MODULO_USUARIOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'de190709-22ce-4251-b206-889e0b28033b', N'VER_DETALLE_DOCUMENTO', N'VER_DETALLE_DOCUMENTO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'e0d64339-b0a4-49cb-88d3-4469a05f2e74', N'VER_VENDEDORES', N'VER_VENDEDORES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'e8cf6b0f-1077-4356-b6a7-bcd357a1240a', N'VER_DETALLE_OPORTUNIDAD', N'VER_DETALLE_OPORTUNIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'ee212df1-33a9-4d4d-a23c-44fea39ecc3c', N'VER_MODULO_CONTACTOS', N'VER_MODULO_CONTACTOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'f1c3bdab-d363-4332-b8fa-5bacf2c4c6a2', N'CREAR_OPORTUNIDAD', N'CREAR_OPORTUNIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'f6543210-9876-5432-10fe-dcba98765432', N'VER_MODULO_HISTORIAL_ACTIVIDADES', N'VER_MODULO_HISTORIAL_ACTIVIDADES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'f9b4d9c7-85f3-4e06-a7e5-d141fd5ec22c', N'VER_OPORTUNIDADES', N'VER_OPORTUNIDADES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'f9e7c6a5-b8d9-h0i1-j2k3-l4m5n6', N'EDITAR_USUARIO', N'EDITAR_USUARIO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'fa99ef17-65d9-40d0-abcb-89ae4036c996', N'VER_ROLES', N'VER_ROLES', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'fedcba98-7654-3210-fedc-ba9876543210', N'VER_DETALLE_ACTIVIDAD', N'VER_DETALLE_ACTIVIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'h1g9e7c6-a8b9-d0f1-i2j3-k4l5m6n7', N'DESACTIVAR_USUARIO', N'DESACTIVAR_USUARIO', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'j3i1g9e7-c8a9-b0d1-f2h3-k4l5m6n7', N'VER_LISTADO_USUARIOS', N'VER_LISTADO_USUARIOS', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'pqrstuvw-xyz0-1234-5678-9abcdefghi', N'DESACTIVAR_ACTIVIDAD', N'DESACTIVAR_ACTIVIDAD', NULL)
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'tuvwxyz01-2345-6789-abcdef-ghijklm', N'VER_HISTORIAL_ACTIVIDAD', N'VER_HISTORIAL_ACTIVIDAD', NULL)
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AspNetRoles where Id = '01eff2f3-37b8-4de9-9f4e-ee1bb8bb051d'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '078e1e55-a68e-4710-904a-b6e2be9a0599'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '07dea0d8-4548-4533-9352-cf99168b937d'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '096a149c-d609-4b71-adb9-66f659d584db'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '12e9f808-0cd6-4502-a37f-0133986c1370'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '1bfe4739-b550-4377-a411-da879cc7f32b'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '1ed10588-8f20-4fd8-93eb-a4bab939716a'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '1f31008a-a76d-4233-b782-c84f705ab300'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '23369738-f022-42d2-8b88-66b185681c80'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '2525042d-f14a-4fea-bee3-a0f5a8268456'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '2c57f926-24ac-4ac5-8125-3ed39fe171c3'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '32ddad12-52ce-418b-9737-955673c0f685'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '3b44c0ab-b713-4f94-a6c6-220a73a2bc31'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '3d7c88c0-977c-4bed-bd38-cecc54cd794c'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '400fe836-3386-408d-963a-938ecd73e971'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '40f8c861-a6a7-47a3-82da-6312d8bb4254'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '43c11e1c-248f-47fd-9716-ff1c8056ed88'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '4776679a-bf09-4066-86cd-31431265546f'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '4cb2c594-00b3-482b-adc5-457e1548bc30'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '4ed778ed-c10c-4949-a3c2-01fe240f1899'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '567890ab-cdef-ghij-klmn-opqrstuvwx'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '59316cb0-c95f-4bc8-8e30-be42897538d2'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '5f1508a6-943c-4eb2-90fe-8a2738a1e650'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '65151529-bf91-4c1b-96b3-4e694e50ccf3'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '694e9d70-c1d8-41e3-86e1-27d8cad478da'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '6fcebc8c-f58b-4a7c-ae26-04ec68d5dd1d'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '78ff7e81-5440-4229-9474-5f9b0b35c4cc'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '87654321-0987-6543-2109-876543210987'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '8973a145-0a93-4a1e-a8e0-82332e13abd2'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '8e177bbf-53d5-4092-bef2-042e89182580'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '8feebd0d-2832-437e-8af1-3d57a9c92d7b'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '94914d80-d8f5-447f-8819-799017fb171b'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '99cd763e-27a0-442d-9a79-06855dd7546a'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '9db239f2-3de7-4470-b5cd-8d963680032a'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = '9e44ee3a-1af3-4aac-a9de-1991634bfb52'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'a3b2c1d0-e4f5-g6h7-i8j9-k0l9m8n7'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'a6358f8c-1c0f-431a-ad23-c0158ebf90e1'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'b2dc1d67-2d88-453c-9e89-b8d43e3f46e2'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'b40e8311-b0e4-4519-9071-174846562f83'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'b5a2c3d1-e6f7-g8h9-i0j1-k2l3m4n5'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'ba5708d1-d33c-4f4b-86b7-def470ec27ad'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'bef219c4-1545-41dc-916d-85aad5687c62'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'c0825109-ca76-44fb-9c61-8b563c7ffe12'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'c86b6c29-a14a-4732-ac1e-25398bcaf866'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'c9860657-df71-4df7-8120-6ca899ee32a6'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'cb1a8dc5-882f-4755-a9b4-f535ae03443c'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'd4809173-8698-44b3-a593-8470f29298a7'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'd68314e7-8406-4e6c-9a94-af6ccf74597c'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'd7c5a4b3-e8f9-g0h1-i2j3-k4l5m6n7'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'de190709-22ce-4251-b206-889e0b28033b'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'e0d64339-b0a4-49cb-88d3-4469a05f2e74'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'e8cf6b0f-1077-4356-b6a7-bcd357a1240a'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'ee212df1-33a9-4d4d-a23c-44fea39ecc3c'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'f1c3bdab-d363-4332-b8fa-5bacf2c4c6a2'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'f6543210-9876-5432-10fe-dcba98765432'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'f9b4d9c7-85f3-4e06-a7e5-d141fd5ec22c'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'f9e7c6a5-b8d9-h0i1-j2k3-l4m5n6'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'fa99ef17-65d9-40d0-abcb-89ae4036c996'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'fedcba98-7654-3210-fedc-ba9876543210'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'h1g9e7c6-a8b9-d0f1-i2j3-k4l5m6n7'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'j3i1g9e7-c8a9-b0d1-f2h3-k4l5m6n7'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'pqrstuvw-xyz0-1234-5678-9abcdefghi'");
            migrationBuilder.Sql("delete from AspNetRoles where Id = 'tuvwxyz01-2345-6789-abcdef-ghijklm'");
        }
    }
}
