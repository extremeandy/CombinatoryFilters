namespace ExtremeAndy.CombinatoryFilters
{
    public interface ILeafFilterNode : IFilterNode
    {
        IFilter Filter { get; }
    }

    public interface ILeafFilterNode<out TFilter> : IFilterNode<TFilter>, ILeafFilterNode
    {
        new TFilter Filter { get; }
    }
}