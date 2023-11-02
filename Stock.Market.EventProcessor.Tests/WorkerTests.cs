using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
using Stock.Market.EventProcessor.Handlers;
using Stock.Market.EventProcessor.Service.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Stock.Market.EventProcessor.Tests
{
    public class WorkerTests
    {
        private readonly Worker _worker;
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly Fixture _fixture;
        private readonly Mock<IConsumer<Ignore, string>> _consumerMock;
        public WorkerTests()
        {
            var configDic = new Dictionary<string, string?> {
                {"Kafka:BootstrapServers", "http://www.dummykafkabootstrap.net"},
                {"Kafka:GroupId", "123"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDic)
                .Build();

            var serviceColletion = new ServiceCollection();
            serviceColletion.AddDbContext<ApplicationDBContext>(builder =>
            {
                builder.UseInMemoryDatabase(databaseName: "InMemoryDB");
            });
            serviceColletion.AddSingleton<IEventService, EventService>();
            _consumerMock = new Mock<IConsumer<Ignore, string>>();

            _serviceProvider = serviceColletion.BuildServiceProvider();
            _loggerMock = new Mock<ILogger<Worker>>();
            _worker = new Worker(_loggerMock.Object, _serviceProvider, _consumerMock.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Worker_ExecuteAsync_WithSellSharesEvent_AndAvailableSharesToBeSold_ShouldSucceed()
        {
            // Arrange
            var symbol = "AAPL";
            var firstShares = _fixture.Build<Shares>().With(s => s.Symbol, symbol).With(s => s.Quantity, 5).Create();
            var secondShares = _fixture.Build<Shares>().With(s => s.Symbol, symbol).With(s => s.Quantity, 5).Create();
            InsertIntoDatabase(firstShares, secondShares);

            var eventObj = _fixture.Build<Event>()
                .With(e => e.Symbol, symbol)
                .With(e => e.Quantity, 7)
                .With(e => e.EventType, EventType.Sell)
                .Create();
            SetupKafkaConsumedMessage(eventObj);

            //Act
            await Task.Run(() => _worker.StartAsync(new CancellationTokenSource().Token));
            await Task.Delay(100);
            await Task.Run(() => _worker.StopAsync(new CancellationTokenSource().Token));

            //Assert 
            var shares = GetFromDatabase();
            Assert.True(shares.Count() == 1);
            Assert.True(shares.Sum(s => s.Quantity) == firstShares.Quantity + secondShares.Quantity - eventObj.Quantity);


        }

        private void SetupKafkaConsumedMessage(Event eventObj)
        {
            var message = new Message<Ignore, string>()
            {
                Value = JsonSerializer.Serialize(eventObj)
            };

            var consumeResult = new ConsumeResult<Ignore, string>()
            {
                Message = message
            };

            _consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);
        }

        private void InsertIntoDatabase(params Shares[] shares)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                context.Shares.AddRange(shares);
                context.SaveChanges();
            }
        }

        private IEnumerable<Shares> GetFromDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                return context.Shares.AsEnumerable();
            }
        }
    }
}