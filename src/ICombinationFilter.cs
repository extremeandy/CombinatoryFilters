using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface ICombinationFilter : IInternalFilterNode
    {
        IReadOnlyCollection<IFilterNode> Filters { get; }

        CombinationOperator Operator { get; }
    }

    public interface ICombinationFilter<out TLeafNode> : IInternalFilterNode<TLeafNode>, ICombinationFilter
        where TLeafNode : class, ILeafFilterNode
    {
        new IReadOnlyCollection<IFilterNode<TLeafNode>> Filters { get; }
    }
}