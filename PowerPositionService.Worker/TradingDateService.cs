using PowerPositionService.Worker.Interfaces;

namespace PowerPositionService.Worker
{
    public class TradingDateService : ITradingDateService
    {
        public DateTime GetDayAheadDate(DateTime utcNow)
        {
            var londonTimezone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime londonTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, londonTimezone);

            // If current time is 23:00 or later in trading terms its already the next day, so return the day after tomorrow
            return londonTime.Date.AddDays(londonTime.Hour >= 23 ? 2 : 1);
        }
    }
}
