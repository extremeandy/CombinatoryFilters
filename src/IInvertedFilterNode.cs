namespace ExtremeAndy.CombinatoryFilters
{
    public interface IInvertedFilterNode : IFilterNode
    {
        IFilterNode NodeToInvert { get; }
    }

    public interface IInvertedFilterNode<out TFilter> : IFilterNode<TFilter>, IInvertedFilterNode
    {
        new IFilterNode<TFilter> NodeToInvert { get; }
    }
}