using Xunit;
using ITStockM.Domain.ValueObjects;

namespace ITStockM.Tests.Domain.ValueObjects
{
    public class SerialNumberTests
    {
        [Fact]
        public void Constructor_WithValidSerialNumber_CreatesInstance()
        {
            // Arrange & Act
            var serialNumber = new SerialNumber("ABC123DEF456");

            // Assert
            Assert.Equal("ABC123DEF456", serialNumber.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Constructor_WithEmptyOrWhitespace_ThrowsArgumentException(string value)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new SerialNumber(value));
        }

        [Fact]
        public void Constructor_TrimsWhitespace()
        {
            // Arrange & Act
            var serialNumber = new SerialNumber("  ABC123  ");

            // Assert
            Assert.Equal("ABC123", serialNumber.Value);
        }

        [Fact]
        public void Equals_SameSerialsIgnoringCase_AreEqual()
        {
            // Arrange
            var serial1 = new SerialNumber("ABC123");
            var serial2 = new SerialNumber("abc123");

            // Act & Assert
            Assert.Equal(serial1, serial2);
        }

        [Fact]
        public void Equals_DifferentSerials_AreNotEqual()
        {
            // Arrange
            var serial1 = new SerialNumber("ABC123");
            var serial2 = new SerialNumber("XYZ789");

            // Act & Assert
            Assert.NotEqual(serial1, serial2);
        }

        [Fact]
        public void GetHashCode_SameSerialsIgnoringCase_SameHashCode()
        {
            // Arrange
            var serial1 = new SerialNumber("ABC123");
            var serial2 = new SerialNumber("abc123");

            // Act & Assert
            Assert.Equal(serial1.GetHashCode(), serial2.GetHashCode());
        }

        [Fact]
        public void ToString_ReturnsValue()
        {
            // Arrange
            var serialNumber = new SerialNumber("SN-2024-001");

            // Act & Assert
            Assert.Equal("SN-2024-001", serialNumber.ToString());
        }

        [Fact]
        public void OperatorEquality_WithEqualSerials_ReturnsTrue()
        {
            // Arrange
            var serial1 = new SerialNumber("ABC123");
            var serial2 = new SerialNumber("ABC123");

            // Act & Assert
            Assert.True(serial1 == serial2);
        }

        [Fact]
        public void OperatorInequality_WithDifferentSerials_ReturnsTrue()
        {
            // Arrange
            var serial1 = new SerialNumber("ABC123");
            var serial2 = new SerialNumber("XYZ789");

            // Act & Assert
            Assert.True(serial1 != serial2);
        }
    }
}
