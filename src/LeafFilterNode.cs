﻿using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class LeafFilterNode<TThis> : FilterNode, ILeafFilterNode<TThis>
        where TThis : class, ILeafFilterNode<TThis>
    {
        public TResult Match<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TThis, TResult> transform)
        {
            return transform(this as TThis);
        }

        public TResult Match<TResult>(
            Func<ICombinationFilterNode<TThis>, TResult> combine,
            Func<IInvertedFilter<TThis>, TResult> invert,
            Func<TThis, TResult> transform)
        {
            return transform(this as TThis);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(
            Func<TThis, TResultLeafNode> mapFunc) where TResultLeafNode : class, ILeafFilterNode<TResultLeafNode>
        {
            return mapFunc(this as TThis);  // Hack to work around c# not supporting higher-order polymorphism
        }
    }
}