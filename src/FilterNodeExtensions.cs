using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
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

        /// <summary>
        /// Computes the filter that includes only leaf filters that satisfy the given predicate.
        /// The result is guaranteed to be less than or equal in restrictiveness to the original.
        /// This means that it will never filter out results that the original filter would not have
        /// filtered.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> GetPartial<TLeafNode>(this IFilterNode<TLeafNode> filter, Func<TLeafNode, bool> predicate)
            where TLeafNode : class, ILeafFilterNode
            => filter.Relax(
                    leafFilter => predicate(leafFilter)
                        ? (IFilterNode<TLeafNode>) leafFilter
                        : FilterNode<TLeafNode>.True,
                    leafFilter => predicate(leafFilter)
                        ? (IFilterNode<TLeafNode>) leafFilter
                        : FilterNode<TLeafNode>.False)
                .Collapse();

        internal static Func<TItemToTest, bool> Combine<TItemToTest>(
            IEnumerable<Func<TItemToTest, bool>> innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AllReducer()
                => relatedItemCollection => innerResults.All(comparator => comparator(relatedItemCollection));

            Func<TItemToTest, bool> AnyReducer()
                => relatedItemCollection => innerResults.Any(comparator => comparator(relatedItemCollection));

            return @operator.Match(AllReducer, AnyReducer);
        }

        internal static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);

        public static async Task<TResult> MatchAsync<TLeafNode, TResult>(
            this IFilterNode<TLeafNode> filter,
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, Task<TResult>> transform)
            where TLeafNode : class, ILeafFilterNode
        {
            switch (filter)
            {
                case ICombinationFilterNode<TLeafNode> combinationFilter:
                    return combine(combinationFilter);
                case IInvertedFilter<TLeafNode> invertedFilter:
                    return invert(invertedFilter);
                case TLeafNode leafFilter:
                    return await transform(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        public static async Task<TResult> MatchAsync<TLeafNode, TResult>(
            this IFilterNode<TLeafNode> filter,
            Func<ICombinationFilterNode<TLeafNode>, Task<TResult>> combine,
            Func<IInvertedFilter<TLeafNode>, Task<TResult>> invert,
            Func<TLeafNode, Task<TResult>> transform)
            where TLeafNode : class, ILeafFilterNode
        {
            switch (filter)
            {
                case ICombinationFilterNode<TLeafNode> combinationFilter:
                    return await combine(combinationFilter);
                case IInvertedFilter<TLeafNode> invertedFilter:
                    return await invert(invertedFilter);
                case TLeafNode leafFilter:
                    return await transform(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        public static async Task<IFilterNode<TResultLeafNode>> MapAsync<TLeafNode, TResultLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Task<TResultLeafNode>> mapFunc)
            where TLeafNode : class, ILeafFilterNode
            where TResultLeafNode : class, ILeafFilterNode
        {
            switch (filter)
            {
                case ICombinationFilterNode<TLeafNode> combinationFilter:
                    var innerFilterTasks = combinationFilter.Filters.Select(f => f.MapAsync(mapFunc));
                    var innerFilters = await Task.WhenAll(innerFilterTasks);
                    return new CombinationFilter<TResultLeafNode>(innerFilters, combinationFilter.Operator);
                case IInvertedFilter<TLeafNode> invertedFilter:
                    var innerFilter = await invertedFilter.FilterToInvert.MapAsync(mapFunc);
                    return new InvertedFilter<TResultLeafNode>(innerFilter);
                case TLeafNode leafFilter:
                    return (IFilterNode<TResultLeafNode>)await mapFunc(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        /// <summary>
        /// Removes non-matching nodes and replaces them with <see cref="FilterNode{TLeafNode}.True"/>
        /// </summary>
        /// <typeparam name="TLeafNode"></typeparam>
        /// <param name="filter"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Where<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, bool> predicate)
            where TLeafNode : class, ILeafFilterNode
            => filter.Bind(leafFilter =>
            {
                if (predicate(leafFilter))
                {
                    return (IFilterNode<TLeafNode>)leafFilter;
                }

                return FilterNode<TLeafNode>.True;
            });
    }
}