using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LearningPolly.Controllers
{
    [Route("api/pricing")]
    public class PricingController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            await Task.Delay(100);
            return Ok(id + 10.27);
        }
    }
}