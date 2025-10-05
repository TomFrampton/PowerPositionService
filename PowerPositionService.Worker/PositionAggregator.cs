using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using Services;

namespace PowerPositionService.Worker
{
    public class PositionAggregator : IPositionAggregator
    {
        public const int Periods = 24;
        public const int StartingHour = 23;

        public IEnumerable<TradePosition> AggregatePositions(IEnumerable<PowerTrade> trades)
        {
            if (trades == null || !trades.Any())
            {
                throw new ArgumentException("Trades collection cannot be null or empty.");
            }

            var positions = new List<TradePosition>();

            for (int i = 0; i < Periods; i++)
            {
                var volume = trades.Sum(trade =>
                {
                    var period = trade.Periods.FirstOrDefault(p => p.Period == i + 1);
                    return period != null ? period.Volume : 0;
                });

                positions.Add(new TradePosition
                {
                    LocalHour = (i + StartingHour) % Periods,
                    Volume = volume
                });
            }

            return positions;
        }
    }
}
