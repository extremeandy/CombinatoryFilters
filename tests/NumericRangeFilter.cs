using System;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class NumericRangeFilter : LeafFilterNode<NumericRangeFilter>, IRealisableLeafFilterNode<int>, IEquatable<IFilterNode>
    {
        public NumericRangeFilter(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public int LowerBound { get; }

        public int UpperBound { get; }

        public bool Equals(IFilterNode other)
        {
            return other is NumericRangeFilter numericRangeOther
                   && LowerBound == numericRangeOther.LowerBound
                   && UpperBound == numericRangeOther.UpperBound;
        }

        public override bool Equals(object obj)
        {
            return obj is IFilterNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LowerBound.GetHashCode() * 397) ^ UpperBound.GetHashCode();
            }
        }

        public bool IsMatch(int item) => LowerBound <= item && item <= UpperBound;

        public override string ToString() => $"{LowerBound} to {UpperBound}";
    }
}