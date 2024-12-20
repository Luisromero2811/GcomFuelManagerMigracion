using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GComFuelManager.Client.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMFormJuridicoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CRMFormJuridicoController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create_KnowYourCustomer([FromBody] ConoceClienteOportunidadPostDTO crmClienteOportunidad)
        {
            try
            {
                var actividad = mapper.Map<ConoceClienteOportunidadPostDTO, ConoceClienteOportunidad>(crmClienteOportunidad);

                var oportunidad = await context.CRMOportunidades
                    .Include(x => x.ConoceClienteOportunidad)
                    .FirstOrDefaultAsync(x => x.Id == crmClienteOportunidad.OportunidadId);

                if (oportunidad is null)
                {
                    return BadRequest("La oportunidad asociada no existe");
                }

                actividad.OportunidadId = crmClienteOportunidad.OportunidadId;

                if (oportunidad.ConoceClienteOportunidad != null)
                {
                    context.Entry(oportunidad.ConoceClienteOportunidad).CurrentValues.SetValues(actividad);
                    actividad = oportunidad.ConoceClienteOportunidad;
                }
                else
                {
                    await context.AddAsync(actividad);
                }

                var documentos = new List<(int? DocumentoId, string NombrePropiedad)>
                {
                     (crmClienteOportunidad.DocumentoIdEtica1, nameof(crmClienteOportunidad.DocumentoIdEtica1)),
                     (crmClienteOportunidad.DocumentoIdEtica2, nameof(crmClienteOportunidad.DocumentoIdEtica2)),
                     (crmClienteOportunidad.DocumentoIdEtica3, nameof(crmClienteOportunidad.DocumentoIdEtica3)),
                     (crmClienteOportunidad.DocumentoIdAdicional1, nameof(crmClienteOportunidad.DocumentoIdAdicional1)),
                     (crmClienteOportunidad.DocumentoIdAdicional2, nameof(crmClienteOportunidad.DocumentoIdAdicional2)),
                     (crmClienteOportunidad.DocumentoIdAdicional3, nameof(crmClienteOportunidad.DocumentoIdAdicional3)),
                     (crmClienteOportunidad.DocumentoIdAdicional4, nameof(crmClienteOportunidad.DocumentoIdAdicional4)),
                     (crmClienteOportunidad.DocumentoIdAdicional5, nameof(crmClienteOportunidad.DocumentoIdAdicional5)),
                     (crmClienteOportunidad.DocumentoIdAdicional6, nameof(crmClienteOportunidad.DocumentoIdAdicional6)),
                     (crmClienteOportunidad.DocumentoIdAdicional7, nameof(crmClienteOportunidad.DocumentoIdAdicional7)),
                     (crmClienteOportunidad.DocumentoIdAdicional8, nameof(crmClienteOportunidad.DocumentoIdAdicional8)),
                     (crmClienteOportunidad.DocumentoIdAdicional9, nameof(crmClienteOportunidad.DocumentoIdAdicional9)),
                     (crmClienteOportunidad.DocumentoIdAdicional10, nameof(crmClienteOportunidad.DocumentoIdAdicional10)),
                     (crmClienteOportunidad.DocumentoIdAdicional11, nameof(crmClienteOportunidad.DocumentoIdAdicional11)),
                     (crmClienteOportunidad.DocumentoIdAdicional12, nameof(crmClienteOportunidad.DocumentoIdAdicional12))
                };

                foreach (var (documentoId, propiedad) in documentos)
                {
                    if (!documentoId.IsZero())
                    {
                        var doc = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == documentoId);
                        if (doc is not null)
                        {
                            // Asignar el tipo de documento basado en la propiedad
                            var tipoDocumento = propiedad switch
                            {
                                nameof(crmClienteOportunidad.DocumentoIdEtica1) => "Etica1",
                                nameof(crmClienteOportunidad.DocumentoIdEtica2) => "Etica2",
                                nameof(crmClienteOportunidad.DocumentoIdEtica3) => "Etica3",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional1) => "Adicional1",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional2) => "Adicional2",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional3) => "Adicional3",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional4) => "Adicional4",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional5) => "Adicional5",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional6) => "Adicional6",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional7) => "Adicional7",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional8) => "Adicional8",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional9) => "Adicional9",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional10) => "Adicional10",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional11) => "Contrato",
                                nameof(crmClienteOportunidad.DocumentoIdAdicional12) => "ContratoFinal",
                                _ => string.Empty,
                            };

                            // Verifica si el documento ya está asociado
                            var existingDocopo = await context.CRMConoceClienteDocumentos
                                .FirstOrDefaultAsync(x => x.DocumentoId == doc.Id && x.ConoceClienteId == actividad.Id);


                            if (existingDocopo == null)
                            {
                                var docopo = new CRMConoceClienteDocumentos
                                {
                                    DocumentoId = doc.Id,
                                    ConoceCliente = actividad,
                                };

                                // Actualizar el tipo de documento si es necesario
                                if (!string.IsNullOrEmpty(tipoDocumento))
                                {
                                    doc.Identificador = tipoDocumento;
                                    context.CRMDocumentos.Update(doc);
                                }

                                await context.AddAsync(docopo);
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("verificar/{oportunidadId}")]
        public async Task<ActionResult> VerificarFormularioConoceCliente([FromRoute] int? oportunidadId)
        {
            try
            {
                var conoceCliente = await context.ConoceClienteOportunidad
                               .FirstOrDefaultAsync(x => x.OportunidadId == oportunidadId);

                if (conoceCliente is null)
                {
                    return NotFound();
                }

                return Ok(conoceCliente);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("obtenerform/{Id:int}")]
        public async Task<ActionResult> ObtenerForm([FromRoute] int Id)
        {
            try
            {
                var conocecliente = await context.ConoceClienteOportunidad
                    .Include(x => x.Documentos)
                    .FirstOrDefaultAsync(x => x.OportunidadId == Id);


                if (conocecliente is null)
                {
                    return NotFound();
                }

                conocecliente.Documentos = conocecliente.Documentos
                   .OrderByDescending(d => d.FechaCreacion)
                   .ToList();

                // Mapea al DTO
                var dto = mapper.Map<ConoceClienteOportunidadPostDTO>(conocecliente);


                return Ok(dto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipocliente")]
        public async Task<ActionResult> ObtenerTipoCliente()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Tipo_Cliente"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Tipo de Cliente");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/giro")]
        public async Task<ActionResult> ObtenerGiro()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Giro"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Giro");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipoentrega")]
        public async Task<ActionResult> ObtenerTipoEntrega()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Tipo_Entrega"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Tipo de Entrega");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/suministro")]
        public async Task<ActionResult> ObtenerSuministro()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Suministro"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Suministro");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/pago")]
        public async Task<ActionResult> ObtenerPago()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Pago"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Pago");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/metodopago")]
        public async Task<ActionResult> ObtenerMetodoPago()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Metodo_Pago"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Método Pago");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/formapago")]
        public async Task<ActionResult> ObtenerFormaPago()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Forma_Pago"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el Forma Pago");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/CFDI")]
        public async Task<ActionResult> ObtenerCFDI()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Uso_CFDI"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para el CFDI");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/InfoEtica")]
        public async Task<ActionResult> ObtenerInfoEtica()
        {
            try
            {
                var catalogo = await context.CRMCatalogos
                    .AsNoTracking()
                    .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Informacion_Etica_Cliente"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para Información Etica del Cliente");
                }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

