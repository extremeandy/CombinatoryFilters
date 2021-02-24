﻿using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class FilterNode<TLeafNode> : FilterNode, IFilterNode<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        /// <summary>
        /// Empty filter which is the equivalent of 'true'
        /// </summary>
        public static OrderedCombinationFilter<TLeafNode> True = new OrderedCombinationFilter<TLeafNode>(new IFilterNode<TLeafNode>[0], CombinationOperator.All, isCollapsed: true);

        /// <summary>
        /// Empty filter which is the equivalent of 'false'
        /// </summary>
        public static OrderedCombinationFilter<TLeafNode> False = new OrderedCombinationFilter<TLeafNode>(new IFilterNode<TLeafNode>[0], CombinationOperator.Any, isCollapsed: true);

        public abstract TResult Aggregate<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform);

        public abstract TResult Match<TResult>(
            Func<ICombinationFilterNode<TLeafNode>, TResult> combine,
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