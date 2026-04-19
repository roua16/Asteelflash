namespace ITStockM.Domain.ValueObjects
{
    /// <summary>
    /// Represents warranty coverage period with start and end dates.
    /// Ensures end date is not before start date.
    /// </summary>
    public sealed class Warranty : IEquatable<Warranty>
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        /// <summary>Gets the number of months of warranty coverage.</summary>
        public int MonthsRemaining
        {
            get
            {
                var today = DateTime.UtcNow;
                if (today >= EndDate) return 0;
                return (EndDate.Year - today.Year) * 12 + (EndDate.Month - today.Month);
            }
        }

        /// <summary>Gets whether the warranty is still active.</summary>
        public bool IsActive => DateTime.UtcNow < EndDate;

        public Warranty(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("Warranty end date cannot be before start date.", nameof(endDate));

            StartDate = startDate;
            EndDate = endDate;
        }

        public bool Equals(Warranty? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Warranty other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartDate, EndDate);
        }

        public static bool operator ==(Warranty? left, Warranty? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Warranty? left, Warranty? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"Warranty from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} ({MonthsRemaining} months remaining)";
        }
    }
}
