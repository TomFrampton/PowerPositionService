using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using Services;

namespace PowerPositionService.Worker
{
    public class PositionAggregator : IPositionAggregator
    {
        public const int Periods = 24;
        public const int StartingHour = 23;

        /// <summary>
        /// Aggregates the trade volumes across all periods for the specified collection of power trades,
        /// returning a list of positions with local hour mappings and total volumes per period.
        /// </summary>
        /// <param name="trades">
        /// The collection of <see cref="PowerTrade"/> instances to aggregate.
        /// Each trade contains period-based volume data.
        /// </param>
        /// <returns>
        /// A collection of <see cref="TradePosition"/> objects, one for each period,
        /// where the volume represents the sum of all trade volumes for that period and
        /// the <c>LocalHour</c> corresponds to the mapped hour of the day.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="trades"/> is <c>null</c> or empty.
        /// </exception>
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
