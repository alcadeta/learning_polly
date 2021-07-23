using System;
using System.Net.Http;
using Polly;
using Polly.Retry;

namespace LearningPolly.Policies
{
    public class PolicyHolder
    {
        public AsyncRetryPolicy<HttpResponseMessage> HttpRetryPolicy { get; }
        public AsyncRetryPolicy HttpClientTimeoutException { get; }

        public PolicyHolder()
        {
            HttpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt));
            HttpClientTimeoutException = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    (exception, _) =>
                    {
                        var message = exception.Message;
                    });
        }
    }
}