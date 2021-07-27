using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public CatalogController(
            HttpClient httpClient,
            AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy)
        {
            _httpClient = httpClient;
            _circuitBreakerPolicy = circuitBreakerPolicy;
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";

            var response = await _retryPolicy.ExecuteAsync(
                () => _circuitBreakerPolicy.ExecuteAsync(
                    () => _httpClient.GetAsync(requestEndpoint)));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var itemsInStock = JsonConvert.DeserializeObject<int>(content);
                return Ok(itemsInStock);
            }

            return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpGet("pricing/{id}")]
        public async Task<IActionResult> GetPricing(int id)
        {
            var requestEndpoint = $"pricing/{id}";

            var response = await _retryPolicy.ExecuteAsync(
                () => _circuitBreakerPolicy.ExecuteAsync(
                    () => _httpClient.GetAsync(requestEndpoint)));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var priceOfItem = JsonConvert.DeserializeObject<decimal>(responseContent);
                return Ok($"${priceOfItem}");
            }

            return StatusCode((int) response.StatusCode, responseContent);
        }
    }
}
