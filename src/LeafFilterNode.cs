using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class LeafFilterNode<TFilter> : FilterNode<TFilter>, ILeafFilterNode<TFilter>
        where TFilter : IFilter
    {
        public LeafFilterNode(TFilter filter)
        {
            Filter = filter;
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
            if (IsTrue())
            {
                return True;
            }

            if (IsFalse())
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

        protected override bool IsTrueInternal() => Filter.IsTrue();

        protected override bool IsFalseInternal() => Filter.IsFalse();

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is ILeafFilterNode leafOther
                   && Filter.Equals(leafOther.Filter);
        }

        protected override int GetHashCodeInternal() => Filter.GetHashCode();

        public override string ToString() => Filter.ToString();
    }
}