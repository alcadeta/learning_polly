using System.Net.Http;
using Polly;
using Polly.Wrap;

namespace LearningPolly
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> RetryPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> FallbackPolicy { get; set; }
        IPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }
    }
}