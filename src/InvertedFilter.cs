using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public class InvertedFilter<TLeafNode> : InvertedFilter, IInvertedFilter<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        public InvertedFilter(IFilterNode<TLeafNode> filterToInvert) : base(filterToInvert)
        {
            FilterToInvert = filterToInvert;
        }

        public new IFilterNode<TLeafNode> FilterToInvert { get; }

        public TResult Aggregate<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var innerFilter = FilterToInvert.Aggregate(combine, invert, transform);
            return invert(innerFilter);
        }

        public TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            return invert(this);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            var innerResult = FilterToInvert.Map(mapFunc);
            return new InvertedFilter<TResultLeafNode>(innerResult);
        }

        public bool Any(Func<TLeafNode, bool> predicate)
        {
            return FilterToInvert.Any(predicate);
        }
    }

    public abstract class InvertedFilter : InternalFilterNode, IInvertedFilter
    {
        protected InvertedFilter(IFilterNode filterToInvert)
        {
            FilterToInvert = filterToInvert;
        }

        public IFilterNode FilterToInvert { get; }

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