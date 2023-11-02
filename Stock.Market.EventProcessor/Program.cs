using Confluent.Kafka;
using Stock.Market.Data;
using static Confluent.Kafka.ConfigPropertyNames;
using System.Text;
using Stock.Market.EventProcessor.Service.Interfaces;
using Stock.Market.EventProcessor.Handlers;

namespace Stock.Market.EventProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddConsumers();
                    services.AddHostedService<Worker>();
                    services.AddDbContext<ApplicationDBContext>(ServiceLifetime.Singleton);
                    services.AddSingleton<IEventService,EventService>();
                })
                .Build();

            host.Run();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsumers(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
            services.AddSingleton(c => new ConsumerBuilder<Ignore, string>(config).Build());

            return services;
        }
    }
}