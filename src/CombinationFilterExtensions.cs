using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationFilterExtensions
    {
        public static TResult Match<TResult, TLeafNode>(
            this ICombinationFilter<TLeafNode> source,
            Func<IEnumerable<IFilterNode<TLeafNode>>, TResult> allReducer,
            Func<IEnumerable<IFilterNode<TLeafNode>>, TResult> anyReducer)
            where TLeafNode : class, ILeafFilterNode
        {
            return source.Filters
                .Match(allReducer, anyReducer, source.Operator);
        }

        public static TResult Match<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, TResult> allReducer,
            Func<IEnumerable<TSource>, TResult> anyReducer,
            CombinationOperator @operator = default)
        {
            return @operator.Match(
                () => allReducer(source), 
                () => anyReducer(source));
        }
    }
}