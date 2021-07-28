using System;
using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class CollapseTests
    {
        private readonly Random _random;
        private readonly RandomCharFilterGenerator _generator;

        public CollapseTests()
        {
            _random = new Random();
            _generator = new RandomCharFilterGenerator(_random);
        }

        [Fact]
        public void Collapse_FilterIsEquivalent()
        {
            var interestingCount = 0;
            while (interestingCount < 10)
            {
                var numSets = _random.Next(1000);

                var strings = Enumerable.Range(1, numSets).Select(_ => _generator.GetRandomString(_random.Next(RandomCharFilterGenerator.Chars.Length)))
                    .ToHashSet();

                const int nodeCount = 10;

                var filter = _generator.GetRandomFilter(nodeCount);
                var collapsedFilter = filter.Collapse();
                
                if (filter.Equals(collapsedFilter))
                {
                    // Trivial, ignore.
                    continue;
                }

                var expectedResult = strings.Where(filter.GetPredicate<CharFilter, string>());
                var actualResult = strings.Where(collapsedFilter.GetPredicate<CharFilter, string>());

                Assert.Equal(expectedResult, actualResult);
                interestingCount++;
            }
        }

        [Fact]
        public void Collapse_ShouldBeReferenceEqual_WhenAlreadyCollapsed()
        {
            var interestingCount = 0;
            while (interestingCount < 10)
            {
                const int nodeCount = 10;

                var filter = _generator.GetRandomFilter(nodeCount);
                var collapsedFilter = filter.Collapse();

                if (filter.Equals(collapsedFilter))
                {
                    // Trivial, ignore.
                    continue;
                }

                var twiceCollapsedFilter = collapsedFilter.Collapse();

                Assert.Same(collapsedFilter, twiceCollapsedFilter);

                interestingCount++;
            }
        }

        [Theory]
        [InlineData(CombinationOperator.All, CombinationOperator.Any)]
        [InlineData(CombinationOperator.Any, CombinationOperator.All)]
        public void Collapse_ShouldFlattenNestedCombinations_WhenInnerOperatorMatchesOuter(CombinationOperator outerCombinationOperator, CombinationOperator alternateInnerCombinationOperator)
        {
            var filterA = new CharFilter('A');
            var filterB = new CharFilter('B');
            var filterC = new CharFilter('C');
            var filterD = new CharFilter('D');
            var filterE = new CharFilter('E');
            var filterF = new CharFilter('F');
            var filterG = new CharFilter('G');
            var filterH = new CharFilter('H');

            var innerCombinationFilterToFlatten = new CombinationFilter<CharFilter>(new [] { filterA, filterB }, outerCombinationOperator);
            var innerCombinationFilterToRetain1 = new CombinationFilter<CharFilter>(new [] { filterC, filterD }, alternateInnerCombinationOperator);
            var innerCombinationFilterToRetain2 = new CombinationFilter<CharFilter>(new[] { filterE, filterF }, alternateInnerCombinationOperator);

            // When outer operator == All and other == Any, this is equivalent to
            // (A AND B) AND (C OR D) AND (E OR F) AND G AND H
            // Which can be collapsed to: A AND B AND G AND H AND (C OR D) AND (E OR F)
            // When outer operator == Any and other == All, this is equivalent to
            // (A OR B) OR (C AND D) OR (E AND F) OR G OR H
            // Which can be collapsed to: A OR B OR G OR H OR (C AND D) OR (E AND F)
            var outerCombinationFilter = new CombinationFilter<CharFilter>(new IFilterNode<CharFilter>[]
            {
                innerCombinationFilterToFlatten,
                innerCombinationFilterToRetain1,
                innerCombinationFilterToRetain2,
                filterG,
                filterH
            }, outerCombinationOperator);

            var collapsedFilter = outerCombinationFilter.Collapse();

            var expectedCollapsedFilter = new CombinationFilter<CharFilter>(new IFilterNode<CharFilter>[]
            {
                filterA,
                filterB,
                filterG,
                filterH,
                innerCombinationFilterToRetain1,
                innerCombinationFilterToRetain2
            }, outerCombinationOperator);

            Assert.Equal(expectedCollapsedFilter, collapsedFilter);
        }

        [Theory]
        [InlineData(CombinationOperator.All, CombinationOperator.Any)]
        [InlineData(CombinationOperator.Any, CombinationOperator.All)]
        public void Collapse_ShouldAbsorbRedundantNestedCombinations(CombinationOperator outerCombinationOperator, CombinationOperator alternateInnerCombinationOperator)
        {
            var filterA = new CharFilter('A');
            var filterB = new CharFilter('B');
            var filterC = new CharFilter('C');
            var filterD = new CharFilter('D');
            var filterE = new CharFilter('E');
            var filterF = new CharFilter('F');
            var filterG = new CharFilter('G');
            var filterH = new CharFilter('H');

            var innerCombinationFilterToFlatten = new CombinationFilter<CharFilter>(new[] { filterA, filterB }, outerCombinationOperator);
            var innerCombinationFilterToRetain = new CombinationFilter<CharFilter>(new[] { filterC, filterD }, alternateInnerCombinationOperator);
            var innerCombinationFilterToAbsorb = new CombinationFilter<CharFilter>(new[] { filterA, filterE }, alternateInnerCombinationOperator);

            var outerCombinationFilter = new CombinationFilter<CharFilter>(new IFilterNode<CharFilter>[]
            {
                innerCombinationFilterToFlatten,
                innerCombinationFilterToRetain,
                innerCombinationFilterToAbsorb,
                filterG,
                filterH
            }, outerCombinationOperator);

            var collapsedFilter = outerCombinationFilter.Collapse();

            var expectedCollapsedFilter = new CombinationFilter<CharFilter>(new IFilterNode<CharFilter>[]
            {
                filterA,
                filterB,
                filterG,
                filterH,
                innerCombinationFilterToRetain,
            }, outerCombinationOperator);

            Assert.Equal(expectedCollapsedFilter, collapsedFilter);
        }
    }
}
