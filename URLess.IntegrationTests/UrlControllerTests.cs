using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using URLess.IntegrationTests.WebApplicationFactory;
using URLessCore.Models.RequestModels;
using URLessCore.Models.ResponseModels;

namespace URLess.IntegrationTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    [TestFixture]
    public class UrlControllerTests : IDisposable
    {
        private IntegrationTestWebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetup() => _factory = new IntegrationTestWebApplicationFactory<Program>();

        [SetUp]
        public void Setup() => _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        [OneTimeTearDown]
        public void Dispose() => _factory?.Dispose();


        [TestCase("https://nice.com/")]
        [TestCase("https://google.com/")]
        [TestCase("https://microsoft.com/")]
        public async Task Url_Should_Be_Created(string url) 
        {
            // Arrange
            var createRequest = new CreateUrlRequest { Url = url };

            // Act
            var createResponse = await _client.PostAsJsonAsync("Url", createRequest);

            // Assert
            Assert.IsNotNull(createResponse);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var created = await createResponse.Content.ReadFromJsonAsync<UrlResponse>();
            Assert.That(created?.Original, Is.EqualTo(url));

            // Act
            var getResponse = await _client.GetAsync($"Url/{created.Shortened}");

            // Assert
            Assert.IsNotNull(getResponse);
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.MovedPermanently));

            getResponse.Headers.TryGetValues("Location", out var locationHeaders);
            Assert.That(locationHeaders?.FirstOrDefault(), Is.EqualTo(url));
        }

        [Test]
        public async Task Get_Should_Return_NotFound_If_No_Url() 
        {
            // Arrange

            // Act
            var getResponse = await _client.GetAsync("Url/123456");

            // Assert
            Assert.IsNotNull(getResponse);
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TestCase("1")]
        [TestCase("12")]
        [TestCase("123")]
        [TestCase("1234")]
        [TestCase("12345")]
        [TestCase("1234567")]
        [TestCase("12345678")]
        public async Task Get_Should_Return_BadRequest_If_Wrong_Length(string url)
        {
            // Arrange

            // Act
            var getResponse = await _client.GetAsync($"Url/{url}");

            // Assert
            Assert.IsNotNull(getResponse);
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Shortened_Urls_Should_Be_Different_For_Same_Url()
        {
            // Arrange
            var createRequest = new CreateUrlRequest { Url = "https://nice.com/" };
            var iterationsCount = 3;
            var shorteneds = new List<string>(iterationsCount);

            // Act
            for (var i = 0; i < iterationsCount; i++)
            {
                var createResponse = await _client.PostAsJsonAsync("Url", createRequest);
                var created = await createResponse.Content.ReadFromJsonAsync<UrlResponse>();
                shorteneds.Add(created.Shortened);
            }

            // Assert
            Assert.That(shorteneds.Distinct().Count(), Is.EqualTo(shorteneds.Count));

            foreach (var shortened in shorteneds) 
            {
                var getResponse = await _client.GetAsync($"Url/{shortened}");

                Assert.IsNotNull(getResponse);

                getResponse.Headers.TryGetValues("Location", out var locationHeaders);
                Assert.That(locationHeaders?.FirstOrDefault(), Is.EqualTo(createRequest.Url));
            }
        }
    }
}
