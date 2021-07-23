using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IPolicyHolder _policyHolder;
        private readonly HttpClient _httpClient;

        public CatalogController(
            IPolicyHolder policyHolder,
            HttpClient httpClient)
        {
            _policyHolder = policyHolder;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";

            var response = await _policyHolder.HttpRetryPolicy.ExecuteAsync(
                () => _policyHolder.HttpClientTimeoutException.ExecuteAsync(
                    token => _httpClient.GetAsync(requestEndpoint, token),
                    CancellationToken.None));

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

        // private HttpClient GetHttpClient()
        // {
        //     var httpClient = new HttpClient();
        //     httpClient.BaseAddress = new Uri(@"http://localhost:5000/api/");
        //     httpClient.DefaultRequestHeaders.Accept.Clear();
        //     httpClient.DefaultRequestHeaders.Accept.Add(
        //         new MediaTypeWithQualityHeaderValue("application/json"));
        //     return httpClient;
        // }
    }
}
