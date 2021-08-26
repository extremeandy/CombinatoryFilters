using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class SortTests
    {
        [Theory]
        [MemberData(nameof(GetTestData))]
        public void Sort_ReturnsExpectedResult(IFilterNode<CharFilter> filterNode, IFilterNode<CharFilter> expectedResult)
        {
            var result = filterNode.Sort();
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> GetTestData()
        {
            var testCases = new (IFilterNode<CharFilter> FilterNode, IFilterNode<CharFilter> ExpectedResult)[]
            {
                (
                    // 'A' should precede 'B'
                    FilterNode: new CombinationFilterNode<CharFilter>(new[] {  new CharFilter('A'), new CharFilter('B'), new CharFilter('A') }),
                    ExpectedResult: new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('A'), new CharFilter('B') })
                ),
                (
                    // Inner combination on inverted filter should be sorted
                    FilterNode: new InvertedFilterNode<CharFilter>(
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('B'), new CharFilter('A') })),
                    ExpectedResult: new InvertedFilterNode<CharFilter>(
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('B') }))
                ),
                (
                    // Leaf filter should precede inverted filter, and inverted filter should preced combination filter.
                    // This also tests that CombinationFilterNode recursively sorts its children.
                    FilterNode: new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
                    {
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('G'), new CharFilter('H') }),
                        new InvertedFilterNode<CharFilter>(new CombinationFilterNode<CharFilter>(new[] { new CharFilter('D'), new CharFilter('C') })),
                        new CharFilter('F').ToLeafFilterNode(),
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('E'), new CharFilter('D') }),
                        new CharFilter('A').ToLeafFilterNode(),
                    }),
                    ExpectedResult: new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
                    {
                        new CharFilter('A').ToLeafFilterNode(),
                        new CharFilter('F').ToLeafFilterNode(),
                        new InvertedFilterNode<CharFilter>(new CombinationFilterNode<CharFilter>(new[] { new CharFilter('C'), new CharFilter('D') })),
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('D'), new CharFilter('E') }),
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('G'), new CharFilter('H') }),
                    })
                ),
                (
                    // Operator All should precede Operator Any
                    FilterNode: new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
                    {
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('B') }, CombinationOperator.Any),
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('C'), new CharFilter('D') }, CombinationOperator.All),
                    }),
                    ExpectedResult: new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
                    {
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('C'), new CharFilter('D') }, CombinationOperator.All),
                        new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('B') }, CombinationOperator.Any),
                    })
                ),
            };

            return testCases.Select(tc => new object[] {tc.FilterNode, tc.ExpectedResult});
        }
    }
}