using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Unordered set of <see cref="IFilterNode{TLeafNode}"/> to be combined according to
    /// then given <see cref="CombinationOperator"/>
    /// </summary>
    /// <typeparam name="TLeafNode"></typeparam>
    public class CombinationFilter<TLeafNode> : CombinationFilterBase<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        private readonly Lazy<int> _hashCode;

        public CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters, @operator, isCollapsed: false)
        {
        }

        internal CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : this(new HashSet<IFilterNode<TLeafNode>>(filters), @operator, isCollapsed)
        {
        }

        public readonly HashSet<IFilterNode<TLeafNode>> FiltersSet;

        private CombinationFilter(HashSet<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : base(filters, @operator, isCollapsed)
        {
            FiltersSet = filters;
            _hashCode = new Lazy<int>(() =>
            {
                var filterHashCodes = filters
                    .Select(x => x.GetHashCode())
                    .OrderBy(x => x);

                var hashCode = filterHashCodes.Aggregate(0, (acc, h) => (acc * 397) ^ h);
                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                return hashCode;
            });
        }

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is CombinationFilter<TLeafNode> concreteCombinationOther && other.GetType() == typeof(CombinationFilter<TLeafNode>))
            {
                return FiltersSet.SetEquals(concreteCombinationOther.FiltersSet)
                       && Operator == concreteCombinationOther.Operator;
            }

            if (other is OrderedCombinationFilter<TLeafNode> orderedCombinationOther &&
                other.GetType() == typeof(OrderedCombinationFilter<TLeafNode>))
            {
                return FiltersSet.SequenceEqual(orderedCombinationOther.Filters)
                       && Operator == orderedCombinationOther.Operator;
            }

            return false;
        }

        public override int GetHashCode() => _hashCode.Value;
    }
}