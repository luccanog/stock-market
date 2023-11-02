using Microsoft.EntityFrameworkCore;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.WebApi.GraphQL.Schema;

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
            await _context.SaveChangesAsync();

            var lastSalePrice = $"${originalUnitCost * 2}";
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
            await _context.SaveChangesAsync();

            var lastSalePrice = $"${originalUnitCost - 3}";
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

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
;