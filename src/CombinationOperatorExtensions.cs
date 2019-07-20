using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class CombinationOperatorExtensions
    {
        public static TResult Match<TResult>(
            this CombinationOperator @operator,
            Func<TResult> andPredicate,
            Func<TResult> orPredicate)
        {
            return @operator == CombinationOperator.And
                ? andPredicate()
                : orPredicate();
        }
    }
}