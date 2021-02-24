using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Relaxes' a filter by relaxing each leaf of the filter according to <see cref="relaxedItemFilterFunc"/>, or restricting
        /// inversions according to <see cref="restrictItemFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="RestrictAsync{T}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="relaxedItemFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <param name="restrictItemFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <returns></returns>
        public static async Task<IFilterNode<TLeafNode>> RelaxAsync<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Task<IFilterNode<TLeafNode>>> relaxedItemFilterFunc,
            Func<TLeafNode, Task<IFilterNode<TLeafNode>>> restrictItemFilterFunc)
            where TLeafNode : class, ILeafFilterNode
            => (await filter.Match<Task<IFilterNode<TLeafNode>>>(
                async combinationFilter =>
                {
                    var innerFilterTasks = combinationFilter.Filters.Select(f => RelaxAsync(f, relaxedItemFilterFunc, restrictItemFilterFunc));
                    var innerFilters = await Task.WhenAll(innerFilterTasks);
                    return new CombinationFilter<TLeafNode>(innerFilters, combinationFilter.Operator);
                },
                async invertedFilter => new InvertedFilter<TLeafNode>(await RestrictAsync(invertedFilter.FilterToInvert, restrictItemFilterFunc, relaxedItemFilterFunc)),
                relaxedItemFilterFunc))
                .Collapse();
    }
}