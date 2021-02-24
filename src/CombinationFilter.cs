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
        private readonly HashSet<IFilterNode<TLeafNode>> _filters;

        public CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default)
            : this(filters, @operator, isCollapsed: false)
        {
        }

        internal CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : this(new HashSet<IFilterNode<TLeafNode>>(filters), @operator, isCollapsed)
        {
        }

        private CombinationFilter(HashSet<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : base(filters, @operator, isCollapsed)
        {
            _filters = filters;
        }

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is ICombinationFilterNode<TLeafNode> typedCombinationOther)
            {
                return _filters.SetEquals(typedCombinationOther.Filters)
                       && Operator == typedCombinationOther.Operator;
            }

            return other is ICombinationFilterNode combinationOther
                   && _filters.Count == 0 && combinationOther.Filters.Count == 0
                   && Operator == combinationOther.Operator;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var filterHashCodes = Filters
                    .Select(x => x.GetHashCode())
                    .OrderBy(x => x);

                var hashCode = filterHashCodes.Aggregate(0, (acc, h) => (acc * 397) ^ h);
                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                return hashCode;
            }
        }
    }
}