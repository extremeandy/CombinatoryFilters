﻿namespace ExtremeAndy.CombinatoryFilters
{
    public interface IInvertedFilter : IInternalFilterNode
    {
        IFilterNode FilterToInvert { get; }
    }

    public interface IInvertedFilter<out TLeafNode> : IInternalFilterNode<TLeafNode>, IInvertedFilter
        where TLeafNode : class, ILeafFilterNode
    {
        new IFilterNode<TLeafNode> FilterToInvert { get; }
    }
}