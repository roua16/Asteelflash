namespace ITStockM.Domain.ValueObjects
{
    /// <summary>
    /// Represents monetary value with amount and currency code.
    /// Enforces non-negative amounts and valid ISO 4217 currency codes.
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string CurrencyCode { get; }

        public Money(decimal amount, string currencyCode = "USD")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
                throw new ArgumentException("Currency code must be a valid 3-letter ISO 4217 code.", nameof(currencyCode));

            Amount = amount;
            CurrencyCode = currencyCode.ToUpperInvariant();
        }

        public bool Equals(Money? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Amount == other.Amount && CurrencyCode == other.CurrencyCode;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, CurrencyCode);
        }

        public static bool operator ==(Money? left, Money? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Money? left, Money? right)
        {
            return !Equals(left, right);
        }

        public static Money operator +(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot add {left.CurrencyCode} and {right.CurrencyCode}.");

            return new Money(left.Amount + right.Amount, left.CurrencyCode);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot subtract {right.CurrencyCode} from {left.CurrencyCode}.");

            return new Money(left.Amount - right.Amount, left.CurrencyCode);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            return new Money(money.Amount * multiplier, money.CurrencyCode);
        }

        public override string ToString()
        {
            return $"{Amount:F2} {CurrencyCode}";
        }
    }
}
