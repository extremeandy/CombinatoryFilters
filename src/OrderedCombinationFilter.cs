using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Ordered collection of <see cref="IFilterNode{TLeafNode}"/> to be combined according to
    /// then given <see cref="CombinationOperator"/>
    /// </summary>
    /// <typeparam name="TLeafNode"></typeparam>
    public class OrderedCombinationFilter<TLeafNode> : OrderedCombinationFilter, ICombinationFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        public OrderedCombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters.ToList(), @operator)
        {
        }

        public OrderedCombinationFilter(IReadOnlyList<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
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

        public TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            return combine(this);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc) where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            var innerFilters = Filters.Select(f => f.Map(mapFunc));
            return new OrderedCombinationFilter<TResultLeafNode>(innerFilters, Operator);
        }
    }

    public abstract class OrderedCombinationFilter : FilterNode, ICombinationFilterNode
    {
        protected OrderedCombinationFilter(IEnumerable<IFilterNode> filters, CombinationOperator @operator = default)
            : this(filters.ToList(), @operator)
        {
        }

        protected OrderedCombinationFilter(IReadOnlyList<IFilterNode> filters, CombinationOperator @operator = default)
        {
            Filters = filters;
            Operator = @operator;
        }

        public IReadOnlyCollection<IFilterNode> Filters { get; }

        public CombinationOperator Operator { get; }

        public bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is ICombinationFilterNode combinationOther
                   && Filters.SequenceEqual(combinationOther.Filters)
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

        public override string ToString()
        {
            var delimiter = Operator.Match(() => " AND ", () => " OR ");
            return string.Join(delimiter, Filters.Select(f => $"({f})"));
        }
    }
}