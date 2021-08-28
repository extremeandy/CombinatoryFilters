using System;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
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
    }
}