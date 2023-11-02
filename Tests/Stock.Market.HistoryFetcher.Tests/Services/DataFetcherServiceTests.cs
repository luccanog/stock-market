using Microsoft.EntityFrameworkCore;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.HistoryFetcher.Services;

namespace Stock.Market.HistoryFetcher.Tests.Services
{
    public class DataFetcherServiceTests : IDisposable
    {
        private readonly Mock<INasdaqService> _nasdaqService;
        private readonly ApplicationDBContext _context;
        private readonly DataFetcherService _dataFetcherService;
        private readonly Fixture _fixture;

        public DataFetcherServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: nameof(DataFetcherServiceTests))
                .Options;
            _context = new ApplicationDBContext(options);
            _nasdaqService = new Mock<INasdaqService>();
            _dataFetcherService = new DataFetcherService(_nasdaqService.Object, _context);
            _fixture = new Fixture();
        }


        [Fact]
        public async Task DataFetcherService_UpdateStocksHistory_WithoutData_ShouldNotUpdateHistory()
        {
            //Act
            await _dataFetcherService.UpdateStocksHistory();

            //Assert
            Assert.Empty(_context.StocksHistory.AsEnumerable());
        }


        [Fact]
        public async Task DataFetcherService_UpdateStocksHistory_WithManySharesFromOneStock_ShouldInsertOneNewItemOnDatabase()
        {
            //Arrange
            var symbol = "MSFT";
            var shares = _fixture.Build<Shares>().With(s => s.Symbol, symbol).CreateMany();
            _context.Shares.AddRange(shares);
            _context.SaveChanges();

            _nasdaqService.Setup(n => n.FetchNasdaqData(symbol)).ReturnsAsync(_fixture.Create<NasdaqData>());

            //Act
            await _dataFetcherService.UpdateStocksHistory();

            //Assert
            _nasdaqService.Verify(n => n.FetchNasdaqData(symbol), Times.Once);
            Assert.True(_context.StocksHistory.Count() == 1);
        }

        [Fact]
        public async Task DataFetcherService_UpdateStocksHistory_WithSharesFromManyStocks_ShouldInsertManyNewItemOnDatabase()
        {
            //Arrange
            var distinctStocksAmount = 5;
            var shares = _fixture.Build<Shares>().CreateMany(distinctStocksAmount);
            _context.Shares.AddRange(shares);
            _context.SaveChanges();

            _nasdaqService.Setup(n => n.FetchNasdaqData(It.IsAny<string>())).ReturnsAsync(_fixture.Create<NasdaqData>());

            //Act
            await _dataFetcherService.UpdateStocksHistory();

            //Assert
            _nasdaqService.Verify(n => n.FetchNasdaqData(It.IsAny<string>()), Times.Exactly(distinctStocksAmount));
            Assert.True(_context.StocksHistory.Count() == distinctStocksAmount);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}