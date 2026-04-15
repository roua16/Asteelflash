using Xunit;
using ITStockM.Domain.ValueObjects;

namespace ITStockM.Tests.Domain.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_WithValidAmount_CreatesInstance()
        {
            // Arrange & Act
            var money = new Money(100.50m, "USD");

            // Assert
            Assert.Equal(100.50m, money.Amount);
            Assert.Equal("USD", money.CurrencyCode);
        }

        [Fact]
        public void Constructor_WithNegativeAmount_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Money(-50, "USD"));
        }

        [Fact]
        public void Constructor_WithDefaultCurrency_UsesUSD()
        {
            // Arrange & Act
            var money = new Money(100);

            // Assert
            Assert.Equal("USD", money.CurrencyCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("US")]
        [InlineData("USDA")]
        public void Constructor_WithInvalidCurrencyCode_ThrowsArgumentException(string code)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Money(100, code));
        }

        [Fact]
        public void Constructor_UppercasesCurrencyCode()
        {
            // Arrange & Act
            var money = new Money(100, "usd");

            // Assert
            Assert.Equal("USD", money.CurrencyCode);
        }

        [Fact]
        public void Addition_WithSameCurrency_ReturnsSum()
        {
            // Arrange
            var m1 = new Money(50, "USD");
            var m2 = new Money(30, "USD");

            // Act
            var result = m1 + m2;

            // Assert
            Assert.Equal(80, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Addition_WithDifferentCurrencies_ThrowsInvalidOperationException()
        {
            // Arrange
            var m1 = new Money(50, "USD");
            var m2 = new Money(30, "EUR");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => m1 + m2);
        }

        [Fact]
        public void Subtraction_WithSameCurrency_ReturnsDifference()
        {
            // Arrange
            var m1 = new Money(100, "USD");
            var m2 = new Money(30, "USD");

            // Act
            var result = m1 - m2;

            // Assert
            Assert.Equal(70, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Multiplication_WithDecimal_ReturnsProduct()
        {
            // Arrange
            var money = new Money(100, "USD");

            // Act
            var result = money * 1.5m;

            // Assert
            Assert.Equal(150, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Multiplication_WithZero_ReturnsZero()
        {
            // Arrange
            var money = new Money(100, "USD");

            // Act
            var result = money * 0;

            // Assert
            Assert.Equal(0, result.Amount);
        }

        [Fact]
        public void Equals_SameAmountAndCurrency_AreEqual()
        {
            // Arrange
            var m1 = new Money(100.50m, "USD");
            var m2 = new Money(100.50m, "USD");

            // Act & Assert
            Assert.Equal(m1, m2);
        }

        [Fact]
        public void Equals_DifferentCurrencies_AreNotEqual()
        {
            // Arrange
            var m1 = new Money(100, "USD");
            var m2 = new Money(100, "EUR");

            // Act & Assert
            Assert.NotEqual(m1, m2);
        }

        [Fact]
        public void ToString_ReturnsFriendlyFormat()
        {
            // Arrange
            var money = new Money(1234.56m, "USD");

            // Act
            var str = money.ToString();

            // Assert
            Assert.Equal("1234.56 USD", str);
        }

        [Fact]
        public void GetHashCode_SameMoney_SameHashCode()
        {
            // Arrange
            var m1 = new Money(100.50m, "USD");
            var m2 = new Money(100.50m, "USD");

            // Act & Assert
            Assert.Equal(m1.GetHashCode(), m2.GetHashCode());
        }
    }
}
