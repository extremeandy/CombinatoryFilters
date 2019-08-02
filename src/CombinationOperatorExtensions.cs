using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationOperatorExtensions
    {
        public static TResult Match<TResult>(
            this CombinationOperator @operator,
            Func<TResult> allPredicate,
            Func<TResult> anyPredicate)
        {
            return @operator == CombinationOperator.All
                ? allPredicate()
                : anyPredicate();
        }
    }
}