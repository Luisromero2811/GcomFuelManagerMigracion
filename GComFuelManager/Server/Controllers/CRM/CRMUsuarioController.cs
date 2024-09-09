using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers.CRM
{
    public class CRMUsuarioController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUsuario> userManager;

        public CRMUsuarioController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> GetListUsers([FromQuery] CRMUsuarioDTO usuario)
        {
            try
            {


                return Ok();
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
                    return BadRequest("El usuario ya existe");
                }

                if (info.IsComercial)
                {
                    var comercial = context.CRMOriginadores.Find(info.IDOriginador);
                    if (comercial is null)
                    {
                        return BadRequest("El comercial seleccionado no existe");
                    }

                }
                else if (info.IsVendedor)
                {
                    var vendedor = context.CRMVendedores.Find(info.IDVendedor);
                    if (vendedor is null)
                    {
                        return BadRequest("El vendedor seleccionado no existe");
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
                    return BadRequest(result.Errors);
                }

                //Asignamos roles al usuario
                foreach (var rol in info.Roles)
                {
                    var crmRole = context.CRMRoles.FirstOrDefault(x => x.Nombre == rol.Nombre);
                    if (crmRole == null)
                    {
                        return BadRequest("El rol no existe");
                    }
                    var roleResult = await userManager.AddToRoleAsync(newuserAsp, crmRole.Nombre);
                    if (!roleResult.Succeeded)
                    {
                        return BadRequest(roleResult.Errors);
                    }
                }
                //Relación de ID Asp.Net a CRMComercial
                if (info.IsComercial)
                {
                    var originador = await context.CRMOriginadores.FindAsync(info.IDOriginador);
                    if (originador == null)
                    {
                        return BadRequest("El originador Seleccionado no existe");
                    }
                    originador.UserId = newuserAsp.Id;
                }
                else if (info.IsVendedor)
                {
                    var vendedor = await context.CRMVendedores.FindAsync(info.IDVendedor);
                    if (vendedor == null)
                    {
                        return BadRequest("El vendedor seleccionado no existe");
                    }
                    vendedor.UserId = newuserAsp.Id;
                }
                await context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

