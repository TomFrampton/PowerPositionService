using Microsoft.Extensions.Options;
using PowerPositionService.Worker.Interfaces;
using PowerPositionService.Worker.Models;
using Services;

namespace PowerPositionService.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddWindowsService();

            builder.Services.AddOptions<SchedulerOptions>()
                .Bind(builder.Configuration.GetSection(SchedulerOptions.Key))
                .Validate(options => options.IntervalMinutes > 0, "IntervalMinutes must be greater than 0")
                .Validate(options => !string.IsNullOrWhiteSpace(options.OutputDirectory), "OutputDirectory is required")
                .ValidateOnStart();

            builder.Services.AddTransient<IPowerService, PowerService>();
            builder.Services.AddTransient<IExportService, CsvExportService>();
            builder.Services.AddTransient<IPositionAggregator, PositionAggregator>();

            var host = builder.Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                await host.StartAsync();
                await host.WaitForShutdownAsync();
            }
            catch (OptionsValidationException ex)
            {
                logger.LogCritical("Configuration validation failed: {Failures}", ex.Failures);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during execution");
            }
            finally
            {
                await host.StopAsync();
                host.Dispose();
            }
        }
    }
}