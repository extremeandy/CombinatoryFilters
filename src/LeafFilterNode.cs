using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class LeafFilterNode<TFilter> : FilterNode<TFilter>, ILeafFilterNode<TFilter>
        where TFilter : IFilter
    {
        private readonly Lazy<bool> _isTrue;
        private readonly Lazy<bool> _isFalse;
        private readonly Lazy<int> _hashCode;

        public LeafFilterNode(TFilter filter)
        {
            Filter = filter;
            _isTrue = new Lazy<bool>(filter.IsTrue);
            _isFalse = new Lazy<bool>(filter.IsFalse);
            _hashCode = new Lazy<int>(filter.GetHashCode);
        }

        public TFilter Filter { get; }
        IFilter ILeafFilterNode.Filter => Filter;

        /// <summary>
        /// This is always true for a leaf, since it is a leaf it cannot have any children,
        /// and so is already collapsed.
        /// </summary>
        public override bool IsCollapsed => true;

        public override TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
            => transform(this);

        public override TResult Match<TResult>(
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform)
            => transform(this);

        public override IFilterNode<TResultFilter> Map<TResultFilter>(Func<TFilter, TResultFilter> mapFunc)
            => mapFunc(Filter).ToLeafFilterNode();

        public override IFilterNode<TResultFilter> Bind<TResultFilter>(Func<TFilter, IFilterNode<TResultFilter>> bindFunc)
            => bindFunc(Filter);

        public override IFilterNode<TFilter> Sort(IComparer<IFilterNode<TFilter>> comparer) => this;

        public override IFilterNode<TFilter> Collapse()
        {
            if (_isTrue.Value)
            {
                return True;
            }

            if (_isFalse.Value)
            {
                return False;
            }

            return this;
        }

        public override bool Any(Func<TFilter, bool> predicate)
            => predicate(Filter); // Hack to work around c# not supporting higher-order polymorphism

        public override bool All(Func<TFilter, bool> predicate)
            => predicate(Filter); // Hack to work around c# not supporting higher-order polymorphism

        protected override bool IsEquivalentToInternal(IFilterNode other)
            => Equals(other);

        public override bool IsTrue() => _isTrue.Value;

        public override bool IsFalse() => _isFalse.Value;

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is ILeafFilterNode leafOther
                   && Filter.Equals(leafOther.Filter);
        }

        public override int GetHashCode() => _hashCode.Value;

        public override string ToString() => Filter.ToString();
    }
}