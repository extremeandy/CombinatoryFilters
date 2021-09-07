using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// Combines the supplied <see cref="IFilterNode" />s into a <see cref="ICombinationFilterNode"/>,
        /// or returns the node if there is only a single one.
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        public static IFilterNode<TFilter> Combine<TFilter>(
            this IEnumerable<IFilterNode<TFilter>> filterNodes,
            CombinationOperator @operator = default)
            where TFilter : IFilter
        {
            var filterNodesArray = filterNodes.ToArray();

            // Use the internal constructor for CombinationFilterNode to save calling ToArray again on filterNodes,
            // which is done in the public constructors.
            return filterNodesArray.Length == 1
                ? filterNodesArray[0]
                : new CombinationFilterNode<TFilter>(filterNodesArray, @operator, isCollapsed: false, comparer: null);
        }
    }
}