namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class Filter : IFilter
    {
        /// <inheritdoc cref="IFilter.IsTrue"/>
        public virtual bool IsTrue() => false;

        /// <inheritdoc cref="IFilter.IsFalse"/>
        public virtual bool IsFalse() => false;
    }

    /// <summary>
    /// Provides a mechanism to actually test an item against a filter. This might seem
    /// like an obvious thing to do, but in some cases one might want to build a filter
    /// without any way to evalute it in the context of the CLR, but use it to e.g. build
    /// SQL queries.
    /// </summary>
    public abstract class Filter<TItemToTest> : Filter, IFilter<TItemToTest>
    {
        /// <inheritdoc cref="IFilter{TItemToTest}.IsMatch"/>
        public abstract bool IsMatch(TItemToTest item);
    }
}