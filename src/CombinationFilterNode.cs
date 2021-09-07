using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class CombinationFilterNode<TFilter> : FilterNode<TFilter>, ICombinationFilterNode<TFilter>
        where TFilter : IFilter
    {
        /// <summary>
        /// If <see langword="null"/>, has not been sorted.
        /// </summary>
        private readonly IComparer<IFilterNode<TFilter>> _comparer;
        internal readonly IFilterNode<TFilter>[] Nodes;
        private readonly SimpleLazy<HashSet<IFilterNode>> _nodesSet;

        public CombinationFilterNode(IEnumerable<TFilter> filters, CombinationOperator @operator = default)
            : this(filters.Select(f => f.ToLeafFilterNode()), @operator, isCollapsed: false, comparer: null)
        {
        }

        public CombinationFilterNode(IEnumerable<IFilterNode<TFilter>> nodes, CombinationOperator @operator = default)
            : this(nodes, @operator, isCollapsed: false, comparer: null)
        {
        }

        internal CombinationFilterNode(IEnumerable<IFilterNode<TFilter>> nodes, CombinationOperator @operator, bool isCollapsed, IComparer<IFilterNode<TFilter>> comparer)
            : this(nodes.ToArray(), @operator, isCollapsed, comparer)
        {
        }

        internal CombinationFilterNode(IFilterNode<TFilter>[] nodes, CombinationOperator @operator, bool isCollapsed, IComparer<IFilterNode<TFilter>> comparer)
        {
            IsCollapsed = isCollapsed;
            Nodes = nodes;
            Operator = @operator;
            _nodesSet = new SimpleLazy<HashSet<IFilterNode>>(() => new HashSet<IFilterNode>(nodes));
            _comparer = comparer;
        }

        public override bool IsCollapsed { get; }

        public CombinationOperator Operator { get; }

        IReadOnlyList<IFilterNode<TFilter>> ICombinationFilterNode<TFilter>.Nodes => Nodes;
        IReadOnlyList<IFilterNode> ICombinationFilterNode.Nodes => Nodes;

        public override TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
        {
            var mappedInnerNodes = new TResult[Nodes.Length];
            for (var i = 0; i < Nodes.Length; i++)
            {
                mappedInnerNodes[i] = Nodes[i].Aggregate(combine, invert, transform);
            }

            return combine(mappedInnerNodes, Operator);
        }

        public override TResult Match<TResult>(
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
            => combine(this);

        public override IFilterNode<TResultFilter> Map<TResultFilter>(Func<TFilter, TResultFilter> mapFunc)
            => Nodes.Select(f => f.Map(mapFunc)).Combine(Operator);

        public override IFilterNode<TResultFilter> Bind<TResultFilter>(Func<TFilter, IFilterNode<TResultFilter>> bindFunc)
            => Nodes.Select(f => f.Bind(bindFunc)).Combine(Operator);

        public override IFilterNode<TFilter> Sort(IComparer<IFilterNode<TFilter>> comparer)
        {
            // Only sort if absolutely necessary. We still need to sort inner nodes even if there is only 1.
            if (_comparer != null && _comparer.Equals(comparer) || Nodes.Length == 0)
            {
                return this;
            }

            var sortedNodes = Nodes
                .Select(n => n.Sort(comparer))
                .OrderBy(n => n, comparer);

            return new CombinationFilterNode<TFilter>(
                sortedNodes,
                Operator,
                IsCollapsed,
                comparer);
        }

        public override IFilterNode<TFilter> Collapse()
        {
            if (IsCollapsed)
            {
                return this;
            }

            IEnumerable<IFilterNode<TFilter>> Flatten(IFilterNode<TFilter> innerNode)
            {
                if (innerNode is CombinationFilterNode<TFilter> combinationFilter &&
                    combinationFilter.Operator == Operator)
                {
                    return combinationFilter.Nodes;
                }

                return new[]
                {
                    innerNode
                };
            }

            IEnumerable<IFilterNode<TFilter>> Absorb(IEnumerable<IFilterNode<TFilter>> innerNodes)
            {
                var innerNodesList = innerNodes.ToList();
                var innerCombinationNodesOfOppositeOperationToRetain = new HashSet<CombinationFilterNode<TFilter>>(
                    innerNodesList
                        .OfType<CombinationFilterNode<TFilter>>()
                        .Where(f => f.Operator != Operator));

                var otherInnerNodes = innerNodesList.Except(innerCombinationNodesOfOppositeOperationToRetain)
                    .ToList();

                foreach (var filter in otherInnerNodes)
                {
                    var combinationFilterNodesToRemove = new List<CombinationFilterNode<TFilter>>();
                    foreach (var combinationFilterNode in innerCombinationNodesOfOppositeOperationToRetain)
                    {
                        // If the inner filters contains one of the outer filters, that means the inner filter is redundant,
                        // so we can remove it, i.e. it is "absorbed" by the outer filter.
                        if (combinationFilterNode._nodesSet.Value.Contains(filter))
                        {
                            combinationFilterNodesToRemove.Add(combinationFilterNode);
                        }
                    }

                    foreach (var combinationFilterNode in combinationFilterNodesToRemove)
                    {
                        innerCombinationNodesOfOppositeOperationToRetain.Remove(combinationFilterNode);
                    }
                }

                // Select from the original list so order may be preserved, slightly more expensive operation
                return innerNodesList.SelectMany(innerNode =>
                {
                    if (innerNode is CombinationFilterNode<TFilter> combinationFilter
                        && combinationFilter.Operator != Operator
                        && !innerCombinationNodesOfOppositeOperationToRetain.Contains(combinationFilter))
                    {
                        return Array.Empty<IFilterNode<TFilter>>();
                    }

                    return new[]
                    {
                        innerNode
                    };
                });
            }

            return Operator.Match(
                () =>
                {
                    var collapsedInnerNodes = Nodes
                        .Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerNodes.Any(f => f.IsFalse()))
                    {
                        return False;
                    }

                    // Identity
                    var nonTrivialNodes = collapsedInnerNodes.Where(f => !f.IsTrue());

                    // Associativity
                    var flattenedNonTrivialNodes = nonTrivialNodes.SelectMany(Flatten);

                    // Absorption
                    var absorbedNodes = Absorb(flattenedNonTrivialNodes);

                    // Must preserve the order
                    var uniqueNodes = absorbedNodes.Distinct();

                    var collapsedCombinationNode = new CombinationFilterNode<TFilter>(uniqueNodes, Operator, isCollapsed: true, comparer: _comparer);
                    return collapsedCombinationNode.Nodes.Length == 1
                        ? collapsedCombinationNode.Nodes[0]
                        : collapsedCombinationNode;
                },
                () =>
                {
                    var collapsedInnerNodes = Nodes
                        .Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerNodes.Any(f => f.IsTrue()))
                    {
                        return True;
                    }

                    // Identity
                    var nonTrivialNodes = collapsedInnerNodes.Where(f => !f.IsFalse());

                    // Associativity
                    var flattenedNonTrivialNodes = nonTrivialNodes.SelectMany(Flatten);

                    // Absorption
                    var absorbedNodes = Absorb(flattenedNonTrivialNodes);

                    // Must preserve the order
                    var uniqueNodes = absorbedNodes.Distinct();

                    var collapsedCombinationNode = new CombinationFilterNode<TFilter>(uniqueNodes, Operator, isCollapsed: true, comparer: _comparer);
                    return collapsedCombinationNode.Nodes.Length == 1
                        ? collapsedCombinationNode.Nodes[0]
                        : collapsedCombinationNode;
                });
        }

        public override bool Any(Func<TFilter, bool> predicate)
            => Nodes.Any(f => f.Any(predicate));

        public override bool All(Func<TFilter, bool> predicate)
            => Nodes.All(f => f.All(predicate));

        protected override bool IsEquivalentToInternal(IFilterNode other)
            => other is ICombinationFilterNode combinationOther
               && Operator == combinationOther.Operator
               && _nodesSet.Value.SetEquals(combinationOther.Nodes);

        protected override bool IsTrueInternal()
            => Operator.Match(
                () => Nodes.All(f => f.IsTrue()),
                () => Nodes.Any(f => f.IsTrue()));

        protected override bool IsFalseInternal()
            => Operator.Match(
                () => Nodes.Any(f => f.IsFalse()),
                () => Nodes.All(f => f.IsFalse()));

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!(other is ICombinationFilterNode combinationOther)
                || Operator != combinationOther.Operator)
            {
                return false;
            }

            return Nodes.SequenceEqual(combinationOther.Nodes);
        }

        protected override int GetHashCodeInternal()
        {
            unchecked
            {
                var hashCode = 0;
                for (var i = 0; i < Nodes.Length; i++)
                {
                    hashCode = (hashCode * 397) ^ Nodes[i].GetHashCode();
                }

                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            if (Nodes.Length == 0)
            {
                return Operator.Match("TRUE", "FALSE");
            }

            var delimeter = Operator.Match(" AND ", " OR ");
            return string.Join(delimeter, Nodes.Select(f => $"({f})"));
        }
    }
}