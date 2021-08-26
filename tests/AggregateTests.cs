using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class AggregateTests
    {
        [Fact]
        public void Aggregate_ReturnsExpectedResult_WhenComputingLengthOfLongestInterval()
        {
            var filter5To10 = new NumericRangeFilter(5, 10);
            var filter8To15 = new NumericRangeFilter(8, 15);
            var filter5To10Or8To15 = new CombinationFilterNode<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Any);
            var filter9To12 = new NumericRangeFilter(9, 12);
            var filter = new CombinationFilterNode<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12.ToLeafFilterNode() }, CombinationOperator.All);

            // Reduce the filter to get the length of the longest interval
            var result = filter.Aggregate<double>(
                (lengths, _) => lengths.Max(), 
                length => double.PositiveInfinity,
                leafFilterNode => leafFilterNode.Filter.UpperBound - leafFilterNode.Filter.LowerBound);

            // Max is 15 - 8
            Assert.Equal(7, result);
        }

        [Fact]
        public void Aggregate_ReturnsExpectedResult_WhenComputingMinimumLowerBound()
        {
            var filter5To10 = new LeafFilterNode<NumericRangeFilter>(new NumericRangeFilter(5, 10));
            var filter = filter5To10.Invert();
            
            double GetLowerBound(NumericRangeFilter f)
            {
                return f.LowerBound;
            }

            // Reduce the filter to get the length of the longest interval
            var result = filter.Aggregate(
                (lowerBounds, _) => lowerBounds.Min(),
                length => double.NegativeInfinity,
                leafFilterNode => GetLowerBound(leafFilterNode.Filter));

            // Max is 15 - 8
            Assert.Equal(double.NegativeInfinity, result);
        }
    }
}
