using GreenDonut;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Stock.Market.Data;
using Stock.Market.WebApi.GraphQL.Models;
using Stock.Market.WebApi.GraphQL.Schema;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Market.WebApi.GraphQL.Tests.Schema
{
    public class MutationTests
    {
        private readonly Mock<INasdaqService> _nasdaqServiceMock;
        private readonly Mock<IMessagingService> _messagingServiceMock;
        private readonly Mutation _mutation;
        private readonly ApplicationDBContext _context;

        private readonly Fixture _fixture;

        public MutationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDB")
                .Options;

            _nasdaqServiceMock = new Mock<INasdaqService>();
            _messagingServiceMock = new Mock<IMessagingService>();
            _context = new ApplicationDBContext(options);
            _mutation = new Mutation(_nasdaqServiceMock.Object, _messagingServiceMock.Object, _context);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task MutationTests_BuyStockShares_WithExistentSymbol_ShouldReturnTrue()
        {
            //Arrange
            var validSymbol = "AAPL";
            var quantity = 4;
            _nasdaqServiceMock.Setup(x => x.FetchNasdaqData(validSymbol))
                .ReturnsAsync(_fixture.Create<NasdaqData>());

            //Act
            var result = await _mutation.BuyStockShares(validSymbol, quantity);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task MutationTests_BuyStockShares_WithNegativeAmount_ShouldThrowGraphQLException()
        {
            //Arrange
            var validSymbol = "AAPL";
            var quantity = -7;

            //Act
            var exception = await Record.ExceptionAsync(async () => await _mutation.BuyStockShares(validSymbol, quantity));

            //Assert
            Assert.IsAssignableFrom<GraphQLException>(exception);
        }


        [Fact]
        public async Task MutationTests_BuyStockShares_WithInexistentSymbol_ShouldThrowGraphQLException()
        {
            //Arrange
            var invalidSymbol = "xSYMBOLx";
            var quantity = 1;
            _nasdaqServiceMock.Setup(x => x.FetchNasdaqData(invalidSymbol));

            //Act
            var exception = await Record.ExceptionAsync(async () => await _mutation.BuyStockShares(invalidSymbol, quantity));

            //Assert
            Assert.IsAssignableFrom<GraphQLException>(exception);
        }
    }
}
