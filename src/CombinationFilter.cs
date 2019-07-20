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
    public class CombinationFilter<TLeafNode> : CombinationFilter, ICombinationFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        public CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters.ToImmutableHashSet(), @operator)
        {
        }

        public CombinationFilter(IImmutableSet<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : base(filters, @operator)
        {
            Filters = filters;
        }

        public new IReadOnlyCollection<IFilterNode<TLeafNode>> Filters { get; }

        public TResult Match<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var innerResults = Filters.Select(f => f.Match(combine, invert, transform));
            return combine(innerResults, Operator);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(
            Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            var innerFilters = Filters.Select(f => f.Map(mapFunc));
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator);
        }
    }

    public abstract class CombinationFilter : InternalFilterNode, ICombinationFilterNode
    {
        private readonly IImmutableSet<IFilterNode> _filters;

        protected CombinationFilter(IEnumerable<IFilterNode> filters, CombinationOperator @operator = default)
            : this(filters.ToImmutableHashSet(), @operator)
        {
        }

        protected CombinationFilter(IImmutableSet<IFilterNode> filters, CombinationOperator @operator = default)
        {
            _filters = filters;
            Operator = @operator;
        }

        public IReadOnlyCollection<IFilterNode> Filters => _filters;

        public CombinationOperator Operator { get; }

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is ICombinationFilterNode combinationOther
                   && _filters.SetEquals(combinationOther.Filters)
                   && Operator == combinationOther.Operator;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var filterHashCodes = Filters
                    .Select(x => x.GetHashCode())
                    .OrderBy(x => x);

                unchecked
                {
                    var hashCode = filterHashCodes.Aggregate(0, (acc, h) => (acc * 397) ^ h);
                    hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}