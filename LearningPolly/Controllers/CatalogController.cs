using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly HttpClient _httpClient;

        public CatalogController(
            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy,
            HttpClient httpClient)
        {
            _httpRetryPolicy = httpRetryPolicy;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";

            var response = await _httpRetryPolicy.ExecuteAsync(
                () => _httpClient.GetAsync(requestEndpoint));

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
