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

                // Also check IsEquivalentTo works, which internally uses Collapse.
                Assert.True(filter.IsEquivalentTo(collapsedFilter));
                Assert.True(collapsedFilter.IsEquivalentTo(filter));

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

        [Fact]
        public void Collapse_ShouldRemoveDuplicates()
        {
            var filter = new CharFilter('A');
            var leafFilterNode = filter.ToLeafFilterNode();
            var combinationFilterNode = new CombinationFilterNode<CharFilter>(new[] { leafFilterNode, leafFilterNode });
            var collapsedFilterNode = combinationFilterNode.Collapse();
            var expectedFilterNode = filter.ToLeafFilterNode();
            Assert.Equal(expectedFilterNode, collapsedFilterNode);

            // Also check IsEquivalentTo works, which internally uses Collapse.
            Assert.True(leafFilterNode.IsEquivalentTo(combinationFilterNode));
            Assert.True(combinationFilterNode.IsEquivalentTo(leafFilterNode));
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

            var innerCombinationFilterToFlatten = new CombinationFilterNode<CharFilter>(new [] { filterA, filterB }, outerCombinationOperator);
            var innerCombinationFilterToRetain1 = new CombinationFilterNode<CharFilter>(new [] { filterC, filterD }, alternateInnerCombinationOperator);
            var innerCombinationFilterToRetain2 = new CombinationFilterNode<CharFilter>(new[] { filterE, filterF }, alternateInnerCombinationOperator);

            // When outer operator == All and other == Any, this is equivalent to
            // (A AND B) AND (C OR D) AND (E OR F) AND G AND H
            // Which can be collapsed to: A AND B AND G AND H AND (C OR D) AND (E OR F)
            // When outer operator == Any and other == All, this is equivalent to
            // (A OR B) OR (C AND D) OR (E AND F) OR G OR H
            // Which can be collapsed to: A OR B OR G OR H OR (C AND D) OR (E AND F)
            var outerCombinationFilter = new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
            {
                innerCombinationFilterToFlatten,
                innerCombinationFilterToRetain1,
                innerCombinationFilterToRetain2,
                filterG.ToLeafFilterNode(),
                filterH.ToLeafFilterNode()
            }, outerCombinationOperator);

            var collapsedFilter = outerCombinationFilter.Collapse();

            var expectedCollapsedFilter = new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
            {
                filterA.ToLeafFilterNode(),
                filterB.ToLeafFilterNode(),
                innerCombinationFilterToRetain1,
                innerCombinationFilterToRetain2,
                filterG.ToLeafFilterNode(),
                filterH.ToLeafFilterNode(),
            }, outerCombinationOperator);

            Assert.Equal(expectedCollapsedFilter, collapsedFilter);

            // Also check IsEquivalentTo works, which internally uses Collapse.
            Assert.True(collapsedFilter.IsEquivalentTo(outerCombinationFilter));
            Assert.True(outerCombinationFilter.IsEquivalentTo(collapsedFilter));
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

            var innerCombinationFilterToFlatten = new CombinationFilterNode<CharFilter>(new[] { filterA, filterB }, outerCombinationOperator);
            var innerCombinationFilterToRetain = new CombinationFilterNode<CharFilter>(new[] { filterC, filterD }, alternateInnerCombinationOperator);
            var innerCombinationFilterToAbsorb = new CombinationFilterNode<CharFilter>(new[] { filterA, filterE }, alternateInnerCombinationOperator);

            var outerCombinationFilter = new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
            {
                innerCombinationFilterToFlatten,
                innerCombinationFilterToRetain,
                innerCombinationFilterToAbsorb,
                filterG.ToLeafFilterNode(),
                filterH.ToLeafFilterNode()
            }, outerCombinationOperator);

            var collapsedFilter = outerCombinationFilter.Collapse();

            var expectedCollapsedFilter = new CombinationFilterNode<CharFilter>(new IFilterNode<CharFilter>[]
            {
                filterA.ToLeafFilterNode(),
                filterB.ToLeafFilterNode(),
                innerCombinationFilterToRetain,
                filterG.ToLeafFilterNode(),
                filterH.ToLeafFilterNode(),
            }, outerCombinationOperator);

            Assert.Equal(expectedCollapsedFilter, collapsedFilter);

            // Also check IsEquivalentTo works, which internally uses Collapse.
            Assert.True(collapsedFilter.IsEquivalentTo(outerCombinationFilter));
            Assert.True(outerCombinationFilter.IsEquivalentTo(collapsedFilter));
        }
    }
}
