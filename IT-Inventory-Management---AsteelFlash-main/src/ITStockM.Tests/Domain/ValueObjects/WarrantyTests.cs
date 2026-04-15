using Xunit;
using ITStockM.Domain.ValueObjects;

namespace ITStockM.Tests.Domain.ValueObjects
{
    public class WarrantyTests
    {
        [Fact]
        public void Constructor_WithValidDates_CreatesInstance()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddYears(1);

            // Act
            var warranty = new Warranty(startDate, endDate);

            // Assert
            Assert.Equal(startDate, warranty.StartDate);
            Assert.Equal(endDate, warranty.EndDate);
        }

        [Fact]
        public void Constructor_WithEndDateBeforeStartDate_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Warranty(startDate, endDate));
        }

        [Fact]
        public void IsActive_WithFutureEndDate_ReturnsTrue()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddYears(1);
            var warranty = new Warranty(startDate, endDate);

            // Act & Assert
            Assert.True(warranty.IsActive);
        }

        [Fact]
        public void IsActive_WithPastEndDate_ReturnsFalse()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddYears(-2);
            var endDate = DateTime.UtcNow.AddDays(-1);
            var warranty = new Warranty(startDate, endDate);

            // Act & Assert
            Assert.False(warranty.IsActive);
        }

        [Fact]
        public void MonthsRemaining_WithActiveWarranty_ReturnsCorrectMonths()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddMonths(6);
            var warranty = new Warranty(startDate, endDate);

            // Act
            var monthsRemaining = warranty.MonthsRemaining;

            // Assert
            Assert.InRange(monthsRemaining, 5, 7); // Allow 1 month variance due to timing
        }

        [Fact]
        public void MonthsRemaining_WithExpiredWarranty_ReturnsZero()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddYears(-2);
            var endDate = DateTime.UtcNow.AddDays(-1);
            var warranty = new Warranty(startDate, endDate);

            // Act & Assert
            Assert.Equal(0, warranty.MonthsRemaining);
        }

        [Fact]
        public void Equals_SameWarranties_AreEqual()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddYears(1);
            var warranty1 = new Warranty(startDate, endDate);
            var warranty2 = new Warranty(startDate, endDate);

            // Act & Assert
            Assert.Equal(warranty1, warranty2);
        }

        [Fact]
        public void Equals_DifferentEndDates_AreNotEqual()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var warranty1 = new Warranty(startDate, DateTime.UtcNow.AddYears(1));
            var warranty2 = new Warranty(startDate, DateTime.UtcNow.AddYears(2));

            // Act & Assert
            Assert.NotEqual(warranty1, warranty2);
        }

        [Fact]
        public void ToString_ReturnsFriendlyFormat()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2025, 1, 1);
            var warranty = new Warranty(startDate, endDate);

            // Act
            var str = warranty.ToString();

            // Assert
            Assert.Contains("2024-01-01", str);
            Assert.Contains("2025-01-01", str);
            Assert.Contains("months remaining", str);
        }
    }
}
