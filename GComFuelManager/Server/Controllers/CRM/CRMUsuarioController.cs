using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GComFuelManager.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers.CRM
{
    public class CRMUsuarioController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CRMUsuarioController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
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

    }
}

