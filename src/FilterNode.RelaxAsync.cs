using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Relaxes' a filter by relaxing each leaf of the filter according to <see cref="relaxFilterFunc"/>, or restricting
        /// inversions according to <see cref="restrictFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="RestrictAsync{T}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="relaxFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <param name="restrictFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <returns></returns>
        public static async Task<IFilterNode<TFilter>> RelaxAsync<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, Task<IFilterNode<TFilter>>> relaxFilterFunc,
            Func<TFilter, Task<IFilterNode<TFilter>>> restrictFilterFunc)
            where TFilter : IFilter
        {
            var result = await filter.Match<Task<IFilterNode<TFilter>>>(
                async combinationFilterNode =>
                {
                    var innerFilterNodeTasks = combinationFilterNode.Nodes.Select(f => RelaxAsync(f, relaxFilterFunc, restrictFilterFunc));
                    var innerFilterNodes = await Task.WhenAll(innerFilterNodeTasks).ConfigureAwait(false);
                    return innerFilterNodes.Combine(combinationFilterNode.Operator);
                },
                async invertedFilterNode => (await RestrictAsync(invertedFilterNode.NodeToInvert, restrictFilterFunc, relaxFilterFunc).ConfigureAwait(false)).Invert(),
                leafFilterNode => relaxFilterFunc(leafFilterNode.Filter))
                .ConfigureAwait(false);

            return result.Collapse();
        }
    }
}