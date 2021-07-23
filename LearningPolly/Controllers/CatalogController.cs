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
        private readonly AsyncTimeoutPolicy _timeoutPolicy;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policyWrap;

        private readonly int _cachedResult = 0;

        public CatalogController()
        {
            _timeoutPolicy = Policy
                .TimeoutAsync(1, onTimeoutAsync: TimeoutDelegate);

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3, onRetry: RetryDelegate);

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
                    },
                    onFallbackAsync: FallbackDelegate);

            _policyWrap = _fallbackPolicy
                .WrapAsync(_retryPolicy
                    .WrapAsync(_timeoutPolicy));
        }

        private Task TimeoutDelegate(
            Context arg1, TimeSpan arg2, Task arg3)
        {
            Debug.WriteLine("In TimeoutAsync");
            return Task.CompletedTask;
        }

        private void RetryDelegate(
            DelegateResult<HttpResponseMessage> arg1,
            int arg2)
        {
            Debug.WriteLine("In RetryAsync");
        }

        private Task FallbackDelegate(DelegateResult<HttpResponseMessage> arg)
        {
            Debug.WriteLine("In FallbackAsync");
            return Task.CompletedTask;
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
