using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Unordered set of <see cref="IFilterNode{TLeafNode}"/> to be combined according to
    /// then given <see cref="CombinationOperator"/>
    /// </summary>
    /// <typeparam name="TLeafNode"></typeparam>
    public abstract class CombinationFilterBase<TLeafNode> : CombinationFilterBase, ICombinationFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        protected CombinationFilterBase(IReadOnlyCollection<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : base(filters, @operator)
        {
            Filters = filters;
        }

        public new IReadOnlyCollection<IFilterNode<TLeafNode>> Filters { get; }

        public TResult Aggregate<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var innerFilters = Filters.Select(f => f.Aggregate(combine, invert, transform));
            return combine(innerFilters, Operator);
        }

        public TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            return combine(this);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(
            Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            var innerFilters = Filters.Select(f => f.Map(mapFunc));
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator);
        }

        public IFilterNode<TLeafNode> Collapse()
        {
            return Operator.Match(
                () =>
                {
                    var collapsedInnerFilters = Filters.Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerFilters.Any(f => f.Equals(FilterNode<TLeafNode>.False)))
                    {
                        return FilterNode<TLeafNode>.False;
                    }

                    var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.Equals(FilterNode<TLeafNode>.True));
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator);
                    return collapsedCombinationFilter.Filters.Count == 1
                        ? collapsedCombinationFilter.Filters.Single()
                        : collapsedCombinationFilter;
                },
                () =>
                {
                    var collapsedInnerFilters = Filters.Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerFilters.Any(f => f.Equals(FilterNode<TLeafNode>.True)))
                    {
                        return FilterNode<TLeafNode>.True;
                    }

                    var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.Equals(FilterNode<TLeafNode>.False));
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator);
                    return collapsedCombinationFilter.Filters.Count == 1
                        ? collapsedCombinationFilter.Filters.Single()
                        : collapsedCombinationFilter;
                });
        }

        public bool Any(Func<TLeafNode, bool> predicate)
        {
            return Filters.Any(f => f.Any(predicate));
        }
    }

    public abstract class CombinationFilterBase : InternalFilterNode, ICombinationFilterNode
    {
        protected CombinationFilterBase(IEnumerable<IFilterNode> filters, CombinationOperator @operator = default)
            : this(filters.ToImmutableHashSet(), @operator)
        {
        }

        protected CombinationFilterBase(IReadOnlyCollection<IFilterNode> filters, CombinationOperator @operator = default)
        {
            Filters = filters;
            Operator = @operator;
        }

        public IReadOnlyCollection<IFilterNode> Filters { get; }

        public CombinationOperator Operator { get; }

        public override string ToString()
        {
            if (Filters.Count == 0)
            {
                return Operator.Match(() => "TRUE", () => "FALSE");
            }

            var delimeter = Operator.Match(() => " AND ", () => " OR ");
            return string.Join(delimeter, Filters.Select(f => $"({f})"));
        }
    }
}