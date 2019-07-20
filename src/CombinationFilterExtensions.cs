using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationFilterExtensions
    {
        public static TResult Match<TResult, TLeafNode>(
            this ICombinationFilterNode<TLeafNode> source,
            Func<IEnumerable<IFilterNode<TLeafNode>>, TResult> andReducer,
            Func<IEnumerable<IFilterNode<TLeafNode>>, TResult> orReducer)
            where TLeafNode : class, ILeafFilterNode
        {
            return source.Filters
                .Match(andReducer, orReducer, source.Operator);
        }

        public static TResult Match<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, TResult> andReducer,
            Func<IEnumerable<TSource>, TResult> orReducer,
            CombinationOperator @operator = default)
        {
            return @operator.Match(
                () => andReducer(source), 
                () => orReducer(source));
        }
    }
}