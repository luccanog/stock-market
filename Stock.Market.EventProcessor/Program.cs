using Confluent.Kafka;
using Stock.Market.Data;
using Stock.Market.EventProcessor.Services.Interfaces;
using Stock.Market.EventProcessor.Services;

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
                BootstrapServers = configuration["Kafka:Broker"],
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = configuration["Kafka:SaslUsername"],
                SaslPassword = configuration["Kafka:SaslPassword"],
                GroupId = "$Default"
            };

            services.AddSingleton(c => new ConsumerBuilder<Ignore, string>(config).Build());

            return services;
        }
    }
}