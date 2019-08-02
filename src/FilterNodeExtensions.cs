using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class FilterNodeExtensions
    {
        public static InvertedFilter<TLeafNode> Invert<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode, IFilterNode<TLeafNode>
        {
            return new InvertedFilter<TLeafNode>(filter);
        }

        // TODO: Maybe this can be made private/internal?
        public static Func<TItemToTest, bool> GetPredicate<TLeafNode, TItemToTest>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Func<TItemToTest, bool>> itemPredicate)
            where TLeafNode : class, ILeafFilterNode, IFilterNode<TLeafNode>
        {
            return filter.Aggregate(
                Combine,
                Invert,
                itemPredicate);
        }

        /// <summary>
        /// Removes redundancies in a filter to give simplest expression
        /// </summary>
        /// <typeparam name="TLeafNode"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Collapse<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode, IFilterNode<TLeafNode>
        {
            return filter.Match(
                combinationFilter =>
                {
                    return combinationFilter.Operator.Match(
                        () =>
                        {
                            var nonEmptyFilters = combinationFilter.Filters.Where(f => !f.Equals(FilterNode<TLeafNode>.Empty));
                            var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonEmptyFilters, combinationFilter.Operator);
                            return collapsedCombinationFilter.Filters.Count == 1
                                ? collapsedCombinationFilter.Filters.Single()
                                : collapsedCombinationFilter;
                        },
                        () =>
                        {
                            return combinationFilter.Filters.Any(f => f.Equals(FilterNode<TLeafNode>.Empty))
                                ? FilterNode<TLeafNode>.Empty
                                : combinationFilter;
                        });
                },
                invertedFilter => invertedFilter.FilterToInvert is IInvertedFilter<TLeafNode> invertedInner
                    ? invertedInner.FilterToInvert
                    : invertedFilter,
                leafFilter => leafFilter);
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