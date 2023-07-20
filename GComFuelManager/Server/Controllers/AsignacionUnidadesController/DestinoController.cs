using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GComFuelManager.Server.Helpers;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using GComFuelManager.Shared.GamoModels;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DestinoController : ControllerBase 
	{
        private readonly ApplicationDbContext context;
        private readonly VerifyUserToken verifyUser;

        public DestinoController(ApplicationDbContext context, VerifyUserToken verifyUser)
		{
            this.context = context;
            this.verifyUser = verifyUser;
        }
        
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var destinos = await context.Destino
                    .Where(x => x.Activo == true)
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den })
                    .ToListAsync();
                return Ok(destinos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("comprador")]
        public ActionResult GetClientComprador()
        {
            try
            {
                var userId = verifyUser.GetName(HttpContext);
                
                if (string.IsNullOrEmpty(userId))
                    return BadRequest();

                var user = context.Usuario.FirstOrDefault(x => x.Usu == userId);
                if (user == null)
                    return BadRequest();

                var clientes = context.Destino.Where(x => x.Codcte == user!.CodCte).AsEnumerable().OrderBy(x => x.Den);
                
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                {
                    return BadRequest();
                }

                var destino = context.Destino.Where(x => x.Cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }
                destino.Activo = status;

                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("gamo")]
        public async Task<ActionResult> GetGamo()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] code = Encoding.ASCII.GetBytes("apisimsa@ubiquite.mx:UA3VbQrbENWF62d");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(code));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    Task<HttpResponseMessage> get = client.GetAsync("https://energas.ubiquite.mx/api/energasB2b/destinos");
                    var response = JsonConvert.DeserializeObject<DestinoGamoList>(await get.Result.Content.ReadAsStringAsync());

                    foreach (var item in response.Destinos)
                    {
                        if(item.IdDestinoTuxpan != 0)
                        {
                            var destino = context.Destino.Where(x => x.Den.ToLower().Contains(item.Nombre.ToLower()) &&
                                x.Cliente.Den.ToLower().Contains(item.RazonSocial.ToLower()))
                                .Include(x=>x.Cliente)
                                .FirstOrDefault();
                            if(destino != null)
                            {
                                destino.CodGamo = item.IdDestinoTuxpan;

                                context.Update(destino);
                            }
                        }
                    }
                    
                    await context.SaveChangesAsync();

                    return Ok(response);
                }
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}

