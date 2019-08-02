using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface ICombinationFilterNode : IInternalFilterNode
    {
        IReadOnlyCollection<IFilterNode> Filters { get; }

        CombinationOperator Operator { get; }
    }

    public interface ICombinationFilterNode<out TLeafNode> : IInternalFilterNode<TLeafNode>, ICombinationFilterNode
        where TLeafNode : class, ILeafFilterNode, IFilterNode<TLeafNode>
    {
        new IReadOnlyCollection<IFilterNode<TLeafNode>> Filters { get; }
    }
}