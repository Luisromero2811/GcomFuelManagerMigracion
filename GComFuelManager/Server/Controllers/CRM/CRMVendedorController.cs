using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using static iText.IO.Image.Jpeg2000ImageData;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMVendedorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly RoleManager<IdentityRol> roleManager;
        private readonly IMapper mapper;
        private readonly IValidator<CRMVendedorPostDTO> validator;

        public CRMVendedorController(ApplicationDbContext context,
            UserManager<IdentityUsuario> userManager,
            RoleManager<IdentityRol> roleManager,
            IMapper mapper,
            IValidator<CRMVendedorPostDTO> validator)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMVendedorDTO dTO)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();
                var vendedores = new List<CRMVendedor>().AsQueryable();
                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    vendedores = context.CRMVendedores.Where(x => x.Activo).AsNoTracking().AsQueryable();
                }
                else if (await userManager.IsInRoleAsync(user, "CRM_LIDER"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();
                    var equipos = await context.CRMEquipos.AsNoTracking().Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();
                    var relaciones = await context.CRMEquipoVendedores.AsNoTracking().Where(x => equipos.Contains(x.EquipoId)).Select(x => x.VendedorId).ToListAsync();
                    vendedores = context.CRMVendedores.AsNoTracking().Where(x => x.Activo && relaciones.Contains(x.Id)).AsQueryable();
                }
                else
                {
                    vendedores = context.CRMVendedores.AsNoTracking().Where(x => x.UserId == user.Id).AsQueryable();
                }


                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    vendedores = vendedores.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()) || v.Apellidos.ToLower().Contains(dTO.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Tel_Movil) || !string.IsNullOrWhiteSpace(dTO.Tel_Movil))
                    vendedores = vendedores.Where(v => !string.IsNullOrEmpty(v.Tel_Movil) && v.Tel_Movil.ToLower().Contains(dTO.Tel_Movil.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Correo) || !string.IsNullOrWhiteSpace(dTO.Correo))
                    vendedores = vendedores.Where(v => !string.IsNullOrEmpty(v.Correo) && v.Correo.ToLower().Contains(dTO.Correo.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(vendedores, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                vendedores = vendedores.Include(x => x.Division).Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                var vendedoresdto = vendedores.Select(x => mapper.Map<CRMVendedorDTO>(x));

                return Ok(vendedoresdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("listvendedores")]
        public async Task<ActionResult> GetAllVendedores()
        {
            try
            {
                var vendedoresCRM = await context.CRMVendedores
                    .Where(x => x.Activo == true)
                    .ToListAsync();
                return Ok(vendedoresCRM);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int Id)
        {
            try
            {
                var vendedor = await context.CRMVendedores.Where(x => x.Id == Id)
                    //.Include(x => x.Originadores)
                    //.ThenInclude(x => x.Division)
                    .Include(x => x.Division)
                    .Include(x => x.Equipos)
                    .ThenInclude(x => x.Originador)
                    .SingleOrDefaultAsync();
                if (vendedor is null) { return NotFound(); }

                var equiposdto = vendedor.Equipos.Select(x => mapper.Map<CRMEquipo, CRMEquipoDTO>(x)).ToList();

                var vendedordto = mapper.Map<CRMVendedorPostDTO>(vendedor);
                vendedordto.EquiposDTO = equiposdto;

                return Ok(vendedordto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public async Task<ActionResult> GetByIdDetalle([FromRoute] int Id)
        {
            try
            {
                var vendedor = await context.CRMVendedores
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Include(x => x.Division)
                    .Include(x => x.Originadores)
                    .Include(x => x.Equipos)
                    .ThenInclude(x => x.Originador)
                    .Include(x => x.Equipos)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Contactos)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Contactos)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Oportunidades)
                    .ThenInclude(x => x.CRMCliente)
                    .Include(x => x.Oportunidades)
                    .ThenInclude(x => x.Contacto)
                    .Include(x => x.Oportunidades)
                    .ThenInclude(x => x.EtapaVenta)
                    .Select(x => mapper.Map<CRMVendedor, CRMVendedorDetalleDTO>(x))
                    .SingleOrDefaultAsync();
                if (vendedor is null) { return NotFound(); }

                //var vendedordto = mapper.Map<CRMVendedorDTO>(vendedor);

                return Ok(vendedor);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMVendedorPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var vendedordto = mapper.Map<CRMVendedorPostDTO, CRMVendedor>(dTO);
                //var originadores = dTO.OriginadoresDTO.Select(x => mapper.Map<CRMOriginadorDTO, CRMOriginador>(x)).ToList();

                //vendedor.Originadores = originadores;

                if (vendedordto.Id != 0)
                {
                    var vendedordb = await context.CRMVendedores.FindAsync(dTO.Id);
                    if (vendedordb is null) { return NotFound(); }

                    var vendedor = mapper.Map(vendedordto, vendedordb);

                    var relations = dTO.EquiposDTO.Select(x => new CRMEquipoVendedor { VendedorId = vendedor.Id, EquipoId = x.Id }).ToList();
                    var relations_actual = await context.CRMEquipoVendedores.Where(x => x.VendedorId == vendedor.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);
                    }


                    context.Update(vendedor);
                }
                else
                {
                    var integrantes = dTO.EquiposDTO.Select(x => new CRMEquipoVendedor { EquipoId = x.Id, VendedorId = vendedordto.Id }).ToList();
                    vendedordto.EquipoVendedores = integrantes;

                    await context.AddAsync(vendedordto);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var vendedor = await context.CRMVendedores.FindAsync(Id);
                if (vendedor is null) { return NotFound(); }
                vendedor.Activo = false;
                context.Update(vendedor);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("relation")]
        public async Task<ActionResult> DeleteRelationVendedorOriginador([FromQuery] CRMVendedorOriginador dto)
        {
            try
            {
                if (dto is null) { return NotFound(); }
                context.Remove(dto);
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("crear")]
        public async Task<ActionResult> CreateUser([FromBody] CRMUsuarioDTO info)
        {
            try
            {
                //Verificamos que el usuario ya exista
                var userAsp = await userManager.FindByNameAsync(info.UserName);
                if (userAsp != null)
                {
                    return BadRequest(new { message = "El usuario ya existe" });
                }

                if (info.IsComercial)
                {
                    var comercial = context.CRMOriginadores.Find(info.IDOriginador);
                    if (comercial is null)
                    {
                        return BadRequest(new { message = "El comercial seleccionado no existe" });
                    }

                }
                else if (info.IsVendedor)
                {
                    var vendedor = context.CRMVendedores.Find(info.IDVendedor);
                    if (vendedor is null)
                    {
                        return BadRequest(new { message = "El vendedor seleccionado no existe" });
                    }
                }
                //Creamos el nuevo usuario en Identity
                var newuserAsp = new IdentityUsuario
                {
                    UserName = info.UserName,
                    UserCod = 0
                };
                var result = await userManager.CreateAsync(newuserAsp, info.Password);
                //Verificamos que la creación del usuario fue exitosa
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors });
                }

                //Asignamos roles al usuario desde CRMRoles
                foreach (var rolNombre in info.Roles)
                {
                    // Obtenemos el rol desde la entidad CRMRoles
                    var crmRole = context.CRMRoles.FirstOrDefault(x => x.Nombre == rolNombre.Nombre);
                    if (crmRole == null)
                    {
                        return BadRequest(new { message = $"El rol '{rolNombre.Nombre}' no existe en CRMRoles." });
                    }

                    var userRole = new CRMRolUsuario
                    {
                        UserId = newuserAsp.Id, // Relación con el ID del usuario en Identity
                        RolId = crmRole.Id          // Relación con el ID del rol en CRMRoles
                    };

                    context.CRMRolUsuarios.Add(userRole);

                    // Obtenemos los permisos los de AspNetRoles asociados al rol de CRMRoles desde CRMRolPermisos
                    var permisos = context.CRMRolPermisos
                        .Where(x => x.RolId == crmRole.Id)
                        .Select(x => x.PermisoId)  // Aquí el permisoId es el ID del rol en AspNetRoles
                        .ToList();
                    // Asignamos los permisos (roles de Identity) al usuario
                    foreach (var permisoId in permisos)
                    {
                        var identityRole = await roleManager.FindByIdAsync(permisoId.ToString());
                        if (identityRole != null)
                        {
                            var alreadyInRole = await userManager.IsInRoleAsync(newuserAsp, identityRole.Name);
                            if (!alreadyInRole)
                            {
                                var identityRoleResult = await userManager.AddToRoleAsync(newuserAsp, identityRole.Name);
                                if (!identityRoleResult.Succeeded)
                                {
                                    return BadRequest(new { errors = identityRoleResult.Errors });
                                }
                            }
                            //Si ya tiene el rol, omitimos la asignación
                        }
                        else
                        {
                            return BadRequest(new { message = $"El permiso con ID '{permisoId}' no existe en AspNetRoles." });
                        }
                    }
                }
                //Relación de ID Asp.Net a CRMComercial
                if (info.IsComercial)
                {
                    var originador = await context.CRMOriginadores.FindAsync(info.IDOriginador);
                    if (originador == null)
                    {
                        return BadRequest(new { message = "El originador seleccionado no existe" });
                    }
                    originador.UserId = newuserAsp.Id;
                }
                else if (info.IsVendedor)
                {
                    var vendedor = await context.CRMVendedores.FindAsync(info.IDVendedor);
                    if (vendedor == null)
                    {
                        return BadRequest(new { message = "El vendedor seleccionado no existe" });
                    }
                    vendedor.UserId = newuserAsp.Id;
                }

                //Lógica para la relación con la división
                if (info.IDDivision != null)
                {
                    //Checamos si existe la division
                    var division = await context.CRMDivisiones.FindAsync(info.IDDivision);
                    if (division == null)
                    {
                        return BadRequest(new { message = "La division seleccionada no existe."});
                    }
                    //creamos la relación
                    var usuarioDivision = new CRMUsuarioDivision
                    {
                        UsuarioId = newuserAsp.Id,
                        DivisionId = (int)info.IDDivision
                    };
                    context.CRMUsuarioDivisiones.Add(usuarioDivision);
                }
                else
                {
                    return BadRequest(new { message = "No se ha proporcionado una División valida"});
                }

                await context.SaveChangesAsync();
                return Ok(new { success = true, user = newuserAsp });

            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpGet("getAllUser")]
        public async Task<ActionResult> GetListUsers([FromQuery] CRMUsuarioDTO usuario)
        {
            try
            {
                var usuarios =
                    (from user in context.Users
                     .Where(x => x.UserCod == 0)
                         // Unimos con CRMVendedores
                     join vendedor in context.CRMVendedores
                     on user.Id equals vendedor.UserId into vendJoin
                     from vendedor in vendJoin.DefaultIfEmpty()
                         // Unimos con CRMOriginadores
                     join originador in context.CRMOriginadores
                     on user.Id equals originador.UserId into origJoin
                     from originador in origJoin
                     .DefaultIfEmpty()
                     select new CRMUsuarioDTO
                     {
                         Id_Asp = user.Id,
                         UserName = user.UserName,
                         NombreUsuario = vendedor != null ? vendedor.Nombre : originador.Nombre,
                         TipoUsuario = vendedor != null ? "Vendedor" : originador != null ? "Comercial" : "Sin asignar",
                         Activo = user.Activo
                     })
                    .AsQueryable();

                //Filtros
                if (!string.IsNullOrEmpty(usuario.NombreUsuario))
                {
                    usuarios = usuarios.Where(x => x.NombreUsuario != null &&
                                           x.NombreUsuario.ToLower().Contains(usuario.NombreUsuario.ToLower()));
                }
                //Paginación
                await HttpContext.InsertarParametrosPaginacion(usuarios, usuario.tamanopagina, usuario.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != usuario.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        usuario.pagina = int.Parse(pagina!);
                    }
                }
                usuarios = usuarios.Skip((usuario.pagina - 1) * usuario.tamanopagina).Take(usuario.tamanopagina);
                return Ok(usuarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("changeStatus/{Id}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] string Id, [FromBody] bool status)
        {
            try
            {
                var Usuarios = await context.Users.FindAsync(Id);

                if (Usuarios is null)
                {
                    return NotFound();
                }

                Usuarios.Activo = status;
                context.Update(Usuarios);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("editar")]
        public async Task<ActionResult> PutUser([FromBody] CRMUsuarioDTO info)
        {
            try
            {
                var usuario = await context.Users
                    .FirstOrDefaultAsync(x => x.Id == info.Id_Asp);

                if (usuario == null)
                {
                    return BadRequest("El usuario no existe");
                }
                //Buscamos si el usuario ya esta asignado a un vendedor u originador
                var vendedorAsignado = await context.CRMVendedores
                    .FirstOrDefaultAsync(x => x.UserId == info.Id_Asp);
                var originadorAsignado = await context.CRMOriginadores
                    .FirstOrDefaultAsync(x => x.UserId == info.Id_Asp);

                if (!string.IsNullOrEmpty(info.UserName) && usuario.UserName != info.UserName)
                {
                    usuario.UserName = info.UserName;
                    usuario.NormalizedUserName = info.UserName.ToUpper();
                }

                if (!string.IsNullOrEmpty(info.Password))
                {
                    var passwordHasher = new PasswordHasher<IdentityUsuario>();
                    usuario.PasswordHash = passwordHasher.HashPassword(usuario, info.Password);
                }

                context.Update(usuario);

                //Obtener los roles actuales del usuario en CRMRolUsuarios
                var rolesActuales = await context.CRMRolUsuarios
                    .Where(x => x.UserId == info.Id_Asp)
                    .ToListAsync();

                //Eliminar roles antiguos que ya no esten en la lista de roles nuevos 
                foreach (var rolActual in rolesActuales)
                {
                    if (!info.Roles.Any(x => x.Id == rolActual.RolId))
                    {
                        //Eliminar también los permisos relacionados de Identity
                        var permisosEliminar = context.CRMRolPermisos
                            .Where(x => x.RolId == rolActual.RolId)
                            .Select(x => x.PermisoId)
                            .ToList();
                        foreach (var permisoId in permisosEliminar)
                        {
                            var identityRole = await roleManager.FindByIdAsync(permisoId.ToString());
                            if (identityRole != null)
                            {
                                var alreadyInRole = await userManager.IsInRoleAsync(usuario, identityRole.Name);
                                if (alreadyInRole)
                                {
                                    var identityRoleResult = await userManager.RemoveFromRoleAsync(usuario, identityRole.Name);
                                    if (!identityRoleResult.Succeeded)
                                    {
                                        return BadRequest(new { errors = identityRoleResult.Errors});
                                    }
                                }
                            }
                        }

                        context.Remove(rolActual);
                    }
                }

                //Aquí agrego nuevos roles seleccionados que no existan en relación
                foreach (var rol in info.Roles)
                {
                    if (!rolesActuales.Any(x => x.RolId == rol.Id))
                    {
                        var nuevoRolUsuario = new CRMRolUsuario
                        {
                            UserId = info.Id_Asp,
                            RolId = rol.Id
                        };
                        context.Add(nuevoRolUsuario);
                        //Asignar los permisos relacionados con el rol en AspNetRoles
                        var permisos = context.CRMRolPermisos
                            .Where(x => x.RolId == rol.Id)
                            .Select(x => x.PermisoId)
                            .ToList();
                        foreach (var permisoId in permisos)
                        {
                            var identityRole = await roleManager.FindByIdAsync(permisoId.ToString());
                            if (identityRole != null)
                            {
                                var alreadyInRole = await userManager.IsInRoleAsync(usuario, identityRole.Name);
                                if (!alreadyInRole)
                                {
                                    var identityRoleResult = await userManager.AddToRoleAsync(usuario, identityRole.Name);
                                    if (!identityRoleResult.Succeeded)
                                    {
                                        return BadRequest(new { error = identityRoleResult.Errors});
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = $"El permiso con ID {permisoId} no existe en AspNetRoles"});
                            }
                        }

                    }
                }

                // Si el usuario ya tiene un vendedor asignado, mostrar un error si intentan cambiarlo
                if (vendedorAsignado != null && info.IDVendedor != null && vendedorAsignado.Id != info.IDVendedor)
                {
                    return BadRequest("El usuario ya está asignado a un vendedor y no puede ser reasignado.");
                }

                // Si el usuario ya tiene un originador asignado, mostrar un error si intentan cambiarlo
                if (originadorAsignado != null && info.IDOriginador != null && originadorAsignado.Id != info.IDOriginador)
                {
                    return BadRequest("El usuario ya está asignado a un originador y no puede ser reasignado.");
                }

                // Si el usuario ya tiene asignado un vendedor, ignorar cualquier cambio en vendedor
                if (vendedorAsignado == null && info.IDVendedor != null)
                {
                    var vendedor = await context.CRMVendedores
                        .FirstOrDefaultAsync(x => x.Id == info.IDVendedor);

                    if (vendedor != null)
                    {
                        vendedor.UserId = info.Id_Asp;
                        context.Update(vendedor);
                    }
                    else
                    {
                        return BadRequest("El vendedor especificado no existe");
                    }
                }

                // Si el usuario ya tiene asignado un originador, ignorar cualquier cambio en originador
                if (originadorAsignado == null && info.IDOriginador != null)
                {
                    var originador = await context.CRMOriginadores
                        .FirstOrDefaultAsync(x => x.Id == info.IDOriginador);

                    if (originador != null)
                    {
                        originador.UserId = info.Id_Asp;
                        context.Update(originador);
                    }
                    else
                    {
                        return BadRequest("El originador especificado no existe");
                    }
                }


                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult> ObtenerUsuarioStatus([FromRoute] string Id)
        {
            try
            {
                var usuario = await context.Users
                    .Where(x => x.Id == Id)
                    .FirstOrDefaultAsync();
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado.");
                }

                // Obtener si el usuario es vendedor
                var vendedor = await context.CRMVendedores
                    .Where(x => x.UserId == Id)
                    .FirstOrDefaultAsync();

                // Obtener si el usuario es originador
                var originador = await context.CRMOriginadores
                    .Where(x => x.UserId == Id)
                    .FirstOrDefaultAsync();

                //Obtener la división del usuario desde CRMUsuarioDivision
                var usuarioDivision = await context.CRMUsuarioDivisiones
                    .Where(x => x.UsuarioId == Id)
                    .Select(x => x.DivisionId)
                    .FirstOrDefaultAsync();

                //Obtener los roles asignados al usuario desde la tabla de relación
                var rolesAsignados = await context.CRMRolUsuarios
                    .Where(x => x.UserId == Id)
                    .Select(x => x.RolId)
                    .ToListAsync();
                // DTO para enviar la información al front
                var usuarioDto = new CRMUsuarioDTO
                {
                    Id_Asp = usuario.Id,
                    UserName = usuario.UserName,
                    IsVendedor = vendedor != null,
                    IsComercial = originador != null,
                    IDVendedor = vendedor?.Id,
                    IDOriginador = originador?.Id,
                    IDDivision = usuarioDivision,
                    RolesAsignados = rolesAsignados
                };

                return Ok(usuarioDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
