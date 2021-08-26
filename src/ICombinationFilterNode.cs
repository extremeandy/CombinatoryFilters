using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface ICombinationFilterNode : IFilterNode
    {
        IReadOnlyList<IFilterNode> Nodes { get; }

        CombinationOperator Operator { get; }
    }

    public interface ICombinationFilterNode<out TFilter> : IFilterNode<TFilter>, ICombinationFilterNode
    {
        new IReadOnlyList<IFilterNode<TFilter>> Nodes { get; }
    }
}