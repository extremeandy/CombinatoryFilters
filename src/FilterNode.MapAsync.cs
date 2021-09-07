using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
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
                    var innerNodes = await Task.WhenAll(innerNodesTasks).ConfigureAwait(false);
                    return innerNodes.Combine(combinationFilter.Operator);
                case IInvertedFilterNode<TFilter> invertedFilter:
                    var innerNodeToInvert = await invertedFilter.NodeToInvert.MapAsync(mapFunc).ConfigureAwait(false);
                    return innerNodeToInvert.Invert();
                case ILeafFilterNode<TFilter> leafFilter:
                    return (await mapFunc(leafFilter.Filter).ConfigureAwait(false)).ToLeafFilterNode();
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }
    }
}