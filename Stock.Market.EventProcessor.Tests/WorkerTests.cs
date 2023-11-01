using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stock.Market.Data;
using System.Text.RegularExpressions;

namespace Stock.Market.EventProcessor.Tests
{
    public class WorkerTests
    {
        private readonly Worker _worker;
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly ServiceProvider _serviceProvider;

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
        }

        [Fact]
        public void Worker_ExecuteAsync_WithSellSharesEvent_AndAvailableSharesToBeSold_ShouldSucceed()
        {
            // Arrange
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                context.Shares.Add(new Data.Entities.Shares());
                context.SaveChanges();
            }
        }
    }
}