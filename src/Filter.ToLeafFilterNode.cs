namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterExtensions
    {
        public static LeafFilterNode<TFilter> ToLeafFilterNode<TFilter>(this TFilter filter)
            where TFilter : IFilter
            => new LeafFilterNode<TFilter>(filter);
    }
}