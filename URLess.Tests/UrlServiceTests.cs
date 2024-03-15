using Moq;
using NUnit.Framework.Internal;
using URLess.Core.Interfaces;
using URLessCore.Interfaces;
using URLessCore.Services;
using URLessDAL.Data.Gateways;
using URLessDAL.Entities;

namespace URLess.Tests
{
    [Parallelizable(ParallelScope.Fixtures)]
    [TestFixture]
    public class UrlServiceTests
    {
        private Mock<IUrlGateway> _urlGatewayMock;
        private Mock<IIdGenerator> _idGeneratorMock;
        private Mock<ICacheService> _cacheMock;

        private UrlService Service { get; set; }

        [SetUp]
        public void SetUp() 
        {
            _urlGatewayMock = new();
            _idGeneratorMock = new();
            _cacheMock = new();

            Service = new UrlService(_urlGatewayMock.Object, _idGeneratorMock.Object, _cacheMock.Object);
        }

        [TestCase("test1")]
        [TestCase("test2")]
        [TestCase("test3")]
        public async Task GetUrl_Should_Return_From_Cache(string id) 
        {
            // Arrange
            _cacheMock.Setup(x => x.Get(id)).Returns(new Url { Id = id, Original = $"{id}_Original" });

            // Act
            var result = await Service.GetUrl(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Shortened, Is.EqualTo(id));
            Assert.That(result.Original, Is.EqualTo($"{id}_Original"));

            _cacheMock.Verify(x => x.Get(id), Times.Once);
            _urlGatewayMock.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
        }

        [TestCase("test1")]
        [TestCase("test2")]
        [TestCase("test3")]
        public async Task GetUrl_Should_Return_From_Gateway(string id)
        {
            // Arrange
            _cacheMock.Setup(x => x.Get(id)).Returns<Url?>(null);
            _urlGatewayMock.Setup(x => x.Get(id)).ReturnsAsync(new Url { Id = id, Original = $"{id}_Original" });

            // Act
            var result = await Service.GetUrl(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Shortened, Is.EqualTo(id));
            Assert.That(result.Original, Is.EqualTo($"{id}_Original"));

            _cacheMock.Verify(x => x.Get(id), Times.Once);
            _urlGatewayMock.Verify(x => x.Get(id), Times.Once);
            _cacheMock.Verify(x => x.Set(It.Is<Url>(url => url.Id == id && url.Original == $"{id}_Original")));
        }

        [TestCase(arguments: null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateUrl_Should_Throw_Exception_If_Empty(string url) 
        {
            // Arrange

            // Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await Service.CreateUrl(url));
        }

        [TestCase("test1")]
        [TestCase("test2")]
        [TestCase("test3")]
        public async Task CreateUrl_Should_Be_Created(string url) 
        {
            // Arrange
            var shorted = url.Last().ToString();
            _idGeneratorMock.Setup(x => x.Generate(url)).Returns(shorted);
            _urlGatewayMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string id, string original) => new Url { Id = id, Original = original });

            // Act
            var result = await Service.CreateUrl(url);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Shortened, Is.EqualTo(shorted));
            Assert.That(result.Original, Is.EqualTo(url));

            _idGeneratorMock.Verify(x => x.Regenerate(), Times.Never);
            _cacheMock.Verify(x => x.Set(It.Is<Url>(added => added.Id == shorted && added.Original == url)));
        }

        [TestCase("test1")]
        [TestCase("test2")]
        [TestCase("test3")]
        public void CreateUrl_Id_Should_Be_Regenerated_Check_Cache_Only(string url)
        {
            // Arrange
            var shorted = url.Last().ToString();
            var regenerated = $"{shorted}_1";
            _cacheMock.Setup(x => x.Get(It.IsAny<string>())).Returns(new Url { Id = "", Original = "" });
            _idGeneratorMock.Setup(x => x.Generate(url)).Returns(shorted);
            _idGeneratorMock.Setup(x => x.Regenerate()).Returns(regenerated);
            _urlGatewayMock.Setup(x => x.IsPresent(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Service.CreateUrl(url));

            // Assert
            _idGeneratorMock.Verify(x => x.Generate(url), Times.Once);
            _idGeneratorMock.Verify(x => x.Regenerate(), Times.Exactly(10));
            _cacheMock.Verify(x => x.Get(shorted), Times.Once);
            _cacheMock.Verify(x => x.Get(regenerated), Times.Exactly(10));
            _urlGatewayMock.Verify(x => x.IsPresent(shorted), Times.Never);
            _urlGatewayMock.Verify(x => x.IsPresent(regenerated), Times.Never);
        }

        [TestCase("test1")]
        [TestCase("test2")]
        [TestCase("test3")]
        public void CreateUrl_Id_Should_Be_Regenerated_Check_Db_Also(string url)
        {
            // Arrange
            var shorted = url.Last().ToString();
            var regenerated = $"{shorted}_1";
            _cacheMock.Setup(x => x.Get(It.IsAny<string>())).Returns<Url?>(null);
            _idGeneratorMock.Setup(x => x.Generate(url)).Returns(shorted);
            _idGeneratorMock.Setup(x => x.Regenerate()).Returns(regenerated);
            _urlGatewayMock.Setup(x => x.IsPresent(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Service.CreateUrl(url));

            // Assert
            _idGeneratorMock.Verify(x => x.Generate(url), Times.Once);
            _idGeneratorMock.Verify(x => x.Regenerate(), Times.Exactly(10));
            _cacheMock.Verify(x => x.Get(shorted), Times.Once);
            _cacheMock.Verify(x => x.Get(regenerated), Times.Exactly(10));
            _urlGatewayMock.Verify(x => x.IsPresent(shorted), Times.Once);
            _urlGatewayMock.Verify(x => x.IsPresent(regenerated), Times.Exactly(10));
        }
    }
}
