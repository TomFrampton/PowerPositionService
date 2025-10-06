using Microsoft.Extensions.Options;
using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using PowerPositionService.Worker.Utilities;
using Services;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PowerPositionService.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IPowerService _powerService;
        private readonly IPositionAggregator _positionAggregator;
        private readonly IExportService _exportService;

        private readonly ILogger<Worker> _logger;
        private readonly SchedulerOptions _schedulerOptions;

        private readonly ConcurrentDictionary<Guid, Task> _runningTasks = new();

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
                var runId = Guid.NewGuid();

                // Start the process without awaiting it to ensure a run is not missed if the previous run takes longer than the interval
                var task = RunProcessAsync(runId, cancellationToken);

                if (_runningTasks.Count > 0)
                {
                    _logger.LogWarning("Starting a new run ({RunId}) while {Count} run(s) are still in progress", runId, _runningTasks.Count);
                }

                // Track running tasks to allow graceful shutdown
                _runningTasks.TryAdd(runId, task);

                _ = task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        _logger.LogError(t.Exception, "An error occurred during execution");
                    }

                    _runningTasks.TryRemove(runId, out _);

                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping service, waiting for {Count} running task(s) to complete...", _runningTasks.Count);

            try
            {
                await Task.WhenAll(_runningTasks.Values);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "One or more background tasks failed during shutdown");
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task RunProcessAsync(Guid runId, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var dayAheadDate = GetDayAheadTradingDate();
                var localTime = DateTime.Now;

                _logger.LogInformation("Run {RunId} started at {time:dd/MM/yyyy}", runId, localTime);

                // Retry up to 10 times to get trades to handle transient errors
                var trades = await RetryHelper.WithAttemptsAsync(10, async () =>
                {
                    try
                    {
                        return await _powerService.GetTradesAsync(dayAheadDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get trades. Retrying.");
                        throw;
                    }

                }, cancellationToken);

                var positions = _positionAggregator.AggregatePositions(trades);

                // Retry up to 10 times incase of transient file system errors
                await RetryHelper.WithAttemptsAsync(10, async () =>
                {
                    try
                    {
                        await _exportService.ExportPositionsAsync(positions, localTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to export positions. Retrying.");
                        throw;
                    }

                }, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Run {RunId} completed in {ElapsedMilliseconds} ms", runId, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                _logger.LogInformation("Run {RunId} cancelled after {ElapsedMilliseconds} ms", runId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Run {RunId} failed after {ElapsedMilliseconds} ms", runId, stopwatch.ElapsedMilliseconds);
            }
        }

        private DateTime GetDayAheadTradingDate()
        {
            var londonTimezone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime currentLondonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimezone);

            return currentLondonTime.Date.AddDays(1);
        }
    }
}
