using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        readonly AsyncTimeoutPolicy _timeoutPolicy;
        readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;

        readonly int _cachedResult = 0;

        public CatalogController()
        {
            // throws `TimeoutRejectedException` if timeout of 1 second is exceeded.
            _timeoutPolicy = Policy.TimeoutAsync(1);

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            _fallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent(
                            _cachedResult.GetType(),
                            _cachedResult,
                            new JsonMediaTypeFormatter())
                    });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            var requestEndpoint = $"inventory/{id}";

            var response = await _fallbackPolicy.ExecuteAsync(
                () => _retryPolicy.ExecuteAsync(
                    () => _timeoutPolicy.ExecuteAsync(
                        async token => await httpClient.GetAsync(requestEndpoint, token),
                        CancellationToken.None)));

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
