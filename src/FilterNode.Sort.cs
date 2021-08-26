using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// Sort the nodes according to the default <see cref="FilterNodeComparer{TFilter}"/>.
        /// By default, the order is leaf, then inverted, then combination. Leaf nodes within combinations
        /// will be sorted using the default <see cref="IComparer{T}"/> for TFilter.
        /// </summary>
        public static IFilterNode<TFilter> Sort<TFilter>(this IFilterNode<TFilter> filter)
            where TFilter : IFilter
            => filter.Sort(FilterNodeComparer<TFilter>.Default);

        /// <summary>
        /// Sort the nodes according to <see cref="FilterNodeComparer{TFilter}"/> using the supplied filter comparer.
        /// By default, the order is leaf, then inverted, then combination. Leaf nodes within combinations
        /// will be sorted using the supplied <see cref="IComparer{T}"/> for TFilter.
        /// </summary>
        public static IFilterNode<TFilter> Sort<TFilter>(this IFilterNode<TFilter> filter, IComparer<TFilter> filterComparer)
            where TFilter : IFilter
            => filter.Sort(new FilterNodeComparer<TFilter>(filterComparer));
    }
}