﻿using Confluent.Kafka;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;
using System.Text.Json;

namespace Stock.Market.WebApi.GraphQL.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly IProducer<Null, string> _producer;

        public MessagingService(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:Broker"],
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = configuration["Kafka:SaslUsername"],
                SaslPassword = configuration["Kafka:SaslPassword"],
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();

        }

        public void Send<T>(string topic, T message)
        {
            _producer.Produce("stock-market", new Message<Null, string> { Value = JsonSerializer.Serialize(message) },
                (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced event to topic");
                    }
                });
        }
    }
}
