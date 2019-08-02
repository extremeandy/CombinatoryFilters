using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface IInternalFilterNode : IFilterNode, IEquatable<IFilterNode>
    {
    }

    public interface IInternalFilterNode<out TLeafNode> : IFilterNode<TLeafNode>, IInternalFilterNode
        where TLeafNode : class, ILeafFilterNode
    {
    }
}