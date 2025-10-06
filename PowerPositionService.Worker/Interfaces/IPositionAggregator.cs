using PowerPositionService.Worker.Models;
using Services;

namespace PowerPositionService.Worker.Interfaces
{
    public interface IPositionAggregator
    {
        IEnumerable<TradePosition> AggregatePositions(IEnumerable<PowerTrade>? trades);
    }
}
