using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        public static bool IsEquivalent<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            IFilterNode<TLeafNode> other)
            where TLeafNode : class, ILeafFilterNode
            => filter.Collapse().Equals(other.Collapse());

    }
}