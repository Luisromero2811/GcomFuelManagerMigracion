using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GComFuelManager.Server.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    }
}

