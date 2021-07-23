using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearningPolly.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace LearningPolly.Tests
{
    public class Tests
    {
        [Test]
        public async Task TestGet()
        {
            // Arrange:
            var fakeInventoryResponse = 15;
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(
                    Task.FromResult(
                        new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(
                                fakeInventoryResponse.ToString(),
                                Encoding.UTF8,
                                "application/json")
                        }));

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = new Uri(@"http://some.address.com/v1/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var mockPolicyHolder = new Mock<PolicyHolder>();
            mockPolicyHolder.SetupAllProperties();
            mockPolicyHolder.Object.HttpRetryPolicy =
                Policy.NoOpAsync<HttpResponseMessage>();
            mockPolicyHolder.Object.HttpClientTimeoutException =
                Policy.NoOpAsync();

            var controller = new CatalogController(
                mockPolicyHolder.Object,
                httpClient);

            // Act:
            var result = await controller.Get(2);

            // Assert:
            var resultObject = (OkObjectResult) result;
            Assert.NotNull(resultObject);

            var number = (int) resultObject.Value;
            Assert.AreEqual(number, 15);
        }
    }
}