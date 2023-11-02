using Stock.Market.Data.Entities;
using Stock.Market.Data;
using Stock.Market.Data.Models;
using Stock.Market.EventProcessor.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Stock.Market.EventProcessor.Handlers
{
    public class EventService : IEventService
    {
        private readonly ApplicationDBContext _context;

        public EventService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task Handle(Event eventMessage)
        {
            if (eventMessage.EventType is EventType.Buy)
            {
                HandleBuyShareEvent(eventMessage);
            }
            else
            {
                HandleSellShareEvent(eventMessage);
            }

            await _context.SaveChangesAsync();
        }

        private void HandleSellShareEvent(Event eventMessage)
        {
            var totalSharesToSold = eventMessage.Quantity;

            var shares = _context.Shares.Where(s => s.Symbol == eventMessage.Symbol);

            if (totalSharesToSold == shares.Sum(s => s.Quantity))
            {
                _context.Shares.Where(s => s.Symbol == eventMessage.Symbol).ExecuteDelete();
                return;
            }

            var currentSoldShares = 0;

            foreach (var share in shares)
            {
                if (share.Quantity + currentSoldShares <= totalSharesToSold)
                {
                    _context.Shares.Remove(share);
                    currentSoldShares += share.Quantity;
                }
                else
                {
                    var remainingSharesToBeSold = totalSharesToSold - currentSoldShares;
                    share.Quantity -= remainingSharesToBeSold;
                    currentSoldShares += remainingSharesToBeSold;
                }

                if (currentSoldShares == totalSharesToSold)
                {
                    break;
                }
            }

        }

        private void HandleBuyShareEvent(Event eventMessage)
        {
            var shares = new Shares(eventMessage.CompanyName, eventMessage.Symbol, eventMessage.Value, eventMessage.Quantity);
            _context.Shares.Add(shares);
        }
    }
}