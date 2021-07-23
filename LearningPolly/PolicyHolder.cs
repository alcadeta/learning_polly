using System;
using System.Net.Http;
using Polly;

namespace LearningPolly
{
    public class PolicyHolder : IPolicyHolder
    {
        public IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        public IAsyncPolicy HttpClientTimeoutException { get; set;  }

        public PolicyHolder()
        {
            HttpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    (response, _) =>
                    {
                        var result = response.Result;
                        // Log the result.
                    });

            HttpClientTimeoutException = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    1,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    (exception, _) =>
                    {
                        var message = exception.Message;
                        // Log the message.
                    });
        }
    }
}