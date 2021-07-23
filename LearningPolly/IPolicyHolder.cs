using System.Net.Http;
using Polly;

namespace LearningPolly
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        IAsyncPolicy HttpClientTimeoutException { get; set; }
    }
}