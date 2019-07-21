using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class RealisableLeafFilterNodeExtensions
    {
        public static bool IsMatch<TLeafNode, TItemToTest>(
            this IFilterNode<TLeafNode> filter,
            TItemToTest item)
            where TLeafNode : class, ILeafFilterNode, IRealisableLeafFilterNode<TItemToTest>
        {
            var predicate = filter.GetPredicate<TLeafNode, TItemToTest>();
            return predicate(item);
        }

        public static Func<TItemToTest, bool> GetPredicate<TLeafNode, TItemToTest>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode, IRealisableLeafFilterNode<TItemToTest>
        {
            return filter.GetPredicate<TLeafNode, TItemToTest>(leafFilter => leafFilter.IsMatch);
        }
    }
}