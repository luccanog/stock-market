using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using System.Text.RegularExpressions;

namespace Stock.Market.EventProcessor.Tests
{
    public class WorkerTests
    {
        private readonly Worker _worker;
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly Fixture _fixture;

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

            _serviceProvider = serviceColletion.BuildServiceProvider();
            _loggerMock = new Mock<ILogger<Worker>>();
            _worker = new Worker(_loggerMock.Object, configuration, _serviceProvider);
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

            /** @todo: mock kafka methods **/

            //Act
            await _worker.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(2));
            await _worker.StopAsync(CancellationToken.None);

            //Assert 
            /** @todo: check database **/

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
    }
}