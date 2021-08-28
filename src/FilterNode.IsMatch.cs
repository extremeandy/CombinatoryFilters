using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        [Obsolete("This method is comparatively slow and causes an allocation for each call. Use `GetPredicate` instead and save the result when testing many items.")]
        public static bool IsMatch<TFilter, TItemToTest>(
            this IFilterNode<TFilter> filter,
            TItemToTest item)
            where TFilter : IFilter<TItemToTest>
        {
            var predicate = filter.GetPredicate<TFilter, TItemToTest>();
            return predicate(item);
        }
    }
}