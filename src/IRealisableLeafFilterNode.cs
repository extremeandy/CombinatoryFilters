namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Provides a mechanism to actually test an item against a filter. This might seem
    /// like an obvious thing to do, but in some cases one might want to build a filter
    /// without any way to evalute it in the context of the CLR, but use it to e.g. build
    /// SQL queries.
    /// </summary>
    /// <typeparam name="TItemToTest"></typeparam>
    public interface IRealisableLeafFilterNode<in TItemToTest> : ILeafFilterNode
    {
        bool IsMatch(TItemToTest item);
    }
}