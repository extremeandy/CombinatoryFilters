using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Restricts' a filter by restricting each leaf of the filter according to <see cref="restrictItemFilterFunc"/>, or restricting
        /// inversions according to <see cref="relaxedItemFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="Relax{TLeafNode}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="restrictItemFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// <param name="relaxedItemFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Restrict<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, IFilterNode<TLeafNode>> restrictItemFilterFunc,
            Func<TLeafNode, IFilterNode<TLeafNode>> relaxedItemFilterFunc)
            where TLeafNode : class, ILeafFilterNode
            => Relax(filter, restrictItemFilterFunc, relaxedItemFilterFunc);
    }
}