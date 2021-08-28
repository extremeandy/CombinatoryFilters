using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        /// <summary>
        /// Removes non-matching nodes and replaces them with <see cref="FilterNode{TFilter}.True"/>
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="filter"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TFilter> Where<TFilter>(
            this IFilterNode<TFilter> filter,
            Func<TFilter, bool> predicate)
            where TFilter : IFilter
            => filter.Bind<TFilter>(leafFilter =>
            {
                if (predicate(leafFilter))
                {
                    return leafFilter.ToLeafFilterNode();
                }

                return FilterNode<TFilter>.True;
            });
    }
}