using Microsoft.EntityFrameworkCore;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
using Stock.Market.EventProcessor.Services;

namespace Stock.Market.EventProcessor.Tests.Services
{
    public class EventServiceTests : IDisposable
    {
        private readonly EventService _eventService;
        private readonly Fixture _fixture;
        private readonly ApplicationDBContext _context;

        public EventServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: nameof(EventServiceTests))
                .Options;

            _context = new ApplicationDBContext(options);
            _eventService = new EventService(_context);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task EventService_Handle_SellEvent_WithQuantityBelowTotalHeldShares_ShouldSucceed()
        {
            // Arrange
            var symbol = "AAPL";
            var firstSharesQuantity = 5;
            var secondSharesQuantity = 8;
            var firstShares = CreateSharesObject(symbol, firstSharesQuantity);
            var secondShares = CreateSharesObject(symbol, secondSharesQuantity);

            _context.Shares.AddRange(firstShares, secondShares);
            _context.SaveChanges();

            var eventMessage = _fixture.Build<Event>()
                .With(e => e.Symbol, symbol)
                .With(e => e.Quantity, 10)
                .With(e => e.EventType, EventType.Sell)
                .Create();

            //Act
            await _eventService.Handle(eventMessage);

            //Assert 
            Assert.True(_context.Shares.Count() == 1);
            Assert.True(_context.Shares.Sum(s => s.Quantity) == firstSharesQuantity + secondSharesQuantity - eventMessage.Quantity);
        }

        [Fact]
        public async Task EventService_Handle_SellEvent_WithQuantityEqualTotalHeldShares_ShouldSucceed()
        {
            // Arrange
            var symbol = "AAPL";
            var firstSharesQuantity = 2;
            var secondSharesQuantity = 3;
            var firstShares = CreateSharesObject(symbol, firstSharesQuantity);
            var secondShares = CreateSharesObject(symbol, secondSharesQuantity);

            _context.Shares.AddRange(firstShares, secondShares);
            _context.SaveChanges();

            var eventMessage = _fixture.Build<Event>()
                .With(e => e.Symbol, symbol)
                .With(e => e.Quantity, 5)
                .With(e => e.EventType, EventType.Sell)
                .Create();

            //Act
            await _eventService.Handle(eventMessage);

            //Assert 
            Assert.False(_context.Shares.Any());
        }

        [Fact]
        public async Task EventService_Handle_BuyEvent_WithValidData_ShouldSucceed()
        {
            // Arrange
            var symbol = "AAPL";

            var eventMessage = _fixture.Build<Event>()
                .With(e => e.Symbol, symbol)
                .With(e => e.Quantity, 5)
                .With(e => e.EventType, EventType.Buy)
                .Create();

            //Act
            await _eventService.Handle(eventMessage);

            //Assert 
            var storedObject = _context.Shares.First();
            Assert.True(_context.Shares.Any());
            Assert.True(storedObject.Symbol.Equals(eventMessage.Symbol));
            Assert.True(storedObject.Quantity.Equals(eventMessage.Quantity));
            Assert.True(storedObject.CompanyName.Equals(eventMessage.CompanyName));
            Assert.True(storedObject.OriginalUnitCost.Equals(eventMessage.Value));
        }


        private Shares CreateSharesObject(string symbol, int firstSharesQuantity)
        {
            return _fixture.Build<Shares>()
                .With(s => s.Symbol, symbol)
                .With(s => s.Quantity, firstSharesQuantity)
                .Create();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

    }
}