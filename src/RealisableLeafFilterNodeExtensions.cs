using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class RealisableLeafFilterNodeExtensions
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

        public static Func<TItemToTest, bool> GetPredicate<TFilter, TItemToTest>(this IFilterNode<TFilter> filter)
            where TFilter : IFilter<TItemToTest>
        {
            return filter.GetPredicate<TFilter, TItemToTest>(leafFilter => leafFilter.IsMatch);
        }
    }
}