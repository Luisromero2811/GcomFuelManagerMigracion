using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GComFuelManager.Server.Controllers.Images
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImageController : ControllerBase
    {
        public ImageController()
        {
        }

        [HttpGet("logo"), AllowAnonymous]
        public IActionResult GetLogo()
        {
            byte[] imageArray = System.IO.File.ReadAllBytes("./imgs/Triptico_EnergasEDITABLE-03.png");
            //string base64Image = Convert.ToBase64String(imageArray);
            //return base64Image;
            return File(imageArray, "image/png");
        }
    }
}
