namespace Stock.Market.WebApi.GraphQL.Services.Interfaces
{
    public interface IMessagingService
    {
        void Send<T>(string topic, T message);
    }
}
