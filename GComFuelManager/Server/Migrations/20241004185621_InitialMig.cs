using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GComFuelManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accion",
                columns: table => new
                {
                    Cod = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estatus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accion", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "ActividadRegistrada",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadRegistrada", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserCod = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Catalogo_Fijo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valor = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Catalogo = table.Column<short>(type: "smallint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Abreviacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo_Fijo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMCatalogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMCatalogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMDivisiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMDivisiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMDocumentoRelacionados",
                columns: table => new
                {
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    DocumentoRelacionadoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMDocumentoRelacionados", x => new { x.DocumentoId, x.DocumentoRelacionadoId });
                });

            migrationBuilder.CreateTable(
                name: "CRMDocumentoRevisiones",
                columns: table => new
                {
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    RevisionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMDocumentoRevisiones", x => new { x.DocumentoId, x.RevisionId });
                });

            migrationBuilder.CreateTable(
                name: "CRMDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCaducidad = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionCreadaPor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Directorio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMDocumentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMGrupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMGrupos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMRolPermisos",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false),
                    PermisoId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMRolPermisos", x => new { x.RolId, x.PermisoId });
                });

            migrationBuilder.CreateTable(
                name: "CRMRolUsuarios",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMRolUsuarios", x => new { x.RolId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "CRMUsuarioDivisiones",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMUsuarioDivisiones", x => new { x.UsuarioId, x.DivisionId });
                });

            migrationBuilder.CreateTable(
                name: "Errors",
                columns: table => new
                {
                    Cod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fch = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "GrupoUsuario",
                columns: table => new
                {
                    cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codusu = table.Column<int>(type: "int", nullable: false),
                    codgru = table.Column<int>(type: "int", nullable: false),
                    fch = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoUsuario", x => x.cod);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codusu = table.Column<int>(type: "int", nullable: true),
                    fch = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.cod);
                });

            migrationBuilder.CreateTable(
                name: "Originadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Originadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tad",
                columns: table => new
                {
                    Cod = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Den = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Nro = table.Column<short>(type: "smallint", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CodigoOrdenes = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Ultima_Actualizacion_Catalogo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tipo_Vale = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tad", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Den = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Usu = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Cve = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Fch = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tip = table.Column<byte>(type: "tinyint", nullable: true),
                    Est = table.Column<byte>(type: "tinyint", nullable: true),
                    Privilegio = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CodCte = table.Column<int>(type: "int", nullable: true),
                    CodGru = table.Column<short>(type: "smallint", nullable: true),
                    IsClient = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Cod);
                });

            migrationBuilder.CreateTable(
                name: "Usuario_Tad",
                columns: table => new
                {
                    Id_Usuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id_Terminal = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario_Tad", x => new { x.Id_Usuario, x.Id_Terminal });
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CRMCatalogoValores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abreviacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CatalogoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMCatalogoValores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMCatalogoValores_CRMCatalogos_CatalogoId",
                        column: x => x.CatalogoId,
                        principalTable: "CRMCatalogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMOriginadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tel_Oficina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tel_Movil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMOriginadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMOriginadores_CRMDivisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "CRMDivisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMRoles_CRMDivisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "CRMDivisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMVendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Departamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel_Oficina = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel_Movil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMVendedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMVendedores_CRMDivisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "CRMDivisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CRMGrupoRoles",
                columns: table => new
                {
                    RolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMGrupoRoles", x => new { x.RolId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_CRMGrupoRoles_CRMGrupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "CRMGrupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Den = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Codusu = table.Column<int>(type: "int", nullable: true),
                    Codforpag = table.Column<int>(type: "int", nullable: true),
                    Tem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Codgru = table.Column<short>(type: "smallint", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Con = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Codtad = table.Column<short>(type: "smallint", nullable: true),
                    Codsyn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Esenergas = table.Column<bool>(type: "bit", nullable: true),
                    Tipven = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CodCte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Consecutivo = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    precioSemanal = table.Column<bool>(type: "bit", nullable: true),
                    Id_Vendedor = table.Column<int>(type: "int", nullable: false),
                    Id_Originador = table.Column<int>(type: "int", nullable: false),
                    Id_Tad = table.Column<short>(type: "smallint", nullable: true),
                    MdVenta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identificador_Externo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Cod);
                    table.ForeignKey(
                        name: "FK_Cliente_Originadores_Id_Originador",
                        column: x => x.Id_Originador,
                        principalTable: "Originadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Vendedores_Id_Vendedor",
                        column: x => x.Id_Vendedor,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Metas_Vendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Meta = table.Column<double>(type: "float", nullable: true),
                    Referencia = table.Column<double>(type: "float", nullable: true),
                    Venta_Real = table.Column<double>(type: "float", nullable: true),
                    Mes = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas_Vendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metas_Vendedor_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendedor_Originador",
                columns: table => new
                {
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    OriginadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedor_Originador", x => new { x.VendedorId, x.OriginadorId });
                    table.ForeignKey(
                        name: "FK_Vendedor_Originador_Originadores_OriginadorId",
                        column: x => x.OriginadorId,
                        principalTable: "Originadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendedor_Originador_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMEquipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LiderId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMEquipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMEquipos_CRMDivisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "CRMDivisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMEquipos_CRMOriginadores_LiderId",
                        column: x => x.LiderId,
                        principalTable: "CRMOriginadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMVendedorOriginadores",
                columns: table => new
                {
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    OriginadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMVendedorOriginadores", x => new { x.VendedorId, x.OriginadorId });
                    table.ForeignKey(
                        name: "FK_CRMVendedorOriginadores_CRMOriginadores_OriginadorId",
                        column: x => x.OriginadorId,
                        principalTable: "CRMOriginadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMVendedorOriginadores_CRMVendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "CRMVendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente_Tad",
                columns: table => new
                {
                    Id_Cliente = table.Column<int>(type: "int", nullable: false),
                    Id_Terminal = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente_Tad", x => new { x.Id_Cliente, x.Id_Terminal });
                    table.ForeignKey(
                        name: "FK_Cliente_Tad_Cliente_Id_Cliente",
                        column: x => x.Id_Cliente,
                        principalTable: "Cliente",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Tad_Tad_Id_Terminal",
                        column: x => x.Id_Terminal,
                        principalTable: "Tad",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contacto",
                columns: table => new
                {
                    Cod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCte = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacto", x => x.Cod);
                    table.ForeignKey(
                        name: "FK_Contacto_Cliente_CodCte",
                        column: x => x.CodCte,
                        principalTable: "Cliente",
                        principalColumn: "Cod",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMEquipoVendedores",
                columns: table => new
                {
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMEquipoVendedores", x => new { x.EquipoId, x.VendedorId });
                    table.ForeignKey(
                        name: "FK_CRMEquipoVendedores_CRMEquipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "CRMEquipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMEquipoVendedores_CRMVendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "CRMVendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMActividadDocumentos",
                columns: table => new
                {
                    ActividadId = table.Column<int>(type: "int", nullable: false),
                    DocumentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMActividadDocumentos", x => new { x.ActividadId, x.DocumentoId });
                    table.ForeignKey(
                        name: "FK_CRMActividadDocumentos_CRMDocumentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "CRMDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Asunto = table.Column<int>(type: "int", nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fecha_Mod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fch_Inicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fecha_Ven = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Prioridad = table.Column<int>(type: "int", nullable: true),
                    Asignado = table.Column<int>(type: "int", nullable: true),
                    Desccripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: true),
                    Contacto_Rel = table.Column<int>(type: "int", nullable: true),
                    Recordatorio = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMActividades_CRMCatalogoValores_Asunto",
                        column: x => x.Asunto,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMActividades_CRMCatalogoValores_Estatus",
                        column: x => x.Estatus,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMActividades_CRMCatalogoValores_Prioridad",
                        column: x => x.Prioridad,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMActividades_CRMEquipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "CRMEquipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMActividades_CRMVendedores_Asignado",
                        column: x => x.Asignado,
                        principalTable: "CRMVendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactoPrincipalId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CRMContactos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: true),
                    Tel_Oficina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tel_Movil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SitioWeb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Colonia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstatusId = table.Column<int>(type: "int", nullable: true),
                    Estatus_Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrigenId = table.Column<int>(type: "int", nullable: true),
                    Recomen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Mod = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMContactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMContactos_CRMCatalogoValores_EstatusId",
                        column: x => x.EstatusId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMContactos_CRMCatalogoValores_OrigenId",
                        column: x => x.OrigenId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMContactos_CRMClientes_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "CRMClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMContactos_CRMDivisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "CRMDivisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMContactos_CRMVendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "CRMVendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMOportunidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Opor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorOportunidad = table.Column<double>(type: "float", nullable: false),
                    UnidadMedidaId = table.Column<int>(type: "int", nullable: false),
                    Prox_Paso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    ContactoId = table.Column<int>(type: "int", nullable: false),
                    PeriodoId = table.Column<int>(type: "int", nullable: false),
                    TipoId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EtapaVentaId = table.Column<int>(type: "int", nullable: false),
                    Probabilidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    OrigenPrductoId = table.Column<int>(type: "int", nullable: false),
                    TipoProductoId = table.Column<int>(type: "int", nullable: false),
                    ModeloVentaId = table.Column<int>(type: "int", nullable: false),
                    VolumenId = table.Column<int>(type: "int", nullable: false),
                    FormaPagoId = table.Column<int>(type: "int", nullable: false),
                    DiasPagoId = table.Column<int>(type: "int", nullable: false),
                    CantidadEstaciones = table.Column<int>(type: "int", nullable: false),
                    CantidadLts = table.Column<double>(type: "float", nullable: false),
                    PrecioLts = table.Column<double>(type: "float", nullable: false),
                    TotalLts = table.Column<double>(type: "float", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMOportunidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_DiasPagoId",
                        column: x => x.DiasPagoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_EtapaVentaId",
                        column: x => x.EtapaVentaId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_FormaPagoId",
                        column: x => x.FormaPagoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_ModeloVentaId",
                        column: x => x.ModeloVentaId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_OrigenPrductoId",
                        column: x => x.OrigenPrductoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_TipoId",
                        column: x => x.TipoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_TipoProductoId",
                        column: x => x.TipoProductoId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMCatalogoValores_VolumenId",
                        column: x => x.VolumenId,
                        principalTable: "CRMCatalogoValores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMClientes_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "CRMClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMContactos_ContactoId",
                        column: x => x.ContactoId,
                        principalTable: "CRMContactos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMEquipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "CRMEquipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidades_CRMVendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "CRMVendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CRMOportunidadDocumentos",
                columns: table => new
                {
                    OportunidadId = table.Column<int>(type: "int", nullable: false),
                    DocumentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRMOportunidadDocumentos", x => new { x.OportunidadId, x.DocumentoId });
                    table.ForeignKey(
                        name: "FK_CRMOportunidadDocumentos_CRMDocumentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "CRMDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CRMOportunidadDocumentos_CRMOportunidades_OportunidadId",
                        column: x => x.OportunidadId,
                        principalTable: "CRMOportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Id_Originador",
                table: "Cliente",
                column: "Id_Originador");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Id_Vendedor",
                table: "Cliente",
                column: "Id_Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Tad_Id_Terminal",
                table: "Cliente_Tad",
                column: "Id_Terminal");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_CodCte",
                table: "Contacto",
                column: "CodCte");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividadDocumentos_DocumentoId",
                table: "CRMActividadDocumentos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_Asignado",
                table: "CRMActividades",
                column: "Asignado");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_Asunto",
                table: "CRMActividades",
                column: "Asunto");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_Contacto_Rel",
                table: "CRMActividades",
                column: "Contacto_Rel");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_EquipoId",
                table: "CRMActividades",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_Estatus",
                table: "CRMActividades",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_CRMActividades_Prioridad",
                table: "CRMActividades",
                column: "Prioridad");

            migrationBuilder.CreateIndex(
                name: "IX_CRMCatalogoValores_CatalogoId",
                table: "CRMCatalogoValores",
                column: "CatalogoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMClientes_ContactoPrincipalId",
                table: "CRMClientes",
                column: "ContactoPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMContactos_CuentaId",
                table: "CRMContactos",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMContactos_DivisionId",
                table: "CRMContactos",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMContactos_EstatusId",
                table: "CRMContactos",
                column: "EstatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMContactos_OrigenId",
                table: "CRMContactos",
                column: "OrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMContactos_VendedorId",
                table: "CRMContactos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMEquipos_DivisionId",
                table: "CRMEquipos",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMEquipos_LiderId",
                table: "CRMEquipos",
                column: "LiderId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMEquipoVendedores_VendedorId",
                table: "CRMEquipoVendedores",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMGrupoRoles_GrupoId",
                table: "CRMGrupoRoles",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidadDocumentos_DocumentoId",
                table: "CRMOportunidadDocumentos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_ContactoId",
                table: "CRMOportunidades",
                column: "ContactoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_CuentaId",
                table: "CRMOportunidades",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_DiasPagoId",
                table: "CRMOportunidades",
                column: "DiasPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_EquipoId",
                table: "CRMOportunidades",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_EtapaVentaId",
                table: "CRMOportunidades",
                column: "EtapaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_FormaPagoId",
                table: "CRMOportunidades",
                column: "FormaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_ModeloVentaId",
                table: "CRMOportunidades",
                column: "ModeloVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_OrigenPrductoId",
                table: "CRMOportunidades",
                column: "OrigenPrductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_PeriodoId",
                table: "CRMOportunidades",
                column: "PeriodoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_TipoId",
                table: "CRMOportunidades",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_TipoProductoId",
                table: "CRMOportunidades",
                column: "TipoProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_UnidadMedidaId",
                table: "CRMOportunidades",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_VendedorId",
                table: "CRMOportunidades",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOportunidades_VolumenId",
                table: "CRMOportunidades",
                column: "VolumenId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMOriginadores_DivisionId",
                table: "CRMOriginadores",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMRoles_DivisionId",
                table: "CRMRoles",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMVendedores_DivisionId",
                table: "CRMVendedores",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CRMVendedorOriginadores_OriginadorId",
                table: "CRMVendedorOriginadores",
                column: "OriginadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_Vendedor_VendedorId",
                table: "Metas_Vendedor",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedor_Originador_OriginadorId",
                table: "Vendedor_Originador",
                column: "OriginadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CRMActividadDocumentos_CRMActividades_ActividadId",
                table: "CRMActividadDocumentos",
                column: "ActividadId",
                principalTable: "CRMActividades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMActividades_CRMContactos_Contacto_Rel",
                table: "CRMActividades",
                column: "Contacto_Rel",
                principalTable: "CRMContactos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CRMClientes_CRMContactos_ContactoPrincipalId",
                table: "CRMClientes",
                column: "ContactoPrincipalId",
                principalTable: "CRMContactos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CRMContactos_CRMCatalogoValores_EstatusId",
                table: "CRMContactos");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMContactos_CRMCatalogoValores_OrigenId",
                table: "CRMContactos");

            migrationBuilder.DropForeignKey(
                name: "FK_CRMClientes_CRMContactos_ContactoPrincipalId",
                table: "CRMClientes");

            migrationBuilder.DropTable(
                name: "Accion");

            migrationBuilder.DropTable(
                name: "ActividadRegistrada");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Catalogo_Fijo");

            migrationBuilder.DropTable(
                name: "Cliente_Tad");

            migrationBuilder.DropTable(
                name: "Contacto");

            migrationBuilder.DropTable(
                name: "CRMActividadDocumentos");

            migrationBuilder.DropTable(
                name: "CRMDocumentoRelacionados");

            migrationBuilder.DropTable(
                name: "CRMDocumentoRevisiones");

            migrationBuilder.DropTable(
                name: "CRMEquipoVendedores");

            migrationBuilder.DropTable(
                name: "CRMGrupoRoles");

            migrationBuilder.DropTable(
                name: "CRMOportunidadDocumentos");

            migrationBuilder.DropTable(
                name: "CRMRoles");

            migrationBuilder.DropTable(
                name: "CRMRolPermisos");

            migrationBuilder.DropTable(
                name: "CRMRolUsuarios");

            migrationBuilder.DropTable(
                name: "CRMUsuarioDivisiones");

            migrationBuilder.DropTable(
                name: "CRMVendedorOriginadores");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "GrupoUsuario");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Metas_Vendedor");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Usuario_Tad");

            migrationBuilder.DropTable(
                name: "Vendedor_Originador");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tad");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "CRMActividades");

            migrationBuilder.DropTable(
                name: "CRMGrupos");

            migrationBuilder.DropTable(
                name: "CRMDocumentos");

            migrationBuilder.DropTable(
                name: "CRMOportunidades");

            migrationBuilder.DropTable(
                name: "Originadores");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropTable(
                name: "CRMEquipos");

            migrationBuilder.DropTable(
                name: "CRMOriginadores");

            migrationBuilder.DropTable(
                name: "CRMCatalogoValores");

            migrationBuilder.DropTable(
                name: "CRMCatalogos");

            migrationBuilder.DropTable(
                name: "CRMContactos");

            migrationBuilder.DropTable(
                name: "CRMClientes");

            migrationBuilder.DropTable(
                name: "CRMVendedores");

            migrationBuilder.DropTable(
                name: "CRMDivisiones");
        }
    }
}
