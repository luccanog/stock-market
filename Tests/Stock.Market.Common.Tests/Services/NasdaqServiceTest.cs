﻿using Flurl.Http.Testing;
using Microsoft.Extensions.Configuration;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services;
using Stock.Market.Common.Services.Interfaces;

namespace Stock.Market.WebApi.GraphQL.Tests.Services
{
    public class NasdaqServiceTests : IDisposable
    {
        private readonly INasdaqService _nasdaqService;
        private readonly HttpTest _httpTest;
        private readonly Fixture _fixture;

        public NasdaqServiceTests()
        {
            _httpTest = new HttpTest();

            var configDic = new Dictionary<string, string?> {
                {"NasdaqApiUrl", "http://www.dummyuri.net"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDic)
                .Build();

            _nasdaqService = new NasdaqService(configuration);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task NasdaqService_FetchNasdaqData_WithValidData_ShouldSucceed()
        {
            //Arrange
            var symbol = "AAPL";
            _httpTest.RespondWithJson(_fixture.Create<NasdaqResponse>(), 200);

            //Act
            var result = await _nasdaqService.FetchNasdaqData(symbol);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task NasdaqService_FetchNasdaqData_WithInvalidData_ShouldReturnNull()
        {
            //Arrange
            var symbol = "invalid-symbol";
            _httpTest.RespondWith("{\"errorMessage\": \"Symbol not exists.\"}", 400);

            //Act
            var result = await _nasdaqService.FetchNasdaqData(symbol);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task NasdaqService_FetchNasdaqData_WithOfflineAPI_ShouldTimeoutAndReturnNull()
        {
            //Arrange
            var symbol = "AAPL";
            _httpTest.SimulateTimeout();

            //Act
            var result = await _nasdaqService.FetchNasdaqData(symbol);

            //Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }
    }
}
