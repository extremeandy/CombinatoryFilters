using System;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Relaxes' a filter by relaxing each leaf of the filter according to <see cref="relaxFilterFunc"/>, or restricting
        /// inversions according to <see cref="restrictFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="Restrict{TFilter}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="relaxFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <param name="restrictFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <returns></returns>
        public static IFilterNode<TFilter> Relax<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, IFilterNode<TFilter>> relaxFilterFunc,
            Func<TFilter, IFilterNode<TFilter>> restrictFilterFunc)
            where TFilter : IFilter
            => filter.Match(
                    combinationFilterNode =>
                    {
                        var innerNodes = combinationFilterNode.Nodes.Select(f => Relax(f, relaxFilterFunc, restrictFilterFunc));
                        return innerNodes.Combine(combinationFilterNode.Operator);
                    },
                    invertedFilterNode => Restrict(invertedFilterNode.NodeToInvert, restrictFilterFunc, relaxFilterFunc).Invert(),
                    leafFilterNode => relaxFilterFunc(leafFilterNode.Filter))
                .Collapse();
    }
}