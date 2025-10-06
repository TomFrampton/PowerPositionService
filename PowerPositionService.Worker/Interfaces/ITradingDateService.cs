namespace PowerPositionService.Worker.Interfaces
{
    public interface ITradingDateService
    {
        public DateTime GetDayAheadDate(DateTime utcNow);
    }
}
