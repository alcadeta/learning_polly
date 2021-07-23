using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Registry;
using Polly.Retry;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly PolicyRegistry _policyRegistry;

        public CatalogController(PolicyRegistry policyRegistry)
        {
            _policyRegistry = policyRegistry;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            var requestEndpoint = $"inventory/{id}";

            var retryPolicy = _policyRegistry
                .Get<AsyncRetryPolicy<HttpResponseMessage>>("SimpleWaitAndRetry");

            var response = await retryPolicy.ExecuteAsync(
                () => httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(
                    await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode(
                (int) response.StatusCode,
                response.Content.ReadAsStringAsync());
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"http://localhost:5000/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
