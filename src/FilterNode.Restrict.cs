using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Restricts' a filter by restricting each leaf of the filter according to <see cref="restrictFilterFunc"/>, or restricting
        /// inversions according to <see cref="relaxFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="Relax{TFilter}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="restrictFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <param name="relaxFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <returns></returns>
        public static IFilterNode<TFilter> Restrict<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, IFilterNode<TFilter>> restrictFilterFunc,
            Func<TFilter, IFilterNode<TFilter>> relaxFilterFunc)
            where TFilter : IFilter
            => Relax(filter, restrictFilterFunc, relaxFilterFunc);
    }
}