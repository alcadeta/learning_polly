using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private int _getRequestCount;
        private int _deleteRequestCount;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100);
            _getRequestCount++;

            return _getRequestCount % 4 == 0
                ? Ok(15)
                : StatusCode(
                    (int) HttpStatusCode.InternalServerError,
                    "Something went wrong when getting.");
        }
    }
}