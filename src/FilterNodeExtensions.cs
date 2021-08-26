using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        public static InvertedFilterNode<TFilter> Invert<TFilter>(this IFilterNode<TFilter> filterNode)
            where TFilter : IFilter
            => new InvertedFilterNode<TFilter>(filterNode);

        // TODO: Maybe this can be made private/internal?
        public static Func<TItemToTest, bool> GetPredicate<TFilter, TItemToTest>(
            this IFilterNode<TFilter> filterNode,
            Func<TFilter, Func<TItemToTest, bool>> itemPredicate)
            => filterNode.Aggregate(
                Combine,
                Invert,
                leafFilterNode => itemPredicate(leafFilterNode.Filter));

        /// <summary>
        /// Computes the filter that includes only leaf filters that satisfy the given predicate.
        /// The result is guaranteed to be less than or equal in restrictiveness to the original.
        /// This means that it will never filter out results that the original filter would not have
        /// filtered.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TFilter> GetPartial<TFilter>(this IFilterNode<TFilter> node, Func<TFilter, bool> predicate)
            where TFilter : IFilter
            => node.Relax(
                filter =>
                {
                    if (predicate(filter))
                    {
                        return filter.ToLeafFilterNode();
                    }

                    return FilterNode<TFilter>.True;
                },
                filter =>
                {
                    if (predicate(filter))
                    {
                        return filter.ToLeafFilterNode();
                    }

                    return FilterNode<TFilter>.False;
                });

        internal static Func<TItemToTest, bool> Combine<TItemToTest>(
            Func<TItemToTest, bool>[] innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AllReducer()
                => relatedItemCollection =>
                {
                    for (var i = 0; i < innerResults.Length; i++)
                    {
                        if (!innerResults[i](relatedItemCollection))
                        {
                            return false;
                        }
                    }

                    return true;
                };

            Func<TItemToTest, bool> AnyReducer()
                => relatedItemCollection =>
                {
                    for (var i = 0; i < innerResults.Length; i++)
                    {
                        if (innerResults[i](relatedItemCollection))
                        {
                            return true;
                        }
                    }

                    return false;
                };

            return @operator.Match(AllReducer, AnyReducer);
        }

        internal static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);

        public static async Task<TResult> MatchAsync<TFilter, TResult>(
            this IFilterNode<TFilter> node,
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, Task<TResult>> transform)
        {
            switch (node)
            {
                case ICombinationFilterNode<TFilter> combinationNode:
                    return combine(combinationNode);
                case IInvertedFilterNode<TFilter> invertedNode:
                    return invert(invertedNode);
                case ILeafFilterNode<TFilter> leafNode:
                    return await transform(leafNode);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(node)} of type: {node.GetType()}");
            }
        }

        public static async Task<TResult> MatchAsync<TFilter, TResult>(
            this IFilterNode<TFilter> filter,
            Func<ICombinationFilterNode<TFilter>, Task<TResult>> combine,
            Func<IInvertedFilterNode<TFilter>, Task<TResult>> invert,
            Func<ILeafFilterNode<TFilter>, Task<TResult>> transform)
        {
            switch (filter)
            {
                case ICombinationFilterNode<TFilter> combinationFilter:
                    return await combine(combinationFilter);
                case IInvertedFilterNode<TFilter> invertedFilter:
                    return await invert(invertedFilter);
                case ILeafFilterNode<TFilter> leafFilter:
                    return await transform(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        public static async Task<IFilterNode<TResultFilter>> MapAsync<TFilter, TResultFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, Task<TResultFilter>> mapFunc)
            where TFilter : IFilter
            where TResultFilter : IFilter
        {
            switch (filter)
            {
                case ICombinationFilterNode<TFilter> combinationFilter:
                    var innerNodesTasks = combinationFilter.Nodes.Select(f => f.MapAsync(mapFunc));
                    var innerNodes = await Task.WhenAll(innerNodesTasks);
                    return new CombinationFilterNode<TResultFilter>(innerNodes, combinationFilter.Operator);
                case IInvertedFilterNode<TFilter> invertedFilter:
                    var innerNodeToInvert = await invertedFilter.NodeToInvert.MapAsync(mapFunc);
                    return new InvertedFilterNode<TResultFilter>(innerNodeToInvert);
                case ILeafFilterNode<TFilter> leafFilter:
                    return (await mapFunc(leafFilter.Filter)).ToLeafFilterNode();
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        /// <summary>
        /// Removes non-matching nodes and replaces them with <see cref="FilterNode{TFilter}.True"/>
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="filter"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TFilter> Where<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, bool> predicate)
            where TFilter : IFilter
            => filter.Bind<TFilter>(leafFilter =>
            {
                if (predicate(leafFilter))
                {
                    return leafFilter.ToLeafFilterNode();
                }

                return FilterNode<TFilter>.True;
            });
    }
}