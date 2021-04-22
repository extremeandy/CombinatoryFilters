using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class FilterNode<TLeafNode> : FilterNode, IFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        /// <summary>
        /// Empty filter which is the equivalent of 'true'
        /// </summary>
        public static readonly CombinationFilter<TLeafNode> True = new CombinationFilter<TLeafNode>(Array.Empty<IFilterNode<TLeafNode>>(), CombinationOperator.All, preserveOrder: true, isCollapsed: true);

        /// <summary>
        /// Empty filter which is the equivalent of 'false'
        /// </summary>
        public static readonly CombinationFilter<TLeafNode> False = new CombinationFilter<TLeafNode>(Array.Empty<IFilterNode<TLeafNode>>(), CombinationOperator.Any, preserveOrder: true, isCollapsed: true);

        public abstract TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform);

        public abstract TResult Match<TResult>(
            Func<ICombinationFilter<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform);

        public abstract IFilterNode<TResultLeafNode> Map<TResultLeafNode>(Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode;

        public abstract IFilterNode<TResultLeafNode> Bind<TResultLeafNode>(Func<TLeafNode, IFilterNode<TResultLeafNode>> bindFunc)
            where TResultLeafNode : class, ILeafFilterNode;

        public abstract IFilterNode<TLeafNode> Collapse();

        public abstract bool Any(Func<TLeafNode, bool> predicate);

        public abstract bool All(Func<TLeafNode, bool> predicate);

        public abstract bool IsTrue();

        public abstract bool IsFalse();
    }

    public abstract class FilterNode : IFilterNode
    {
    }
}