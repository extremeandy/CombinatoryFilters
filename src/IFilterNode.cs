using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface IFilterNode : IEquatable<IFilterNode>
    {
        bool IsCollapsed { get; }

        /// <summary>
        /// As for <see cref="IEquatable{T}.Equals"/>, but uses an unordered set comparison
        /// operation which ignores duplicates and order of nodes.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IsEquivalentTo(IFilterNode other);

        IFilterNode Collapse();

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

    public interface IFilterNode<out TFilter> : IFilterNode
    {
        TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform);

        TResult Match<TResult>(
            Func<ICombinationFilterNode<TFilter>, TResult> combine,
            Func<IInvertedFilterNode<TFilter>, TResult> invert,
            Func<ILeafFilterNode<TFilter>, TResult> transform);

        IFilterNode<TResultFilter> Map<TResultFilter>(Func<TFilter, TResultFilter> mapFunc)
            where TResultFilter : IFilter;

        IFilterNode<TResultFilter> Bind<TResultFilter>(Func<TFilter, IFilterNode<TResultFilter>> bindFunc)
            where TResultFilter : IFilter;

        /// <summary>
        /// Sort the nodes according to the supplied <see cref="FilterNodeComparer{TFilter}"/>.
        /// </summary>
        IFilterNode<TFilter> Sort(IComparer<IFilterNode<TFilter>> comparer);

        IFilterNode<TFilter> Collapse();

        bool Any(Func<TFilter, bool> predicate);

        bool All(Func<TFilter, bool> predicate);
    }
}