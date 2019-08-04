using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class FilterNodeExtensions
    {
        public static InvertedFilter<TLeafNode> Invert<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode
        {
            return new InvertedFilter<TLeafNode>(filter);
        }

        // TODO: Maybe this can be made private/internal?
        public static Func<TItemToTest, bool> GetPredicate<TLeafNode, TItemToTest>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Func<TItemToTest, bool>> itemPredicate)
            where TLeafNode : class, ILeafFilterNode
        {
            return filter.Aggregate(
                Combine,
                Invert,
                itemPredicate);
        }

        /// <summary>
        /// Removes redundancies in a filter to give simplest expression
        /// </summary>
        /// <typeparam name="TLeafNode"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Collapse<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode
        {
            return filter.Match(
                combinationFilter =>
                {
                    return combinationFilter.Operator.Match(
                        () =>
                        {
                            var collapsedInnerFilters = combinationFilter.Filters.Select(f => f.Collapse())
                                .ToList();

                            if (collapsedInnerFilters.Any(f => f.Equals(FilterNode<TLeafNode>.False)))
                            {
                                return FilterNode<TLeafNode>.False;
                            }

                            var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.Equals(FilterNode<TLeafNode>.True));
                            var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, combinationFilter.Operator);
                            return collapsedCombinationFilter.Filters.Count == 1
                                ? collapsedCombinationFilter.Filters.Single()
                                : collapsedCombinationFilter;
                        },
                        () =>
                        {
                            var collapsedInnerFilters = combinationFilter.Filters.Select(f => f.Collapse())
                                .ToList();

                            if (collapsedInnerFilters.Any(f => f.Equals(FilterNode<TLeafNode>.True)))
                            {
                                return FilterNode<TLeafNode>.True;
                            }

                            var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.Equals(FilterNode<TLeafNode>.False));
                            var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, combinationFilter.Operator);
                            return collapsedCombinationFilter.Filters.Count == 1
                                ? collapsedCombinationFilter.Filters.Single()
                                : collapsedCombinationFilter;
                        });
                },
                invertedFilter =>
                {
                    var collapsedInnerFilter = invertedFilter.FilterToInvert.Collapse();
                    // If we have NOT(TRUE) then return FALSE or if we have NOT(FALSE) return TRUE.
                    if (collapsedInnerFilter is ICombinationFilterNode<TLeafNode> combinationInner && combinationInner.Filters.Count == 0)
                    {
                        return combinationInner.Operator.Match(() => FilterNode<TLeafNode>.False, () => FilterNode<TLeafNode>.True);
                    }

                    // If we have NOT(NOT(f)) just return f
                    if (collapsedInnerFilter is IInvertedFilter<TLeafNode> invertedInner)
                    {
                        return invertedInner.FilterToInvert;
                    }

                    return new InvertedFilter<TLeafNode>(collapsedInnerFilter);
                },
                leafFilter => (IFilterNode<TLeafNode>)leafFilter); // TODO: Figure out a way to remove this evil cast?
        }

        /// <summary>
        /// Computes the filter that includes only leaf filters that satisfy the given predicate.
        /// The result is guaranteed to be less than or equal in restrictiveness to the original.
        /// This means that it will never filter out results that the original filter would not have
        /// filtered.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> GetPartial<TLeafNode>(this IFilterNode<TLeafNode> filter, Func<TLeafNode, bool> predicate)
            where TLeafNode : class, ILeafFilterNode
        {
            (IFilterNode<TLeafNode> Result, IFilterNode<TLeafNode> ResultToInvert) GetPartialTuple(IFilterNode<TLeafNode> inner)
            {
                return inner.Match(
                    combinationFilter =>
                    {
                        var innerPartialTuples = combinationFilter.Filters.Select(GetPartialTuple)
                            .ToList();

                        return (
                            Result: new CombinationFilter<TLeafNode>(innerPartialTuples.Select(tuple => tuple.Result), combinationFilter.Operator),
                            ResultToInvert: new CombinationFilter<TLeafNode>(innerPartialTuples.Select(tuple => tuple.ResultToInvert), combinationFilter.Operator));
                    },
                    invertedFilter =>
                    {
                        var partialTuple = GetPartialTuple(invertedFilter.FilterToInvert);
                        return (
                            Result: new InvertedFilter<TLeafNode>(partialTuple.ResultToInvert),
                            ResultToInvert: new InvertedFilter<TLeafNode>(partialTuple.Result));
                    },
                    leafFilter =>
                    {
                        if (predicate(leafFilter))
                        {
                            return (Result: (IFilterNode<TLeafNode>)leafFilter, ResultToInvert: (IFilterNode<TLeafNode>)leafFilter);
                        }

                        // If we are here, it means we're going to remove the leaf filter. For a result that won't be inverted,
                        // return true - the least restrictive. For a result that will be inverted, return false - the most restrictive.
                        // The final result can not be more restrictive than the original.
                        return (Result: FilterNode<TLeafNode>.True, ResultToInvert: FilterNode<TLeafNode>.False);
                    });
            }

            return GetPartialTuple(filter)
                .Result
                .Collapse();
        }

        internal static Func<TItemToTest, bool> Combine<TItemToTest>(
            IEnumerable<Func<TItemToTest, bool>> innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AllReducer()
                => relatedItemCollection => innerResults.All(comparator => comparator(relatedItemCollection));

            Func<TItemToTest, bool> AnyReducer()
                => relatedItemCollection => innerResults.Any(comparator => comparator(relatedItemCollection));

            return @operator.Match(AllReducer, AnyReducer);
        }

        internal static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);


        public static async Task<TResult> MatchAsync<TLeafNode, TResult>(
            this IFilterNode<TLeafNode> filter,
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, Task<TResult>> transform)
            where TLeafNode : class, ILeafFilterNode
        {
            switch (filter)
            {
                case ICombinationFilterNode<TLeafNode> combinationFilter:
                    return combine(combinationFilter);
                case IInvertedFilter<TLeafNode> invertedFilter:
                    return invert(invertedFilter);
                case TLeafNode leafFilter:
                    return await transform(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }

        public static async Task<IFilterNode<TResultLeafNode>> MapAsync<TLeafNode, TResultLeafNode>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Task<TResultLeafNode>> mapFunc)
            where TLeafNode : class, ILeafFilterNode
            where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            switch (filter)
            {
                case ICombinationFilterNode<TLeafNode> combinationFilter:
                    var innerFilterTasks = combinationFilter.Filters.Select(f => f.MapAsync(mapFunc));
                    var innerFilters = await Task.WhenAll(innerFilterTasks);
                    return new CombinationFilter<TResultLeafNode>(innerFilters, combinationFilter.Operator);
                case IInvertedFilter<TLeafNode> invertedFilter:
                    var innerFilter = await invertedFilter.FilterToInvert.MapAsync(mapFunc);
                    return new InvertedFilter<TResultLeafNode>(innerFilter);
                case TLeafNode leafFilter:
                    return await mapFunc(leafFilter);
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(filter)} of type: {filter.GetType()}");
            }
        }
    }
}