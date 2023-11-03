using Stock.Market.Common.Models;

namespace Stock.Market.EventProcessor.Services.Interfaces
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
