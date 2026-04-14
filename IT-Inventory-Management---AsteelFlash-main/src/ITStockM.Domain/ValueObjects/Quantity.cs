namespace ITStockM.Domain.ValueObjects
{
    using System.Globalization;

    /// <summary>
    /// Represents a quantity with a numeric value and unit of measurement.
    /// Enforces non-negative values.
    /// </summary>
    public sealed class Quantity : IEquatable<Quantity>
    {
        public decimal Value { get; }
        public string Unit { get; }

        public Quantity(decimal value, string unit)
        {
            if (value < 0)
                throw new ArgumentException("Quantity value cannot be negative.", nameof(value));

            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit cannot be empty.", nameof(unit));

            Value = value;
            Unit = unit.Trim();
        }

        public bool Equals(Quantity? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value && Unit.Equals(other.Unit, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Quantity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Unit.ToUpperInvariant());
        }

        public static bool operator ==(Quantity? left, Quantity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quantity? left, Quantity? right)
        {
            return !Equals(left, right);
        }

        public static Quantity operator +(Quantity left, Quantity right)
        {
            if (!left.Unit.Equals(right.Unit, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot add quantities with different units.");

            return new Quantity(left.Value + right.Value, left.Unit);
        }

        public static Quantity operator -(Quantity left, Quantity right)
        {
            if (!left.Unit.Equals(right.Unit, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot subtract quantities with different units.");

            return new Quantity(left.Value - right.Value, left.Unit);
        }

        public override string ToString()
        {
            return $"{Value.ToString(CultureInfo.InvariantCulture)} {Unit}";
        }
    }
}
