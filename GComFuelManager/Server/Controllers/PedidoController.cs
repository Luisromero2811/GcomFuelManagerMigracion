using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public PedidoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<ActionResult> Get()
        {
            var pedidos = await context.OrdenEmbarques.ToListAsync();
            return Ok(pedidos);
        }
    }
}
