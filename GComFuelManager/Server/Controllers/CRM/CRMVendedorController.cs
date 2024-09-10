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
                var vendedores = context.CRMVendedores.AsNoTracking().AsQueryable();

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
                    .Include(x => x.Originadores)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Division)
                    .SingleOrDefaultAsync();
                if (vendedor is null) { return NotFound(); }

                var originadoresdto = vendedor.Originadores.Select(x => mapper.Map<CRMOriginador, CRMOriginadorDTO>(x)).ToList();

                var vendedordto = mapper.Map<CRMVendedorPostDTO>(vendedor);
                vendedordto.OriginadoresDTO = originadoresdto;

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
                    .Include(x => x.Division)
                    .Include(x => x.Originadores)
                    .Where(x => x.Id == Id)
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

                    var relations = dTO.OriginadoresDTO.Select(x => new CRMVendedorOriginador { VendedorId = vendedor.Id, OriginadorId = x.Id }).ToList();
                    var relations_actual = await context.CRMVendedorOriginadores.Where(x => x.VendedorId == vendedor.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);
                    }


                    context.Update(vendedor);
                }
                else
                {
                    var integrantes = dTO.OriginadoresDTO.Select(x => new CRMVendedorOriginador { OriginadorId = x.Id, VendedorId = vendedordto.Id }).ToList();
                    vendedordto.VendedorOriginadores = integrantes;

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
                var updateUserAsp = await userManager.FindByIdAsync(info.Id_Asp);

                if (updateUserAsp != null)
                {
                    ////Variable para asignacion de los roles
                    //var roles = info.Roles;
                    ////Nuevo dato a actualizar del usuario de Asp, solo mandamos el Nombre de usuario 
                    //updateUserAsp.UserName = info.UserName;
                    ////Nuevo dato para actualizar la contraseña
                    //var changepassword = await userManager.ChangePasswordAsync(updateUserAsp, viejaPass, updateUserSistema.Cve);
                    ////A través de estas acciones, vamos a obtener, remover y volver a agregar el listado de roles
                    ////Method para obtención de los roles
                    //var changeGetRoles = await userManager.GetRolesAsync(updateUserAsp);
                    ////Method para eliminar los roles 
                    //var resultDeleteRoles = await userManager.RemoveFromRolesAsync(updateUserAsp, changeGetRoles.ToList());
                    ////Method para mandar el listado de roles
                    //var resultAddRoles = await userManager.AddToRolesAsync(updateUserAsp, roles);
                    ////Segundo parametros me pide un string de roles no un listado 

                    //var resultado = await userManager.UpdateAsync(updateUserAsp);
                    //if (!resultado.Succeeded)
                    //{
                    //    context.Update(oldUser);
                    //    await context.SaveChangesAsync();
                    //    return BadRequest();
                    //}
                }

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

                var rolesAsignados = await context.CRMRolUsuarios
                  .Where(x => x.UserId == Id)
                  .Select(x => x.RolId)
                  .ToListAsync();

                // Obtener roles
                var roles = await context.CRMRoles.ToListAsync();

                // Filtrar la lista de roles del sistema y marcar los que están asignados al usuario
                var rolesUsuario = roles
                    .Select(r => new CRMRol
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        // Si el Id del rol existe en la lista de roles asignados al usuario, lo marcamos como asignado
                        Asignado = rolesAsignados.Contains(r.Id)
                    })
                    .ToList();

                // DTO para enviar la información al front
                var usuarioDto = new CRMUsuarioDTO
                {
                    Id_Asp = usuario.Id,
                    UserName = usuario.UserName,
                    IsVendedor = vendedor != null,
                    IsComercial = originador != null,
                    IDVendedor = vendedor?.Id,
                    IDOriginador = originador?.Id,
                    IDDivision = vendedor?.DivisionId ?? originador?.DivisionId,  // Relacionar con división
                    Roles = rolesUsuario // Asignar roles del usuario
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
