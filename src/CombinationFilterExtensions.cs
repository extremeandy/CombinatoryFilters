using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationFilterExtensions
    {
        public static TResult Match<TFilter, TResult>(
            this ICombinationFilterNode<TFilter> source,
            Func<IEnumerable<IFilterNode<TFilter>>, TResult> allReducer,
            Func<IEnumerable<IFilterNode<TFilter>>, TResult> anyReducer)
            => source.Nodes
                .Match(allReducer, anyReducer, source.Operator);

        public static TResult Match<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, TResult> allReducer,
            Func<IEnumerable<TSource>, TResult> anyReducer,
            CombinationOperator @operator = default)
            => @operator.Match(
                () => allReducer(source),
                () => anyReducer(source));
    }
}