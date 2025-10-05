
using Services;

namespace PowerPositionService.Worker.Tests
{
    public class PositionAggregatorTests
    {
        private readonly PositionAggregator _aggregator = new();

        [Fact]
        public void AggregatePositions_Returns24PositionsWithCorrectHoursAndVolumes()
        {
            var trades = new List<PowerTrade> { CreateMockTrade(1.0) };

            var result = _aggregator.AggregatePositions(trades).ToList();

            Assert.Equal(PositionAggregator.Periods, result.Count);

            for (int i = 0; i < PositionAggregator.Periods; i++)
            {
                int expectedHour = (i + PositionAggregator.StartingHour) % PositionAggregator.Periods;
                Assert.Equal(expectedHour, result[i].LocalHour);
                Assert.Equal(1.0, result[i].Volume);
            }
        }

        [Fact]
        public void AggregatePositions_SumsVolumesAcrossMultipleTrades()
        {
            var trades = new List<PowerTrade>
            {
                CreateMockTrade(1.0),
                CreateMockTrade(2.0)
            };

            var result = _aggregator.AggregatePositions(trades).ToList();

            Assert.All(result, p => Assert.Equal(3.0, p.Volume));
        }

        [Fact]
        public void AggregatePositions_ThrowsIfTradesNull()
        {
            var ex = Assert.Throws<ArgumentException>(() => _aggregator.AggregatePositions(null));
            Assert.Contains("Trades collection cannot be null", ex.Message);
        }

        [Fact]
        public void AggregatePositions_ThrowsIfTradesEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() => _aggregator.AggregatePositions(new List<PowerTrade>()));
            Assert.Contains("Trades collection cannot be null", ex.Message);
        }

        private PowerTrade CreateMockTrade(double volumePerPeriod)
        {
            return CreateMockTradeWithCustomPeriods(PositionAggregator.Periods, volumePerPeriod);
        }

        private PowerTrade CreateMockTradeWithCustomPeriods(int periodCount, double volumePerPeriod)
        {
            var trade = PowerTrade.Create(DateTime.Now, periodCount);

            foreach (var period in trade.Periods)
            {
                period.Volume = volumePerPeriod;
            }

            return trade;
        }
    }
}
