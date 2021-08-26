using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class EqualityTests
    {
        [Fact]
        public void LeafFilterNode_Equality_IsNotAffectedByGenericType()
        {
            IFilterNode<CharFilter> leafNode1 = new CharFilterSubClass('A').ToLeafFilterNode<CharFilter>();

            // Note combination2 is constructed using a more specific generic type than combination1.
            IFilterNode<CharFilter> leafNode2 = new CharFilterSubClass('A').ToLeafFilterNode<CharFilterSubClass>();

            Assert.Equal(leafNode1, leafNode2);
            Assert.Equal(leafNode2, leafNode1);
        }

        [Fact]
        public void InvertedFilterNode_Equality_IsNotAffectedByGenericType()
        {
            IFilterNode<CharFilter> invertedNode1 = new InvertedFilterNode<CharFilter>(new CharFilterSubClass('A'));

            // Note combination2 is constructed using a more specific generic type than combination1.
            IFilterNode<CharFilter> invertedNode2 = new InvertedFilterNode<CharFilterSubClass>(new CharFilterSubClass('A'));

            Assert.Equal(invertedNode1, invertedNode2);
            Assert.Equal(invertedNode2, invertedNode1);
        }

        [Fact]
        public void CombinationFilterNode_Equality_IsNotAffectedByGenericType()
        {
            IFilterNode<CharFilter> combination1 = new CombinationFilterNode<CharFilter>(new[] { new CharFilterSubClass('A'), new CharFilterSubClass('B') });

            // Note combination2 is constructed using a more specific generic type than combination1.
            IFilterNode<CharFilter> combination2 = new CombinationFilterNode<CharFilterSubClass>(new[] { new CharFilterSubClass('A'), new CharFilterSubClass('B') });

            Assert.Equal(combination1, combination2);
            Assert.Equal(combination2, combination1);
        }

        private class CharFilterSubClass : CharFilter
        {
            public CharFilterSubClass(char c)
                : base(c)
            {
            }
        }
    }
}
