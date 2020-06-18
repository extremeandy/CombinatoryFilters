﻿using System;
using System.Collections.Generic;
using System.Net;

namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class LeafFilterNode<TThis> : FilterNode, ILeafFilterNode<TThis>
        where TThis : class, ILeafFilterNode<TThis>
    {
        public TResult Aggregate<TResult>(
            Func<IEnumerable<TResult>, CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TThis, TResult> transform)
        {
            return transform(this as TThis); // Hack to work around c# not supporting higher-order polymorphism
        }

        public TResult Match<TResult>(
            Func<ICombinationFilterNode<TThis>, TResult> combine,
            Func<IInvertedFilter<TThis>, TResult> invert,
            Func<TThis, TResult> transform)
        {
            return transform(this as TThis); // Hack to work around c# not supporting higher-order polymorphism
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(
            Func<TThis, TResultLeafNode> mapFunc) where TResultLeafNode : class, ILeafFilterNode
        {
            return (IFilterNode<TResultLeafNode>)mapFunc(this as TThis); // Hack to work around c# not supporting higher-order polymorphism
        }

        public IFilterNode<TThis> Collapse()
        {
            if (IsTrue())
            {
                return FilterNode<TThis>.True;
            }

            if (IsFalse())
            {
                return FilterNode<TThis>.False;
            }

            return this;
        }

        public bool Any(Func<TThis, bool> predicate)
        {
            return predicate(this as TThis); // Hack to work around c# not supporting higher-order polymorphism
        }

        /// <summary>
        /// This can be overridden if the leaf filter node can be determined to be True.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTrue() => false;

        /// <summary>
        /// This can be overridden if the leaf filter node can be determined to be False.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsFalse() => false;
    }
}