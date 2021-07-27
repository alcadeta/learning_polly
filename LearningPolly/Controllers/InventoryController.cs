using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100);
            return Ok(id + 100);
        }
    }
}