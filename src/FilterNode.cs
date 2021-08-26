using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class FilterNode<TFilter> : IFilterNode<TFilter>
        where TFilter : IFilter
    {
        /// <summary>
        /// Empty filter which is the equivalent of 'true'
        /// </summary>
        public static readonly CombinationFilterNode<TFilter> True = new CombinationFilterNode<TFilter>(Array.Empty<IFilterNode<TFilter>>(), CombinationOperator.All, isCollapsed: true, comparer: FilterNodeComparer<TFilter>.Default);

        /// <summary>
        /// Empty filter which is the equivalent of 'false'
        /// </summary>
        public static readonly CombinationFilterNode<TFilter> False = new CombinationFilterNode<TFilter>(Array.Empty<IFilterNode<TFilter>>(), CombinationOperator.Any, isCollapsed: true, comparer: FilterNodeComparer<TFilter>.Default);

        public abstract TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform);

        public abstract TResult Match<TResult>(
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform);

        public abstract IFilterNode<TResultFilter> Map<TResultFilter>(Func<TFilter, TResultFilter> mapFunc)
            where TResultFilter : IFilter;

        public abstract IFilterNode<TResultFilter> Bind<TResultFilter>(Func<TFilter, IFilterNode<TResultFilter>> bindFunc)
            where TResultFilter : IFilter;

        public abstract IFilterNode<TFilter> Sort(IComparer<IFilterNode<TFilter>> comparer);

        public abstract IFilterNode<TFilter> Collapse();
        IFilterNode IFilterNode.Collapse() => Collapse();

        public abstract bool Any(Func<TFilter, bool> predicate);

        public abstract bool All(Func<TFilter, bool> predicate);

        public bool IsEquivalentTo(IFilterNode other)
        {
            if (Equals(this, other))
            {
                return true;
            }

            if (!IsCollapsed)
            {
                return Collapse().IsEquivalentTo(other);
            }

            return IsEquivalentToInternal(other.Collapse());
        }

        /// <summary>
        /// The
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected abstract bool IsEquivalentToInternal(IFilterNode other);

        public abstract bool IsTrue();

        public abstract bool IsFalse();

        public abstract bool Equals(IFilterNode other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is IFilterNode other && Equals(other);
        }

        public abstract override int GetHashCode();
        
        public abstract bool IsCollapsed { get; }
    }
}