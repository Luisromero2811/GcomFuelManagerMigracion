using System;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using System.Diagnostics;
using Newtonsoft.Json;

namespace GComFuelManager.Server.Controllers.UsuarioController
{
    [Route("api/usuarios")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Administrador de Usuarios, Direccion, Gerencia")]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;

        public UsuarioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        [HttpGet("list")]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                //Variable asignada para traer el contexto de Usuario igualado a UsuarioInfo con sus propiedades
                var usuarios = await context.Usuario.Select(x => new UsuarioInfo
                {
                    UserName = x.Usu!,
                    Password = x.Cve!,
                    Nombre = x.Den!,
                    UserCod = x.Cod,
                    Activo = x.Activo
                }).ToListAsync();

                foreach (var item in usuarios)
                {
                    var u = await context.Users.Where(x => x.UserCod == item.UserCod).FirstOrDefaultAsync();
                    if (u != null)
                    {
                        IList<string> roles = await userManager.GetRolesAsync(u);
                        usuarios.FirstOrDefault(x => x.UserCod == item.UserCod)!.Roles = roles.ToList();
                        usuarios.FirstOrDefault(x => x.UserCod == item.UserCod)!.Id = u.Id;
                    }
                }

                return Ok(usuarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("roles")]
        public async Task<ActionResult<List<RolDTO>>> Get()
        {
            return await context.Roles.Select(x => new RolDTO { ID = x.Id, NombreRol = x.Name! }).ToListAsync();
        }

        [HttpPost("crear")]
        public async Task<ActionResult> Create([FromBody] UsuarioInfo info)
        {
            try
            {
                var userSistema = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == info.UserName);
                if (userSistema != null)
                {
                    return BadRequest("El usuario ya existe");
                }
                var userAsp = await userManager.FindByNameAsync(info.UserName);
                if (userAsp != null)
                {
                    return BadRequest("El usuario ya existe");
                }
                var newUserSistema = new Usuario { Den = info.Nombre, Usu = info.UserName, Fch = DateTime.Now, Cve = info.Password };
                context.Add(newUserSistema);
                await context.SaveChangesAsync();
                var newUserAsp = new IdentityUsuario { UserName = newUserSistema.Usu, UserCod = newUserSistema.Cod };
                var result = await userManager.CreateAsync(newUserAsp, newUserSistema.Cve);
                //Si el resultado no fue exitoso
                if (!result.Succeeded)
                {
                    context.Remove(newUserSistema);
                    await context.SaveChangesAsync();
                    return BadRequest(result.Errors);
                }
                result = await userManager.AddToRolesAsync(newUserAsp, info.Roles);
                //Si el resultado no fue exitoso
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                //Si el resultado fue exitoso, retorna el nuevo usuario
                return Ok(newUserSistema);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPut("editar")]
        public async Task<ActionResult> PutUser([FromBody] UsuarioInfo info)
        {
            try
            {
                //Buscamos al usuario y lo sincronizamos con el usuario de ASP
                var updateUserSistema = await context.Usuario.FirstOrDefaultAsync(x => x.Cod == info.UserCod);

                if (updateUserSistema == null)
                {
                    return NotFound();
                }

                var oldUser = updateUserSistema;

                //Variable para asignacion de la vieja contraseña
                var viejaPass = updateUserSistema.Cve;
                //Nuevos datos a actualizar del usuario, Nombre, nombre de usuario y contraseña
                updateUserSistema.Den = info.Nombre;
                updateUserSistema.Usu = info.UserName;
                updateUserSistema.Cve = info.Password;
                //Actualizacion de registros
                context.Update(updateUserSistema);
                await context.SaveChangesAsync();
                //Actualizar Usuario de Identity AspNet
                var updateUserAsp = await userManager.FindByIdAsync(info.Id);

                if (updateUserAsp != null)
                {
                    //Variable para asignacion de los roles
                    var roles = info.Roles;
                    //Nuevo dato a actualizar del usuario de Asp, solo mandamos el Nombre de usuario 
                    updateUserAsp.UserName = info.UserName;
                    //Nuevo dato para actualizar la contraseña
                    var changepassword = await userManager.ChangePasswordAsync(updateUserAsp, viejaPass, updateUserSistema.Cve);
                    //A través de estas acciones, vamos a obtener, remover y volver a agregar el listado de roles
                    //Method para obtención de los roles
                    var changeGetRoles = await userManager.GetRolesAsync(updateUserAsp);
                    //Method para eliminar los roles 
                    var resultDeleteRoles = await userManager.RemoveFromRolesAsync(updateUserAsp, changeGetRoles.ToList());
                    //Method para mandar el listado de roles
                    var resultAddRoles = await userManager.AddToRolesAsync(updateUserAsp, roles);
                    //Segundo parametros me pide un string de roles no un listado 

                    var resultado = await userManager.UpdateAsync(updateUserAsp);

                    if (!resultado.Succeeded)
                    {
                        context.Update(oldUser);
                        await context.SaveChangesAsync();
                        return BadRequest();
                    }
                }

                //Asignación del rol editado

                //Se retorna al usuario actualizado
                return Ok(updateUserSistema);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut("activar")]
        public async Task<ActionResult> PutActive([FromBody] UsuarioInfo info)
        {
            try
            {
                var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.Cod == info.UserCod);
                //Si el usuario no existe
                if (usuario == null)
                {
                    return NotFound();
                }
                usuario.Activo = info.Activo;
                //Función para actualizar el estado activo del usuario
                context.Update(usuario);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}

//   var updateUserSistema = await context.Usuario.FirstOrDefaultAsync(x => x.Cod == info.UserCod);