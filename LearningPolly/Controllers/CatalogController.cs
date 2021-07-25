using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly HttpClient _httpClient;

        private readonly int _cachedResult = 0;

        public CatalogController(
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy,
            HttpClient httpClient)
        {
            _retryPolicy = retryPolicy;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";

            var host = Request
                .Headers
                .FirstOrDefault(h => h.Key == "Host")
                .Value;

            var userAgent = Request
                .Headers
                .FirstOrDefault(h => h.Key == "User-Agent")
                .Value;

            var contextDictionary = new Dictionary<string, object>
            {
                {"Host", host}, {"CatalogId", id}, {"UserAgent", userAgent}
            };

            var context = new Context("CatalogContext", contextDictionary);

            var response = await _retryPolicy.ExecuteAsync(
                _ => _httpClient.GetAsync(requestEndpoint),
                context);

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
