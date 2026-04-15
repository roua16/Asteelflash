using Xunit;
using ITStockM.Domain.ValueObjects;

namespace ITStockM.Tests.Domain.ValueObjects
{
    public class QuantityTests
    {
        [Fact]
        public void Constructor_WithValidValues_CreatesInstance()
        {
            // Arrange & Act
            var quantity = new Quantity(10.5m, "pieces");

            // Assert
            Assert.Equal(10.5m, quantity.Value);
            Assert.Equal("pieces", quantity.Unit);
        }

        [Fact]
        public void Constructor_WithNegativeValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Quantity(-5, "pieces"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_WithEmptyUnit_ThrowsArgumentException(string unit)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Quantity(10, unit));
        }

        [Fact]
        public void Constructor_TrimsUnitWhitespace()
        {
            // Arrange & Act
            var quantity = new Quantity(10, "  pieces  ");

            // Assert
            Assert.Equal("pieces", quantity.Unit);
        }

        [Fact]
        public void Addition_WithSameUnit_ReturnsSum()
        {
            // Arrange
            var q1 = new Quantity(5, "pieces");
            var q2 = new Quantity(3, "pieces");

            // Act
            var result = q1 + q2;

            // Assert
            Assert.Equal(8, result.Value);
            Assert.Equal("pieces", result.Unit);
        }

        [Fact]
        public void Addition_WithDifferentUnits_ThrowsInvalidOperationException()
        {
            // Arrange
            var q1 = new Quantity(5, "pieces");
            var q2 = new Quantity(3, "kilograms");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => q1 + q2);
        }

        [Fact]
        public void Subtraction_WithSameUnit_ReturnsDifference()
        {
            // Arrange
            var q1 = new Quantity(10, "pieces");
            var q2 = new Quantity(3, "pieces");

            // Act
            var result = q1 - q2;

            // Assert
            Assert.Equal(7, result.Value);
            Assert.Equal("pieces", result.Unit);
        }

        [Fact]
        public void Subtraction_WithDifferentUnits_ThrowsInvalidOperationException()
        {
            // Arrange
            var q1 = new Quantity(10, "pieces");
            var q2 = new Quantity(3, "kilograms");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => q1 - q2);
        }

        [Fact]
        public void Subtraction_ResultingInNegative_IsValid()
        {
            // Arrange
            var q1 = new Quantity(3, "pieces");
            var q2 = new Quantity(5, "pieces");

            // Act - Note: BaseCrudService handles validation, ValueObject allows it
            // This is realistic for inventory corrections
            var result = q1 - q2;

            // Assert
            Assert.Equal(-2, result.Value);
        }

        [Fact]
        public void Equals_SameQuantities_AreEqual()
        {
            // Arrange
            var q1 = new Quantity(5, "pieces");
            var q2 = new Quantity(5, "pieces");

            // Act & Assert
            Assert.Equal(q1, q2);
        }

        [Fact]
        public void Equals_DifferentUnits_AreNotEqual()
        {
            // Arrange
            var q1 = new Quantity(5, "pieces");
            var q2 = new Quantity(5, "kilograms");

            // Act & Assert
            Assert.NotEqual(q1, q2);
        }

        [Fact]
        public void ToString_ReturnsFriendlyFormat()
        {
            // Arrange
            var quantity = new Quantity(10.5m, "pieces");

            // Act
            var str = quantity.ToString();

            // Assert
            Assert.Equal("10.5 pieces", str);
        }
    }
}
