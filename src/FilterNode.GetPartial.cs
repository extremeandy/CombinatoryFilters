using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
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
    }
}