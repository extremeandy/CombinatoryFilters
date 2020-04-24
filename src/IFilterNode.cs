using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface IFilterNode
    {
    }

    public interface IFilterNode<out TLeafNode> : IFilterNode
        where TLeafNode : class, ILeafFilterNode
    {
        TResult Aggregate<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform);

        TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform);

        IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode;

        IFilterNode<TLeafNode> Collapse();

        bool Any(Func<TLeafNode, bool> predicate);
    }
}