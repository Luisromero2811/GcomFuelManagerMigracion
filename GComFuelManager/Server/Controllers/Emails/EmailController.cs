using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using RazorHtmlEmails.Common;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;

namespace GComFuelManager.Server.Controllers.Emails
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IRazorViewToStringRenderer razorView;
        private readonly IRegisterAccountService registerAccount;
        private readonly IVencimientoService vencimientoService;
        private readonly IPreciosService preciosService;
        private readonly IConfirmOrden confirmOrden;
        private readonly IConfirmarCreacionOrdenes confirmarCreacion;
        private readonly IDenegarCreacionOrdenes denegarCreacion;
        private readonly IConfirmarPrecio confirmarPrecio;

        public EmailController(ApplicationDbContext context,
            IRazorViewToStringRenderer razorView,
            IRegisterAccountService registerAccount,
            IVencimientoService vencimientoService,
            IPreciosService preciosService,
            IConfirmOrden confirmOrden,
            IConfirmarCreacionOrdenes confirmarCreacion,
            IDenegarCreacionOrdenes denegarCreacion,
            IConfirmarPrecio confirmarPrecio)
        {
            this.context = context;
            this.razorView = razorView;
            this.registerAccount = registerAccount;
            this.vencimientoService = vencimientoService;
            this.preciosService = preciosService;
            this.confirmOrden = confirmOrden;
            this.confirmarCreacion = confirmarCreacion;
            this.denegarCreacion = denegarCreacion;
            this.confirmarPrecio = confirmarPrecio;
        }

        [HttpPost("confirmacion")]
        public async Task<ActionResult> SendEmailConfirmacion([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new();
                int? VolumenTotal = 0;
                List<MailboxAddress> ToList = new();
                foreach (var cliente in ordenCierres.DistinctBy(x => x.CodCte))
                {
                    ToList = context.AccionCorreo.Where(x => x.Contacto != null && x.Contacto.CodCte == cliente.CodCte && x.Contacto.Estado == true
                                        && x.Accion != null && x.Accion.Nombre.Equals("Compra"))
                                            .Include(x => x.Accion)
                                            .Include(x => x.Contacto)
                                            .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo))
                                            .ToList();

                    if (ToList is null || ToList.Count() == 0)
                        return BadRequest($"{cliente.Cliente?.Den}, No cuenta con un correo activo o registrado");
                }

                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();

                emailContent.CC = cc;

                //Funcion para el volumen
                IEnumerable<OrdenCierre> cierresDistinc = ordenCierres.DistinctBy(x => x.Producto!.Den);

                foreach (var item in cierresDistinc)
                {
                    foreach (var cierre in ordenCierres)
                        if (cierre.Producto!.Den == item.Producto!.Den)
                            VolumenTotal += cierre.Volumen;
                    cierresDistinc.FirstOrDefault(x => x.Producto!.Den == item.Producto!.Den)!.Volumen = VolumenTotal;
                    VolumenTotal = 0;
                }

                emailContent.ToList = ToList;
                emailContent.Subject = "Confirmacion de compra";
                emailContent.Lista = cierresDistinc;

                await registerAccount.Register(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmacion/grupo")]
        public async Task<ActionResult> SendEmailConfirmacionGrupo([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new();
                int? VolumenTotal = 0;
                List<MailboxAddress> ToList = new();

                foreach (var i in ordenCierres)
                {
                    var ctes = context.Cliente.Where(x => x.codgru == i.CodGru).ToList();
                    foreach (var item in ctes)
                    {
                        var emails = context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == item.Cod && x.Contacto.Estado == true
                    && x.Accion.Nombre.Equals("Compra"))
                        .Include(x => x.Accion)
                        .Include(x => x.Contacto)
                        .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo))
                        .ToList();
                        ToList.AddRange(emails);
                    }

                    if (ToList is null || ToList.Count == 0)
                        return BadRequest($"{i.Grupo?.Den}, No cuenta con un correo activo o registrado");
                }

                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();

                emailContent.CC = cc;

                //Funcion para el volumen
                IEnumerable<OrdenCierre> cierresDistinc = ordenCierres.DistinctBy(x => x.Producto!.Den);

                foreach (var item in cierresDistinc)
                {
                    foreach (var cierre in ordenCierres)
                        if (cierre.Producto!.Den == item.Producto!.Den)
                            VolumenTotal = VolumenTotal + cierre.Volumen;
                    cierresDistinc.FirstOrDefault(x => x.Producto!.Den == item.Producto!.Den)!.Volumen = VolumenTotal;
                    VolumenTotal = 0;
                }

                emailContent.ToList = ToList;
                emailContent.Subject = "Confirmacion de compra";
                emailContent.Lista = cierresDistinc;

                await registerAccount.Register(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("preciosGroup")]
        public async Task<ActionResult> SendEmailGroupPrecio([FromQuery] CodDenDTO grupo)
        {
            try
            {
                EmailContent<Precio> emailContent = new();
                List<MailboxAddress> ToList = new();
                List<Precio> list = new();
                var gpo = context.Grupo.FirstOrDefault(x =>(!string.IsNullOrEmpty(x.Den) && !string.IsNullOrWhiteSpace(x.Den)) && x.Den.ToLower().Equals(grupo.Den));
                if (gpo is not null)
                {
                    var ctes = context.Cliente.Where(x => x.codgru == gpo.Cod).ToList();
                    foreach (var item in ctes)
                    {
                        ToList = await context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == item.Cod && x.Contacto.Estado == true
                        && x.Accion.Nombre.Equals("Precios"))
                            .Include(x => x.Accion)
                            .Include(x => x.Contacto)
                            .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo))
                            .ToListAsync();
                        if (ToList is null || ToList.Count == 0)
                        {
                            return BadRequest($"No se encontro un correo con la accion de Precios para el cliente");
                        }
                        var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();

                        list = context.Precio.Where(x => x.codCte == item.Cod).Include(x => x.Cliente).Include(x => x.Producto).Include(x => x.Destino).ToList();
                        if (list.Count == 0)
                        {
                            return BadRequest($"No se encontraron precios para los clientes del grupo {gpo.Den}");
                        }
                        emailContent.CC = cc;

                        emailContent.ToList = ToList;

                        emailContent.Subject = "Listado de precios";
                        emailContent.Lista = list;

                        await preciosService.NotifyPrecio(emailContent);
                    }
                }
                else
                {
                    return BadRequest($"No se encontraron clientes del grupo {gpo?.Den}");
                }
                return Ok(list);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("precios")]
        public async Task<ActionResult> SendEmailPrecios([FromQuery] CodDenDTO cliente)
        {
            try
            {
                List<Precio> list = new();
                var cte = context.Cliente.Where(x =>(!string.IsNullOrEmpty(x.Den) && !string.IsNullOrWhiteSpace(x.Den)) && x.Den.ToLower().Equals(cliente.Den)).FirstOrDefault();

                if (cte is not null)
                    list = context.Precio.Where(x => x.codCte == cte.Cod && x.Activo == true)
                        .Include(x => x.Cliente)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.Zona)
                        .ToList();
                else
                    return BadRequest($"No se encontro el cliente {cliente.Den}");

                if (list.Count == 0)
                    return BadRequest($"No se encontraron precios para {cte.Den}");

                EmailContent<Precio> emailContent = new();

                var ToList = await context.AccionCorreo.Where(x =>x.Contacto != null && x.Contacto.CodCte == cte.Cod && x.Contacto.Estado == true
                   && x.Accion != null && x.Accion.Nombre.Equals("Precios"))
                       .Include(x => x.Accion)
                       .Include(x => x.Contacto)
                       .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo))
                       .ToListAsync();

                if (ToList is null || ToList.Count == 0)
                    return BadRequest($"No se encontro un correo con la accion de 'Precios' para el cliente {cte.Den}");

                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                emailContent.CC = cc;

                emailContent.Subject = "Listado de precios";
                emailContent.Lista = list;
                emailContent.ToList = ToList;

                await preciosService.NotifyPrecio(emailContent);

                return Ok(list);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("preciosgroup/programados")]
        public async Task<ActionResult> SendEmailGroupProgramado([FromQuery] CodDenDTO grupo)
        {

            try
            {
                EmailContent<Precio> emailContent = new();
                List<MailboxAddress> ToList = new();
                List<Precio> list = new();
                var gpo = context.Grupo.Where(x => x.Den.ToLower().Equals(grupo.Den)).FirstOrDefault();
                if (gpo is not null)
                {
                    var ctes = context.Cliente.Where(x => x.codgru == gpo.Cod).ToList();
                    foreach (var item in ctes)
                    {
                        ToList = await context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == item.Cod && x.Contacto.Estado == true
                        && x.Accion.Nombre.Equals("Precios"))
                            .Include(x => x.Accion)
                            .Include(x => x.Contacto)
                            .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                            .ToListAsync();
                        if (ToList is null || ToList.Count == 0)
                        {
                            return BadRequest($"No se encontro un correo con la accion de Precios para el cliente");
                        }
                        var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();

                        list = context.PrecioProgramado.Where(x => x.codCte == item.Cod).Include(x => x.Cliente).Include(x => x.Producto).Include(x => x.Destino)
                               .Select(x => new Precio
                               {
                                   codPrd = x.codPrd,
                                   codZona = x.codZona,
                                   codDes = x.codDes,
                                   codCte = x.codCte,
                                   Pre = x.Pre,
                                   FchDia = x.FchDia,
                                   FchActualizacion = x.FchActualizacion,
                                   Activo = x.Activo,
                                   Cliente = x.Cliente,
                                   Destino = x.Destino,
                                   Producto = x.Producto,
                                   Zona = x.Zona,
                               }).ToList();
                        if (list.Count == 0)
                        {
                            return BadRequest($"No se encontraron precios para los clientes del grupo {gpo.Den}");
                        }
                        emailContent.CC = cc;

                        emailContent.ToList = ToList;

                        emailContent.Subject = "Listado de precios";
                        emailContent.Lista = list;

                        await preciosService.NotifyPrecio(emailContent);
                    }
                }
                else
                {
                    return BadRequest($"No se encontraron clientes del grupo {gpo.Den}");
                }
                return Ok(list);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("precios/programados")]
        public async Task<ActionResult> SendEmailPreciosPro([FromQuery] CodDenDTO cliente)
        {
            try
            {
                List<Precio> list = new();
                var cte = context.Cliente.Where(x => (!string.IsNullOrEmpty(x.Den) && !string.IsNullOrWhiteSpace(x.Den)) && x.Den.ToLower().Equals(cliente.Den)).FirstOrDefault();

                if (cte is not null)
                    list = context.PrecioProgramado.Where(x => x.codCte == cte.Cod && x.Activo == true)
                        .Include(x => x.Cliente)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.Zona)
                        .Select(x => new Precio
                        {
                            codPrd = x.codPrd,
                            codZona = x.codZona,
                            codDes = x.codDes,
                            codCte = x.codCte,
                            Pre = x.Pre,
                            FchDia = x.FchDia,
                            FchActualizacion = x.FchActualizacion,
                            Activo = x.Activo,
                            Cliente = x.Cliente,
                            Destino = x.Destino,
                            Producto = x.Producto,
                            Zona = x.Zona,
                        })
                        .ToList();
                else
                    return BadRequest($"No se encontro el cliente {cliente.Den}");

                if (list.Count == 0)
                    return BadRequest($"No se encontraron precios para {cte.Den}");

                EmailContent<Precio> emailContent = new();

                var ToList = await context.AccionCorreo.Where(x => x.Contacto != null && x.Contacto.CodCte == cte.Cod && x.Contacto.Estado == true
                   && x.Accion != null && x.Accion.Nombre.Equals("Precios"))
                       .Include(x => x.Accion)
                       .Include(x => x.Contacto)
                       .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo))
                       .ToListAsync();

                if (ToList is null || ToList.Count == 0)
                    return BadRequest($"No se encontro un correo con la accion de 'Precios' para el cliente {cte.Den}");

                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                emailContent.CC = cc;

                emailContent.Subject = "Listado de precios";
                emailContent.Lista = list;
                emailContent.ToList = ToList;

                await preciosService.NotifyPrecio(emailContent);

                return Ok(list);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmorden")]
        public async Task<ActionResult> SendEmailConfirmOrden([FromBody] List<OrdenEmbarque> ordenEmbarques)
        {
            try
            {
                var clientes = ordenEmbarques.DistinctBy(x => x.OrdenCierre.CodCte).Select(x => x.OrdenCierre.CodCte);
                foreach (var item in clientes)
                {
                    var list = ordenEmbarques.Where(x => x.OrdenCierre.CodCte == item);

                    EmailContent<OrdenEmbarque> emailContent = new();

                    var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                    emailContent.CC = cc;

                    var ToList = context.AccionCorreo.Where(x => x.Contacto.CodCte == ordenEmbarques.FirstOrDefault().OrdenCierre.CodCte && x.Contacto.Estado == true
                        && x.Accion.Nombre.Equals("Confirmacion Orden"))
                            .Include(x => x.Accion)
                            .Include(x => x.Contacto)
                            .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                            .AsEnumerable();

                    var contacto = context.Contacto.FirstOrDefault(x => x.CodCte == ordenEmbarques.FirstOrDefault()!.OrdenCierre.CodCte && x.Estado == true);
                    if (contacto is null)
                        return BadRequest($"{ordenEmbarques.FirstOrDefault(x => x.OrdenCierre.CodCte == item).Cliente.Den} No tiene un contacto asignado");

                    //emailContent.Nombre = contacto.Nombre;
                    //emailContent.Email = contacto.Correo;
                    emailContent.Subject = "Autorizacion de orden";
                    emailContent.Lista = list;
                    emailContent.ToList = ToList;

                    await confirmOrden.NotifyConfirmOrden(emailContent);

                }

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmar/creacion/ordenes")]
        public async Task<ActionResult> SendEmailConfirmarCreacionOrdenes(OrdenCierre cierre)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new();

                var cc = context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == 0 && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("Confirmar Creacion Ordenes"))
                    .Include(x => x.Contacto)
                    .Include(x => x.Accion)
                    .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo)).AsEnumerable();

                emailContent.CC = new List<MailboxAddress>();
                emailContent.ToList = cc;
                emailContent.Subject = "Creacion de ordenes de cierre";
                emailContent.Item = cierre;

                await confirmarCreacion.Confirmar(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("denegar/creacion/ordenes")]
        public async Task<ActionResult> SendEmailDenegarCreacionOrdenes(OrdenCierre cierre)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new();

                var cc = context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == 0 && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("Denegar Creacion Ordenes"))
                    .Include(x => x.Contacto)
                    .Include(x => x.Accion)
                    .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo)).AsEnumerable();

                emailContent.CC = new List<MailboxAddress>();
                emailContent.ToList = cc;
                emailContent.Subject = "Cierre pendiente";
                emailContent.Item = cierre;

                await denegarCreacion.Denegar(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmar/precio")]
        public async Task<ActionResult> SendEmailConfirmarPrecioOrdenCierre(OrdenCierre cierre)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new();

                var cc = context.AccionCorreo.Where(x => x.Contacto != null && x.Accion != null && x.Contacto.CodCte == 0 && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("Confirmar Precio"))
                    .Include(x => x.Contacto)
                    .Include(x => x.Accion)
                    .Select(x => new MailboxAddress(x.Contacto!.Nombre, x.Contacto.Correo)).ToList();

                emailContent.CC = new List<MailboxAddress>();
                emailContent.ToList = cc;
                emailContent.Subject = "Precio por confirmar";
                emailContent.Item = cierre;

                await confirmarPrecio.ConfirmarPrecio(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
