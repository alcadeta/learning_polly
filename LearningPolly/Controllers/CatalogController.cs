using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Bulkhead;
using System.Diagnostics;


namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private static int _requestCount;
        private readonly HttpClient _httpClient;
        private readonly AsyncBulkheadPolicy<HttpResponseMessage> _bulkheadIsolationPolicy;

        public CatalogController(
            HttpClient httpClient,
            AsyncBulkheadPolicy<HttpResponseMessage> bulkheadIsolationPolicy)
        {
            _httpClient = httpClient;
            _bulkheadIsolationPolicy = bulkheadIsolationPolicy;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;
            LogBulkheadInfo();
            var requestEndpoint = $"inventory/{id}";

            var response = await _bulkheadIsolationPolicy.ExecuteAsync(
                () => _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var itemsInStock = JsonConvert.DeserializeObject<int>(content);
                return Ok(itemsInStock);
            }

            return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private void LogBulkheadInfo()
        {
            Debug.WriteLine($"LearningPolly RequestCount: {_requestCount}");
            Debug.WriteLine("LearningPolly BulkheadAvailableCount: " +
                            _bulkheadIsolationPolicy.BulkheadAvailableCount);
            Debug.WriteLine("LearningPolly QueueAvailableCount: "+
                            _bulkheadIsolationPolicy.QueueAvailableCount);
        }
    }
}
