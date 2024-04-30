using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ActivoController(ApplicationDbContext context)
        {
            this.context = context;
        }


    }
}
