using System;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Relaxes' a filter by relaxing each leaf of the filter according to <see cref="relaxedItemFilterFunc"/>, or restricting
        /// inversions according to <see cref="restrictItemFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="Restrict{TLeafNode}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="relaxedItemFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <param name="restrictItemFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Relax<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, IFilterNode<TLeafNode>> relaxedItemFilterFunc,
            Func<TLeafNode, IFilterNode<TLeafNode>> restrictItemFilterFunc)
            where TLeafNode : class, ILeafFilterNode
            => filter.Match(
                combinationFilter =>
                {
                    var innerFilters = combinationFilter.Filters.Select(f => Relax(f, relaxedItemFilterFunc, restrictItemFilterFunc));
                    return new CombinationFilter<TLeafNode>(innerFilters, combinationFilter.Operator);
                },
                invertedFilter => new InvertedFilter<TLeafNode>(Restrict(invertedFilter.FilterToInvert, restrictItemFilterFunc, relaxedItemFilterFunc)),
                relaxedItemFilterFunc);
    }
}