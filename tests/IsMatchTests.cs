using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class IsMatchTests
    {
        [Fact]
        public void ComplexFilter_IsMatch_FiltersToExpectedResults()
        {
            var filter5To10 = new NumericRangeFilter(5, 10);
            var filter8To15 = new NumericRangeFilter(8, 15);
            var filter5To10Or8To15 = new CombinationFilter<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Or);
            var filter9To12 = new NumericRangeFilter(9, 12);
            var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12 }, CombinationOperator.And);

            var values = new[] { 1, 3, 5, 9, 11 };
            var expectedFilteredValues = new[] { 9, 11 };

            var filteredValues = values.Where(v => filter.IsMatch(v));

            Assert.Equal(expectedFilteredValues, filteredValues);
        }
    }
}
