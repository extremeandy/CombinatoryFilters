using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationOperatorExtensions
    {
        public static TResult Match<TResult>(
            this CombinationOperator @operator,
            Func<TResult> allPredicate,
            Func<TResult> anyPredicate)
            => @operator == CombinationOperator.All
                ? allPredicate()
                : anyPredicate();

        public static TResult Match<TResult>(
            this CombinationOperator @operator,
            TResult allResult,
            TResult anyResult)
            => @operator == CombinationOperator.All
                ? allResult
                : anyResult;
    }
}