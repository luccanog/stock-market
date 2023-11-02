using Microsoft.EntityFrameworkCore;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.WebApi.GraphQL.Schema;
using System.Globalization;

namespace Stock.Market.WebApi.GraphQL.Tests.Schema
{
    public class QueryTests : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly Mock<INasdaqService> _nasdaqServiceMock;
        private readonly Query _query;
        private readonly Fixture _fixture;

        public QueryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: nameof(QueryTests))
                .Options;
            _context = new ApplicationDBContext(options);
            _nasdaqServiceMock = new Mock<INasdaqService>();
            _query = new Query(_context, _nasdaqServiceMock.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Query_GetStockData_WithPositiveVariation_ShouldSucceed()
        {
            //Arrange
            var originalUnitCost = 5.50m;
            var shares = _fixture.Build<Shares>()
                .With(a => a.Quantity, 2)
                .With(a => a.OriginalUnitCost, originalUnitCost)
                .Create();
            _context.Shares.Add(shares);
            _context.SaveChanges();

            var lastSalePrice = ParseDecimalCostToString(originalUnitCost * 2);
            NasdaqData nasdaqData = CreateNasdaqData(shares.Symbol, lastSalePrice);

            _nasdaqServiceMock.Setup(n => n.FetchNasdaqData(shares.Symbol)).ReturnsAsync(nasdaqData);

            //Act
            var result = await _query.GetStockData();

            //Assert
            Assert.Collection(result, r => Assert.Equal("100%", r.Variation));
            Assert.Collection(result, r => Assert.Equal(shares.OriginalUnitCost * shares.Quantity, r.TotalValue));
            _nasdaqServiceMock.Verify(n => n.FetchNasdaqData(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task Query_GetStockData_WithNegativeVariation_ShouldSucceed()
        {
            //Arrange
            var originalUnitCost = 10.00m;
            var shares = _fixture.Build<Shares>()
                .With(a => a.Quantity, 2)
                .With(a => a.OriginalUnitCost, originalUnitCost)
                .Create();
            _context.Shares.Add(shares);
            _context.SaveChanges();

            var lastSalePrice = ParseDecimalCostToString(originalUnitCost - 3);
            NasdaqData nasdaqData = CreateNasdaqData(shares.Symbol, lastSalePrice);

            _nasdaqServiceMock.Setup(n => n.FetchNasdaqData(shares.Symbol)).ReturnsAsync(nasdaqData);

            //Act
            var result = await _query.GetStockData();

            //Assert
            Assert.Collection(result, r => Assert.Equal("-30%", r.Variation));
            Assert.Collection(result, r => Assert.Equal(shares.OriginalUnitCost * shares.Quantity, r.TotalValue));
            _nasdaqServiceMock.Verify(n => n.FetchNasdaqData(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task Query_GetStockData_WithoutData_ShouldReturnEmptyList()
        {
            //Act
            var result = await _query.GetStockData();

            //Assert
            Assert.False(result.Any());
            Assert.NotNull(result);

            _nasdaqServiceMock.Verify(n => n.FetchNasdaqData(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task Query_GetStockData_WithAvailableStockHistory_ShouldSucceed()
        {
            //Arrange
            var symbol = "CFLT";
            var lowestPrice = 10m;
            var averagePrice = 15m;
            var highestPrice = 20m;

            var shares = _fixture.Build<Shares>().With(a => a.Symbol, symbol).Create();
            var stocksHistory = CreateStocksHistoryData(symbol, lowestPrice, averagePrice, highestPrice);

            _context.Shares.Add(shares);
            _context.StocksHistory.AddRange(stocksHistory);
            _context.SaveChanges();

            //Act
            var result = await _query.GetStockData();

            //Assert
            var currentDayReferencePrices = result.FirstOrDefault()!.CurrentDayReferencePrices;
            Assert.True(currentDayReferencePrices.LowestPrice == lowestPrice);
            Assert.True(currentDayReferencePrices.HighestPrice == highestPrice);
            Assert.True(currentDayReferencePrices.AveragePrice == (lowestPrice + averagePrice + highestPrice) / 3);
        }

        private IEnumerable<StockHistory> CreateStocksHistoryData(string symbol, params decimal[] prices)
        {
            var result = new List<StockHistory>();

            foreach (var price in prices)
            {
                var stockHistory = _fixture.Build<StockHistory>()
                    .With(s => s.Symbol, symbol)
                    .With(s => s.Price, 10m)
                    .With(s => s.InsertDate, DateTime.UtcNow)
                    .Create();

                result.Add(stockHistory);
            }
            return result;
        }

        private NasdaqData CreateNasdaqData(string symbol, string lastSalePrice)
        {
            var primaryData = _fixture.Build<PrimaryData>()
                .With(p => p.LastSalePrice, lastSalePrice)
                .Create();

            var nasdaqData = _fixture.Build<NasdaqData>()
                .With(n => n.Symbol, symbol)
                .With(n => n.PrimaryData, primaryData)
                .Create();

            return nasdaqData;
        }

        private string ParseDecimalCostToString(decimal cost)
        {
            var parsedValue = $"${cost.ToString(CultureInfo.InvariantCulture)}";
            return parsedValue;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
;