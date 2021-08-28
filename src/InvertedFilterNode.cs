using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class InvertedFilterNode<TFilter> : FilterNode<TFilter>, IInvertedFilterNode<TFilter>
        where TFilter : IFilter
    {
        /// <summary>
        /// If <see langword="null"/>, has not been sorted.
        /// </summary>
        private readonly IComparer<IFilterNode<TFilter>> _comparer;

        public InvertedFilterNode(TFilter filterToInvert)
            : this(filterToInvert.ToLeafFilterNode())
        {
        }

        public InvertedFilterNode(IFilterNode<TFilter> nodeToInvert)
            : this(nodeToInvert, isCollapsed: false, comparer: null)
        {
        }

        internal InvertedFilterNode(IFilterNode<TFilter> nodeToInvert, bool isCollapsed, IComparer<IFilterNode<TFilter>> comparer)
        {
            NodeToInvert = nodeToInvert;
            IsCollapsed = isCollapsed;
            _comparer = comparer;
        }

        public override bool IsCollapsed { get; }

        public IFilterNode<TFilter> NodeToInvert { get; }
        IFilterNode IInvertedFilterNode.NodeToInvert => NodeToInvert;

        public override TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
        {
            var innerNode = NodeToInvert.Aggregate(combine, invert, transform);
            return invert(innerNode);
        }

        public override TResult Match<TResult>(
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
            => invert(this);

        public override IFilterNode<TResultFilter> Map<TResultFilter>(Func<TFilter, TResultFilter> mapFunc)
        {
            var innerNode = NodeToInvert.Map(mapFunc);
            return new InvertedFilterNode<TResultFilter>(innerNode);
        }

        public override IFilterNode<TResultFilter> Bind<TResultFilter>(Func<TFilter, IFilterNode<TResultFilter>> bindFunc)
        {
            var innerNode = NodeToInvert.Bind(bindFunc);
            return new InvertedFilterNode<TResultFilter>(innerNode);
        }

        public override IFilterNode<TFilter> Sort(IComparer<IFilterNode<TFilter>> comparer)
        {
            if (_comparer != null && _comparer.Equals(comparer))
            {
                return this;
            }

            var sortedInnerNode = NodeToInvert.Sort();
            return new InvertedFilterNode<TFilter>(sortedInnerNode, IsCollapsed, comparer);
        }

        public override IFilterNode<TFilter> Collapse()
        {
            if (IsCollapsed)
            {
                return this;
            }

            var collapsedInnerNode = NodeToInvert.Collapse();
            // If we have NOT(TRUE) then return FALSE or if we have NOT(FALSE) return TRUE.
            if (collapsedInnerNode is ICombinationFilterNode<TFilter> combinationInner)
            {
                if (combinationInner.IsTrue())
                {
                    return False;
                }

                if (combinationInner.IsFalse())
                {
                    return True;
                }
            }

            // If we have NOT(NOT(f)) just return f
            if (collapsedInnerNode is IInvertedFilterNode<TFilter> invertedInner)
            {
                return invertedInner.NodeToInvert;
            }

            return new InvertedFilterNode<TFilter>(collapsedInnerNode, isCollapsed: true, _comparer);
        }

        public override bool Any(Func<TFilter, bool> predicate)
            => NodeToInvert.Any(predicate);

        public override bool All(Func<TFilter, bool> predicate)
            => NodeToInvert.All(predicate);

        protected override bool IsEquivalentToInternal(IFilterNode other)
            => other is IInvertedFilterNode invertedOther
               && NodeToInvert.Equals(invertedOther.NodeToInvert);

        protected override bool IsTrueInternal() => NodeToInvert.IsFalse();

        protected override bool IsFalseInternal() => NodeToInvert.IsTrue();

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is IInvertedFilterNode invertedOther
                   && NodeToInvert.Equals(invertedOther.NodeToInvert);
        }

        protected override int GetHashCodeInternal()
        {
            unchecked
            {
                return (NodeToInvert.GetHashCode() * 397) ^ -1;
            }
        }

        public override string ToString() => $"NOT ({NodeToInvert})";
    }
}