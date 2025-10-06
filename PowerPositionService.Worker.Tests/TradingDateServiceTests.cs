using System.Globalization;

namespace PowerPositionService.Worker.Tests
{
    public class TradingDateServiceTests
    {
        private readonly TradingDateService _service;

        public TradingDateServiceTests()
        {
            _service = new TradingDateService();
        }

        [Theory]
        [InlineData("2025-10-01T09:00:00Z", "2025-10-02")] // London 10:00 BST
        [InlineData("2025-10-01T21:59:00Z", "2025-10-02")] // London 22:59 BST
        [InlineData("2025-12-01T22:30:00Z", "2025-12-02")] // London 22:30 GMT
        public void GetDayAheadDate_Before23London_ReturnsTomorrow(string utcNowString, string expectedDateString)
        {
            DateTime utcNow = DateTime.Parse(utcNowString, null, DateTimeStyles.AdjustToUniversal);
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            DateTime result = _service.GetDayAheadDate(utcNow);

            Assert.Equal(expectedDate, result);
        }

        [Theory]
        [InlineData("2025-10-01T22:15:00Z", "2025-10-03")] // London 23:15 BST
        [InlineData("2025-10-01T23:00:00Z", "2025-10-03")] // London 00:00 BST
        [InlineData("2025-12-01T23:30:00Z", "2025-12-03")] // London 23:30 GMT
        public void GetDayAheadDate_AtOrAfter23London_ReturnsDayAfterTomorrow(string utcNowString, string expectedDateString)
        {
            DateTime utcNow = DateTime.Parse(utcNowString, null, DateTimeStyles.AdjustToUniversal);
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            DateTime result = _service.GetDayAheadDate(utcNow);

            Assert.Equal(expectedDate, result);
        }
    }
}
