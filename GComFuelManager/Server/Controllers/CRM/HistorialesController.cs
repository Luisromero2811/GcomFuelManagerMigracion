using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialesController : Controller
    {
        private readonly ApplicationDbContext context;

        public HistorialesController(ApplicationDbContext context)
        {
            this.context = context;
        }
    }
}

