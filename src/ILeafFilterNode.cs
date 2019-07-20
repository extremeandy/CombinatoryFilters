namespace ExtremeAndy.CombinatoryFilters
{
    public interface ILeafFilterNode : IFilterNode
    {
    }

    public interface ILeafFilterNode<out TThis> : IFilterNode<TThis>, ILeafFilterNode
        where TThis : class, ILeafFilterNode<TThis>
    {
    }
}