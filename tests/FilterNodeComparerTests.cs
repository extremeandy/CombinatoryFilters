using System.Collections.Generic;
using NSubstitute;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class FilterNodeComparerTests
    {
        [Fact]
        public void LeafFilterNode_ShouldPrecedeInvertedFilterNode()
        {
            var leafFilterNode = new CharFilter('A').ToLeafFilterNode();
            var invertedFilterNode = new InvertedFilterNode<CharFilter>(new CharFilter('B'));

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(leafFilterNode, invertedFilterNode);
            Assert.Equal(-1, result);

            var oppositeResult = sut.Compare(invertedFilterNode, leafFilterNode);
            Assert.Equal(1, oppositeResult);
        }

        [Fact]
        public void LeafFilterNode_ShouldPrecedeCombinationFilterNode()
        {
            var leafFilterNode = new CharFilter('A').ToLeafFilterNode();
            var combinationFilterNode = new CombinationFilterNode<CharFilter>(new [] { new CharFilter('B'), new CharFilter('C')});

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(leafFilterNode, combinationFilterNode);
            Assert.Equal(-1, result);

            var oppositeResult = sut.Compare(combinationFilterNode, leafFilterNode);
            Assert.Equal(1, oppositeResult);
        }

        [Fact]
        public void InvertedFilterNode_ShouldPrecedeCombinationFilterNode()
        {
            var invertedFilterNode = new InvertedFilterNode<CharFilter>(new CharFilter('A'));
            var combinationFilterNode = new CombinationFilterNode<CharFilter>(new[] { new CharFilter('B'), new CharFilter('C') });

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(invertedFilterNode, combinationFilterNode);
            Assert.Equal(-1, result);

            var oppositeResult = sut.Compare(combinationFilterNode, invertedFilterNode);
            Assert.Equal(1, oppositeResult);
        }

        [Fact]
        public void CombinationFilterNodeWithOperatorAll_ShouldPrecedeCombinationFilterNodeWithOperatorAny()
        {
            var allCombination = new CombinationFilterNode<CharFilter>(new[] { new CharFilter('B'), new CharFilter('C') }, CombinationOperator.All);
            var anyCombination = new CombinationFilterNode<CharFilter>(new[] { new CharFilter('B'), new CharFilter('C') }, CombinationOperator.Any);

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(allCombination, anyCombination);
            Assert.Equal(-1, result);

            var oppositeResult = sut.Compare(anyCombination, allCombination);
            Assert.Equal(1, oppositeResult);
        }

        [Fact]
        public void LeafFilterNodeComparison_ShouldUseFilterComparer()
        {
            var filterA = new CharFilter('A');
            var filterALeafNode = filterA.ToLeafFilterNode();
            var filterB = new CharFilter('B');
            var filterBLeafNode = filterB.ToLeafFilterNode();

            const int expectedResultFilterAVsFilterB = 1337;
            const int expectedResultFilterBVsFilterA = 1338;

            var filterComparer = Substitute.For<IComparer<CharFilter>>();
            filterComparer.Compare(filterA, filterB)
                .Returns(expectedResultFilterAVsFilterB);
            filterComparer.Compare(filterB, filterA)
                .Returns(expectedResultFilterBVsFilterA);

            var sut = new FilterNodeComparer<CharFilter>(filterComparer);
            var result = sut.Compare(filterALeafNode, filterBLeafNode);
            Assert.Equal(expectedResultFilterAVsFilterB, result);

            var oppositeResult = sut.Compare(filterBLeafNode, filterALeafNode);
            Assert.Equal(expectedResultFilterBVsFilterA, oppositeResult);
        }

        [Fact]
        public void InvertedFilterNodeComparison_ShouldCompareNodeToInvert()
        {
            var filterA = new CharFilter('A');
            var filterALeafNode = filterA.ToLeafFilterNode();
            var invertedFilterA = filterALeafNode.Invert();
            
            var filterB = new CharFilter('B');
            var filterBLeafNode = filterB.ToLeafFilterNode();
            var invertedFilterB = filterBLeafNode.Invert();

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(invertedFilterA, invertedFilterB);
            Assert.Equal(-1, result);

            var oppositeResult = sut.Compare(invertedFilterB, invertedFilterA);
            Assert.Equal(1, oppositeResult);
        }

        [Fact]
        public void CombinationFilterNodeComparison_ShouldUseLexicographicalListComparison()
        {
            var combination1 = new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('C') });
            var combination2 = new CombinationFilterNode<CharFilter>(new[] { new CharFilter('A'), new CharFilter('B') });

            var sut = FilterNodeComparer<CharFilter>.Default;
            var result = sut.Compare(combination1, combination2);
            Assert.Equal(1, result);

            var oppositeResult = sut.Compare(combination2, combination1);
            Assert.Equal(-1, oppositeResult);
        }
    }
}