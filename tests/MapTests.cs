using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class MapTests
    {
        [Fact]
        public void Map_ReturnsCorrectStructureAndValues()
        {
            var filter5To10 = new NumericRangeFilter(5, 10);
            var filter8To15 = new NumericRangeFilter(8, 15);
            var filter5To10Or8To15 = new CombinationFilterNode<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Any);
            var filter9To12 = new NumericRangeFilter(9, 12);
            var filter = new CombinationFilterNode<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12.ToLeafFilterNode() }, CombinationOperator.All);

            var result = filter.Map(
                f =>
                {
                    var newLowerBound = f.LowerBound + 1;
                    var newUpperBound = f.UpperBound - 1;
                    return new NumericRangeFilter(newLowerBound, newUpperBound);
                });

            var filter6To9 = new NumericRangeFilter(6, 9);
            var filter9To14 = new NumericRangeFilter(9, 14);
            var filter6To9Or9To14 = new CombinationFilterNode<NumericRangeFilter>(new[] { filter6To9, filter9To14 }, CombinationOperator.Any);
            var filter10To11 = new NumericRangeFilter(10, 11);
            var expectedFilter = new CombinationFilterNode<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter6To9Or9To14, filter10To11.ToLeafFilterNode() }, CombinationOperator.All);

            Assert.Equal(expectedFilter, result);
        }
    }
}
