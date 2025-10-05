using PowerPositionService.Worker.Models;

namespace PowerPositionService.Worker.Interfaces
{
    public interface IExportService
    {
        Task ExportPositionsAsync(IEnumerable<TradePosition> positions, DateTime time);
    }
}
