using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class RealisableLeafFilterNodeExtensions
    {
        /// <summary>
        /// Use for testing a single item against the filter. For testing multiple items,
        /// use <see cref="GetPredicate{TItemToTest,TLeafNode}"/> to save computing
        /// the predicate for reach call to <see cref="IsMatch{TItemToTest,TLeafNode}"/>.
        /// </summary>
        /// <typeparam name="TItemToTest">Type of the item to filter</typeparam>
        /// <typeparam name="TLeafNode">Type of the leaf filter node</typeparam>
        /// <param name="filter"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>Use for testing a single item.</remarks>
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