using Microsoft.Extensions.Options;
using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using System.Globalization;
using System.Text;

namespace PowerPositionService.Worker
{
    public class CsvExportService : IExportService
    {
        private readonly SchedulerOptions _schedulerOptions;

        public CsvExportService(IOptions<SchedulerOptions> schedulerOptions)
        {
            _schedulerOptions = schedulerOptions.Value;
        }

        public async Task ExportPositionsAsync(IEnumerable<TradePosition> positions, DateTime time)
        {
            var outputDirectory = _schedulerOptions.OutputDirectory!;

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var timestamp = time.ToString("yyyyMMdd_HHmm");
            var fileName = $"PowerPosition_{timestamp}.csv";

            string filePath = Path.Combine(outputDirectory, fileName);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Local Time,Volume");

            foreach (var position in positions)
            {
                string timeFormatted = position.LocalHour.ToString("D2") + ":00";
                var line = string.Format(CultureInfo.InvariantCulture, "{0},{1}", timeFormatted, position.Volume);

                stringBuilder.AppendLine(line);
            }

            await File.WriteAllTextAsync(filePath, stringBuilder.ToString(), Encoding.UTF8);
        }
    }
}
