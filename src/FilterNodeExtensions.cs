using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class FilterNodeExtensions
    {
        public static InvertedFilter<TLeafNode> Invert<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode
        {
            return new InvertedFilter<TLeafNode>(filter);
        }

        // TODO: Maybe this can be made private/internal?
        public static Func<TItemToTest, bool> GetPredicate<TLeafNode, TItemToTest>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Func<TItemToTest, bool>> itemPredicate)
            where TLeafNode : class, ILeafFilterNode
        {
            return filter.Aggregate(
                Combine,
                Invert,
                itemPredicate);
        }

        internal static Func<TItemToTest, bool> Combine<TItemToTest>(
            IEnumerable<Func<TItemToTest, bool>> innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AndReducer()
                => relatedItemCollection => innerResults.All(comparator => comparator(relatedItemCollection));

            Func<TItemToTest, bool> OrReducer()
                => relatedItemCollection => innerResults.Any(comparator => comparator(relatedItemCollection));

            return @operator.Match(AndReducer, OrReducer);
        }

        internal static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);
    }
}