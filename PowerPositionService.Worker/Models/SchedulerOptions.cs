namespace PowerPositionService.Worker.Models
{
    public class SchedulerOptions
    {
        public const string Key = "Scheduler";

        public double IntervalMinutes { get; set; }
        public string? OutputDirectory { get; set; }
    }
}
