using System;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// 'Restricts' a filter by restricting each leaf of the filter according to <see cref="restrictItemFilterFunc"/>, or restricting
        /// inversions according to <see cref="relaxedItemFilterFunc"/>.
        ///
        /// This is the inverse operation of <see cref="RelaxAsync{T}"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="restrictItemFilterFunc">
        /// A function which takes a leaf node and restricts it (i.e. the inverse operation of relax).
        /// </param>
        /// 
        /// <param name="relaxedItemFilterFunc">
        /// A function which takes a leaf node and relaxes it
        /// </param>
        /// <returns></returns>
        public static Task<IFilterNode<TLeafNode>> RestrictAsync<TLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Task<IFilterNode<TLeafNode>>> restrictItemFilterFunc,
            Func<TLeafNode, Task<IFilterNode<TLeafNode>>> relaxedItemFilterFunc)
            where TLeafNode : class, ILeafFilterNode
            => RelaxAsync(filter, restrictItemFilterFunc, relaxedItemFilterFunc);
    }
}