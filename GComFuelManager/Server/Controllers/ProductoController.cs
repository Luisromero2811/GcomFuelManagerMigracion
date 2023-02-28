using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ProductoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var productos = await context.Producto.Where(x=>x.Activo == true).ToListAsync();
                return Ok(productos);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
