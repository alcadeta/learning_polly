using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Registry;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncCachePolicy<HttpResponseMessage> _cachePolicy;

        public CatalogController(IPolicyRegistry<string> myRegistry, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cachePolicy = myRegistry.Get<AsyncCachePolicy<HttpResponseMessage>>("myLocalCachePolicy");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";

            var policyExecutionContext = new Context($"GetInventoryById-{id}");

            var response = await _cachePolicy.ExecuteAsync(_ => _httpClient.GetAsync(requestEndpoint), policyExecutionContext);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var itemsInStock = JsonConvert.DeserializeObject<int>(content);
                return Ok(itemsInStock);
            }

            return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
