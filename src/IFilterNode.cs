using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface IFilterNode
    {
    }

    public interface IFilterNode<out TLeafNode> : IFilterNode
        where TLeafNode : class, ILeafFilterNode
    {
        TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform);

        TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform);

        IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode;

        IFilterNode<TResultLeafNode> Bind<TResultLeafNode>(Func<TLeafNode, IFilterNode<TResultLeafNode>> bindFunc)
            where TResultLeafNode : class, ILeafFilterNode;

        IFilterNode<TLeafNode> Collapse();

        bool Any(Func<TLeafNode, bool> predicate);

        bool All(Func<TLeafNode, bool> predicate);

        /// <summary>
        /// Returns <see langword="true" /> if the filter is will always evaluate to <see langword="true" />
        /// regardless of the input, i.e. the filter is trivial and will include everything.
        ///
        /// Returns <see langword="false" /> if it is not known whether the filter will evaluate to <see langword="true" />
        /// until an input is tested.
        ///
        /// Note that <see cref="IsTrue"/> returning <see langword="false" /> does *not* automatically imply that
        /// <see cref="IsFalse"/> will return <see langword="true" />.
        /// </summary>
        /// <returns></returns>
        bool IsTrue();

        /// <summary>
        /// Returns <see langword="true" /> if the filter is will always evaluate to <see langword="false" />
        /// regardless of the input, i.e. the filter is trivial and will include nothing.
        ///
        /// Returns <see langword="false" /> if it is not known whether the filter will evaluate to <see langword="false" />
        /// until an input is tested.
        ///
        /// Note that <see cref="IsFalse"/> returning <see langword="false" /> does *not* automatically imply that
        /// <see cref="IsTrue"/> will return <see langword="true" />.
        /// </summary>
        /// <returns></returns>
        bool IsFalse();
    }
}