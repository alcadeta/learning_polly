using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;

            // simulate some data processing by delaying for 100 milliseconds
            await Task.Delay(100);

            // only one of out four requests will succeed
            return _requestCount % 4 == 0
                ? Ok(15)
                : StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
        }
    }
}