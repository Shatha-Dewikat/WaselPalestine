using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Wasel_Palestine.PL.Area.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class test : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello, World!");
        }
    }
}
