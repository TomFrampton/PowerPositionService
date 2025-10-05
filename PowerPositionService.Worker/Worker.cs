using Microsoft.Extensions.Options;
using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using PowerPositionService.Worker.Utilities;
using Services;

namespace PowerPositionService.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IPowerService _powerService;
        private readonly IPositionAggregator _positionAggregator;
        private readonly IExportService _exportService;

        private readonly ILogger<Worker> _logger;
        private readonly SchedulerOptions _schedulerOptions;

        public Worker(
            IPowerService powerService,
            IPositionAggregator positionAggregator,
            IExportService exportService,
            ILogger<Worker> logger,
            IOptions<SchedulerOptions> schedulerOptions)
        {
            _powerService = powerService;
            _positionAggregator = positionAggregator;
            _exportService = exportService;

            _logger = logger;
            _schedulerOptions = schedulerOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var interval = TimeSpan.FromMinutes(_schedulerOptions.IntervalMinutes);
            using var timer = new PeriodicTimer(interval);

            do
            {
                try
                {
                    _ = StartProcess(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during execution");
                }
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }

        private Task StartProcess(CancellationToken cancellationToken)
        {

            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Extract started at {time}", DateTime.UtcNow);

                    var trades = await RetryHelper.WithAttemptsAsync(10, async () =>
                    {
                        try
                        {
                            return await _powerService.GetTradesAsync(DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get trades. Retrying.");
                            throw;
                        }

                    }, cancellationToken);

                    var positions = _positionAggregator.AggregatePositions(trades);

                    await _exportService.ExportPositionsAsync(positions);

                    _logger.LogInformation("Extract finished at {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during execution");
                }

            }, cancellationToken);
        }
    }
}
