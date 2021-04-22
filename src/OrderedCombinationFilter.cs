using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Ordered collection of <see cref="IFilterNode{TLeafNode}"/> to be combined according to
    /// then given <see cref="CombinationOperator"/>
    /// </summary>
    /// <typeparam name="TLeafNode"></typeparam>
    public class OrderedCombinationFilter<TLeafNode> : CombinationFilterBase<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        public OrderedCombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters.ToList(), @operator)
        {
        }

        public OrderedCombinationFilter(IReadOnlyList<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters, @operator, isCollapsed: false)
        {
        }

        internal OrderedCombinationFilter(IReadOnlyList<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : base(filters, @operator, isCollapsed)
        {
        }

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is OrderedCombinationFilter<TLeafNode> orderedCombinationOther && other.GetType() == typeof(OrderedCombinationFilter<TLeafNode>))
            {
                return Filters.SequenceEqual(orderedCombinationOther.Filters)
                       && Operator == orderedCombinationOther.Operator;
            }

            if (other is CombinationFilter<TLeafNode> concreteCombinationOther && other.GetType() == typeof(CombinationFilter<TLeafNode>))
            {
                return concreteCombinationOther.FiltersSet.SetEquals(Filters)
                       && Operator == concreteCombinationOther.Operator;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Filters.Aggregate(0, (acc, f) => (acc * 397) ^ f.GetHashCode());
                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                return hashCode;
            }
        }
    }
}