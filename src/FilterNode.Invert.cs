namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        public static InvertedFilterNode<TFilter> Invert<TFilter>(this IFilterNode<TFilter> filterNode)
            where TFilter : IFilter
            => new InvertedFilterNode<TFilter>(filterNode);
    }
}