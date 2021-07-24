using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace LearningPolly
{
    public class PolicyHolder : IPolicyHolder
    {
        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set;  }
        public IAsyncPolicy<HttpResponseMessage> RetryPolicy { get; set; }
        public IAsyncPolicy<HttpResponseMessage> FallbackPolicy { get; set; }

        public IPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }

        private readonly int _cachedResult = 0;

        public PolicyHolder()
        {
            TimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1);

            RetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            FallbackPolicy = Policy
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

            TimeoutRetryAndFallbackWrap = Policy
                .WrapAsync(FallbackPolicy, RetryPolicy, TimeoutPolicy);
        }
    }
}