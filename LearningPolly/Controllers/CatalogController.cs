using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Polly.Fallback;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private HttpClient _httpClient;
        private AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private AsyncFallbackPolicy<HttpResponseMessage> _httpFallbackPolicy;

        private int _cachedNumber = 0;

        public CatalogController()
        {
            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3);

            _httpFallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .FallbackAsync(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent(
                            _cachedNumber.GetType(),
                            _cachedNumber,
                            new JsonMediaTypeFormatter())
                    });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            _httpClient = GetHttpClient("BadAuthCode");
            var requestEndpoint = $"inventory/{id}";

            // Note: though chaining policies like this is reasonable, PolicyWrap is the idiomatic
            // way of doing it. PolicyWrap will be covered later on.
            var response = await _httpFallbackPolicy
                .ExecuteAsync(
                    () => _httpRetryPolicy.ExecuteAsync(
                        () => _httpClient.GetAsync(requestEndpoint)));

            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(
                    await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private HttpClient GetHttpClient(string authCode)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookieContainer};
            cookieContainer.Add(new Uri("http://localhost"), new Cookie("Auth", authCode));

            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(@"http://localhost:5000/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
