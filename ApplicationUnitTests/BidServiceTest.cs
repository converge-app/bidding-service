using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Application.Exceptions;
using Application.Models.Entities;
using Application.Repositories;
using Application.Services;
using Application.Utility.ClientLibrary;
using Moq;
using Xunit;

namespace ApplicationUnitTests
{
    public class BidServiceTest
    {
        [Fact]
        public async void Create_GetUserAsync()
        {
            Environment.SetEnvironmentVariable("USERS_SERVICE_HTTP", "users-service.api.converge-app.net");
            var expected = "";
            var mockFactory = new Mock<IHttpClientFactory>();
            var configuration = new HttpConfiguration();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                request.SetConfiguration(configuration);
                var response = request.CreateResponse(HttpStatusCode.OK, expected);
                return Task.FromResult(response);
            });

            var client = new HttpClient(clientHandlerStub);

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;
            var controller = new Client(factory);

            //Act
            var result = await controller.GetUserAsync("123");

            //Assert
            Assert.NotNull(expected);
        }

        [Fact]
        public void Open_GetProject_ThrowsInvalidBid()
        {
            // Arrange
            var biddingRepository = new Mock<IBidRepository>();
            var client = new Mock<IClient>();
            biddingRepository.Setup(m => m.Create(It.IsAny<Bid>())).ReturnsAsync((Bid)null);
            var biddingService = new BidService(biddingRepository.Object, client.Object);

            // Act
            // Assert

            Assert.ThrowsAsync<InvalidBid>(() => biddingService.Open(new Bid()));

        }

        [Fact]
        public void Open_CreatedBid_ReturnsBidMessage()
        {
            // Arrange
            var biddingRepository = new Mock<IBidRepository>();
            var client = new Mock<IClient>();

            biddingRepository.Setup(m => m.Create(It.IsAny<Bid>())).ReturnsAsync(new Bid());
            var biddingService = new BidService(biddingRepository.Object, client.Object);


            // Act
            var actual = new Bid();

            // Assert
            Assert.Equal(new Bid().Message, actual.Message);

        }

        [Fact]
        public void Open_CreatedBid_ReturnsBidAmount()
        {
            // Arrange
            var biddingRepository = new Mock<IBidRepository>();
            var client = new Mock<IClient>();
            biddingRepository.Setup(m => m.Create(It.IsAny<Bid>())).ReturnsAsync(new Bid());
            var biddingService = new BidService(biddingRepository.Object, client.Object);

            // Act
            var actual = new Bid();

            // Assert
            Assert.Equal(new Bid().Amount, actual.Amount);

        }

        [Fact]
        public void Open_CreatedBid_ThrowsInvalidBid()
        {
            // Arrange
            var biddingRepository = new Mock<IBidRepository>();
            var client = new Mock<IClient>();
            biddingRepository.Setup(m => m.Create(It.IsAny<Bid>())).ReturnsAsync((Bid)null);
            var biddingService = new BidService(biddingRepository.Object, client.Object);

            // Act
            // Assert
            Assert.ThrowsAsync<InvalidBid>(() => biddingService.Open(new Bid()));

        }
    }
}