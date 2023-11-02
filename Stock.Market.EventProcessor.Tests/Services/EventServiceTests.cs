using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
using Stock.Market.EventProcessor.Handlers;

namespace Stock.Market.EventProcessor.Tests.Services
{
    public class EventServiceTests
    {
        private readonly EventService _eventService;
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Fixture _fixture;
        private readonly ApplicationDBContext _context;

        public EventServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDB")
                .Options;

            _context = new ApplicationDBContext(options);
            _loggerMock = new Mock<ILogger<Worker>>();
            _eventService = new EventService(_context);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task EventService_Handle_WithSellSharesEvent_AndAvailableSharesToBeSold_ShouldSucceed()
        {
            // Arrange
            var symbol = "AAPL";
            var firstSharesQuantity = 5;
            var secondSharesQuantity = 8;

            var firstShares = _fixture.Build<Shares>().With(s => s.Symbol, symbol).With(s => s.Quantity, firstSharesQuantity).Create();
            var secondShares = _fixture.Build<Shares>().With(s => s.Symbol, symbol).With(s => s.Quantity, secondSharesQuantity).Create();
            
            _context.Shares.AddRange(firstShares,secondShares);
            _context.SaveChanges();

            var eventMessage = _fixture.Build<Event>()
                .With(e => e.Symbol, symbol)
                .With(e => e.Quantity, 10)
                .With(e => e.EventType, EventType.Sell)
                .Create();

            //Act
            await _eventService.Handle(eventMessage);

            //Assert 
            var shares = _context.Shares;
            Assert.True(shares.Count() == 1);
            Assert.True(shares.Sum(s => s.Quantity) == firstSharesQuantity + secondSharesQuantity - eventMessage.Quantity);
        }
    }
}