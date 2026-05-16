namespace Test_DateTimeExtensions
{
    public class DateTimeExtensionsTests
    {
        //(2026, 5, 13, 14, 30, 0);
        [Theory]
        // 1. Kịch bản tính Giây
        [InlineData("2026-05-13 14:29:45", "15 giây trước")]
        [InlineData("2026-05-13 14:29:01", "59 giây trước")]

        // 2. Kịch bản tính Phút
        [InlineData("2026-05-13 14:29:00", "1 phút trước")]
        [InlineData("2026-05-13 13:45:00", "45 phút trước")]

        // 3. Kịch bản tính Giờ
        [InlineData("2026-05-13 13:30:00", "1 giờ trước")]
        [InlineData("2026-05-13 09:30:00", "5 giờ trước")]

        // 4. Kịch bản quá 24h (Hiện Ngày/Tháng/Năm)
        [InlineData("2026-05-12 10:15:00", "12/05/2026 10:15")] // Qua 1 ngày
        [InlineData("2025-01-01 08:00:00", "01/01/2025 08:00")] // Qua 1 năm

        public void TestTimeAgo_ShouldReturnCorrectFormat(string targetDateStr, string expectedResult)
        {
            DateTime now = new DateTime(2026, 5, 13, 14, 30, 0);
            DateTime targetDate = DateTime.Parse(targetDateStr);

            string result = targetDate.ToTimeAgoFormat(now);

            Assert.Equal(expectedResult, result);
        }
        [Fact] 
        public void TestTimeAgo_TargetInFuture_ShouldThrowException()
        {
            DateTime fixedNow = new DateTime(2026, 5, 13, 14, 30, 0);
            DateTime targetDateInFuture = new DateTime(2026, 5, 13, 14, 39, 0);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                targetDateInFuture.ToTimeAgoFormat(fixedNow);
            });

            //Assert.Contains("không được nằm ở tương lai", exception.Message);
        }
    }
}