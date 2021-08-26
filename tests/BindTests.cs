using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class BindTests
    {
        [Fact]
        public void Bind_ReturnsCorrectStructureAndValues()
        {
            var filter5To10 = new NumericRangeFilter(5, 10);
            var filter8To15 = new NumericRangeFilter(8, 15);
            var filter5To10Or8To15 = new CombinationFilterNode<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Any);
            var filter9To12 = new NumericRangeFilter(9, 12);
            var filter = new CombinationFilterNode<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12.ToLeafFilterNode() }, CombinationOperator.All);

            var result = filter.Bind<NumericRangeFilter>(
                f =>
                {
                    if (ReferenceEquals(f, filter5To10))
                    {
                        return new CombinationFilterNode<NumericRangeFilter>(new [] { filter5To10, filter8To15 }, CombinationOperator.Any);
                    }

                    if (ReferenceEquals(f, filter8To15))
                    {
                        return FilterNode<NumericRangeFilter>.False;
                    }

                    if (ReferenceEquals(f, filter9To12))
                    {
                        return new InvertedFilterNode<NumericRangeFilter>(f);
                    }

                    return f.ToLeafFilterNode();
                });

            var expectedFilter = new CombinationFilterNode<NumericRangeFilter>(
                new IFilterNode<NumericRangeFilter>[]
                {
                    new CombinationFilterNode<NumericRangeFilter>(
                        new IFilterNode<NumericRangeFilter>[]
                        {
                            filter5To10Or8To15,
                            FilterNode<NumericRangeFilter>.False
                        }, CombinationOperator.Any),
                    new InvertedFilterNode<NumericRangeFilter>(filter9To12),
                }, CombinationOperator.All);

            Assert.Equal(expectedFilter, result);
        }
    }
}
