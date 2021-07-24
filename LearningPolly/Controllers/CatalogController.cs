using System;
using System.Diagnostics;
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
using Polly.Wrap;

namespace LearningPolly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly AsyncTimeoutPolicy<HttpResponseMessage> _timeoutPolicy;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policyWrap;

        private readonly int _cachedResult = 0;

        public CatalogController()
        {
            _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1);

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

            _policyWrap = Policy.WrapAsync(_fallbackPolicy, _retryPolicy, _timeoutPolicy);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var requestEndpoint = $"inventory/{id}";
            var httpClient = GetHttpClient();

            var response = await _policyWrap.ExecuteAsync(
                token => httpClient.GetAsync(requestEndpoint, token),
                CancellationToken.None);

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
