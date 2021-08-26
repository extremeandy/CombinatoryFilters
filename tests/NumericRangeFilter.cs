using System;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class NumericRangeFilter : Filter<int>, IEquatable<NumericRangeFilter>, IComparable<NumericRangeFilter>
    {
        public NumericRangeFilter(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public int LowerBound { get; }

        public int UpperBound { get; }

        public bool Equals(NumericRangeFilter other)
        {
            if (other is null)
            {
                return false;
            }

            return LowerBound == other.LowerBound
                   && UpperBound == other.UpperBound;
        }

        public override bool Equals(object obj)
            => obj is NumericRangeFilter other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (LowerBound.GetHashCode() * 397) ^ UpperBound.GetHashCode();
            }
        }

        public override bool IsMatch(int item) => LowerBound <= item && item <= UpperBound;

        public override string ToString() => $"{LowerBound} to {UpperBound}";

        public int CompareTo(NumericRangeFilter other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var lowerBoundComparison = LowerBound.CompareTo(other.LowerBound);
            return lowerBoundComparison != 0
                ? lowerBoundComparison
                : UpperBound.CompareTo(other.UpperBound);
        }
    }
}