using Stock.Market.Data.Models;

namespace Stock.Market.EventProcessor.Service.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Handle the Event message according to it's <see cref="EventType"/>
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns></returns>
        public Task Handle(Event eventMessage);
    }
}
