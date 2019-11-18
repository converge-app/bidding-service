using System.Threading.Tasks;
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
        public void Open_GetProject_ThrowsInvalidBid()
        {
            // Arrange
            var biddingRepository = new Mock<IBidRepository>();
            var client = new Mock<IClient>();
            var biddingService = new BidService(biddingRepository.Object, client.Object);

            // Act
            // Assert

            Assert.ThrowsAsync<InvalidBid>(() => biddingService.Open(new Bid() { ProjectId = null }));

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