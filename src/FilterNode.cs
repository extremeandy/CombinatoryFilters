using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class FilterNode<TLeafNode> : FilterNode, IFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        public static OrderedCombinationFilter<TLeafNode> Empty = new OrderedCombinationFilter<TLeafNode>(new IFilterNode<TLeafNode>[0], CombinationOperator.And);

        public abstract TResult Match<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform);

        public abstract TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform);

        public abstract IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>;
    }

    public abstract class FilterNode : IFilterNode
    {
    }
}