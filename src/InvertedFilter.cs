﻿using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class InvertedFilter<TLeafNode> : InternalFilterNode, IInvertedFilter<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        private readonly bool _isCollapsed;

        public InvertedFilter(IFilterNode<TLeafNode> filterToInvert)
            : this(filterToInvert, isCollapsed: false)
        {
        }

        internal InvertedFilter(IFilterNode<TLeafNode> filterToInvert, bool isCollapsed)
        {
            FilterToInvert = filterToInvert;
            _isCollapsed = isCollapsed;
        }

        public IFilterNode<TLeafNode> FilterToInvert { get; }

        IFilterNode IInvertedFilter.FilterToInvert => FilterToInvert;

        public TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var innerFilter = FilterToInvert.Aggregate(combine, invert, transform);
            return invert(innerFilter);
        }

        public TResult Match<TResult>(
            Func<ICombinationFilter<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            return invert(this);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode
        {
            var innerResult = FilterToInvert.Map(mapFunc);
            return new InvertedFilter<TResultLeafNode>(innerResult);
        }

        public IFilterNode<TResultLeafNode> Bind<TResultLeafNode>(Func<TLeafNode, IFilterNode<TResultLeafNode>> bindFunc)
            where TResultLeafNode : class, ILeafFilterNode
        {
            var innerResult = FilterToInvert.Bind(bindFunc);
            return new InvertedFilter<TResultLeafNode>(innerResult);
        }

        public IFilterNode<TLeafNode> Collapse()
        {
            if (_isCollapsed)
            {
                return this;
            }

            var collapsedInnerFilter = FilterToInvert.Collapse();
            // If we have NOT(TRUE) then return FALSE or if we have NOT(FALSE) return TRUE.
            if (collapsedInnerFilter is ICombinationFilter<TLeafNode> combinationInner)
            {
                if (combinationInner.IsTrue())
                {
                    return FilterNode<TLeafNode>.False;
                }

                if (combinationInner.IsFalse())
                {
                    return FilterNode<TLeafNode>.True;
                }
            }

            // If we have NOT(NOT(f)) just return f
            if (collapsedInnerFilter is IInvertedFilter<TLeafNode> invertedInner)
            {
                return invertedInner.FilterToInvert;
            }

            return new InvertedFilter<TLeafNode>(collapsedInnerFilter, isCollapsed: true);
        }

        public bool Any(Func<TLeafNode, bool> predicate)
            => FilterToInvert.Any(predicate);

        public bool All(Func<TLeafNode, bool> predicate)
            => FilterToInvert.All(predicate);

        public bool IsTrue() => FilterToInvert.IsFalse();

        public bool IsFalse() => FilterToInvert.IsTrue();

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is IInvertedFilter invertedOther
                   && FilterToInvert.Equals(invertedOther.FilterToInvert);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FilterToInvert.GetHashCode() * 397) ^ -1;
            }
        }

        public override string ToString() => $"NOT ({FilterToInvert})";
    }
}