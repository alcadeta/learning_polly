using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private static int _requestCount;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100);
            _requestCount++;

            return _requestCount % 4 == 0
                ? Ok(15)
                : StatusCode((int) HttpStatusCode.InternalServerError, "Something Went Wrong");
        }
    }
}