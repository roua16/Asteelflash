namespace ITStockM.Domain.ValueObjects
{
    /// <summary>
    /// Represents a serial number for IT assets.
    /// Enforces non-empty, trimmed values and provides equality comparison.
    /// </summary>
    public sealed class SerialNumber : IEquatable<SerialNumber>
    {
        public string Value { get; }

        public SerialNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Serial number cannot be empty or whitespace.", nameof(value));

            Value = value.Trim();
        }

        public bool Equals(SerialNumber? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is SerialNumber other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator ==(SerialNumber? left, SerialNumber? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SerialNumber? left, SerialNumber? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
