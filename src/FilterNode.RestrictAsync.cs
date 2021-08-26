using System;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Restricts' a filter by restricting each leaf of the filter according to <see cref="restrictFilterFunc"/>, or restricting
        /// inversions according to <see cref="relaxFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="RelaxAsync{T}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="restrictFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// 
        /// <param name="relaxFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <returns></returns>
        public static Task<IFilterNode<TFilter>> RestrictAsync<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, Task<IFilterNode<TFilter>>> restrictFilterFunc,
            Func<TFilter, Task<IFilterNode<TFilter>>> relaxFilterFunc)
            where TFilter : IFilter
            => RelaxAsync(filter, restrictFilterFunc, relaxFilterFunc);
    }
}