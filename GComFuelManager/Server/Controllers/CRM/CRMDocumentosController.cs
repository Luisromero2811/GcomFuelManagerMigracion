using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers.CRM
{
    public class CRMDocumentosController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> manager;
        private readonly IMapper mapper;

        public CRMDocumentosController(ApplicationDbContext context, UserManager<IdentityUsuario> manager, IMapper mapper)
        {
            this.context = context;
            this.manager = manager;
            this.mapper = mapper;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload_Files()
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

        [HttpGet("tipodocumento")]
        public async Task<ActionResult> GetAll([FromQuery] CRMTipoDocumentoDTO dTO)
        {
            try
            {
                var tiposDocumento = context.TipoDocumento
                    .AsNoTracking()
                    .OrderBy(x => x.Nombre)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    tiposDocumento = tiposDocumento.Where(v => v.Nombre!.ToLower().Contains(dTO.Nombre.ToLower()));

                if (dTO.Paginacion)
                {
                    await HttpContext.InsertarParametrosPaginacion(tiposDocumento, dTO.Registros_por_pagina, dTO.Pagina);
                    dTO.Pagina = HttpContext.ObtenerPagina();
                    tiposDocumento = tiposDocumento.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                }

                var tiposdto = tiposDocumento.Select(x => mapper.Map<CRMTipoDocumentoDTO>(x));
                return Ok(tiposdto);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

