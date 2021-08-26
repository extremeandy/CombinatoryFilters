namespace ExtremeAndy.CombinatoryFilters
{
    public interface IFilter
    {
        /// <summary>
        /// This can be overridden if the leaf filter can be determined to be True.
        /// If unknown, just return <see langword="false" />.
        /// </summary>
        /// <returns></returns>
        bool IsTrue();

        /// <summary>
        /// This can be overridden if the leaf filter can be determined to be False.
        /// If unknown, just return <see langword="false" />.
        /// </summary>
        /// <returns></returns>
        bool IsFalse();
    }

    /// <summary>
    /// Provides a mechanism to actually test an item against a filter. This might seem
    /// like an obvious thing to do, but in some cases one might want to build a filter
    /// without any way to evalute it in the context of the CLR, but use it to e.g. build
    /// SQL queries.
    /// </summary>
    /// <typeparam name="TItemToTest"></typeparam>
    public interface IFilter<in TItemToTest> : IFilter
    {
        /// <summary>
        /// Return a boolean indicating whether the item satisfied the filter condition
        /// </summary>
        bool IsMatch(TItemToTest item);
    }
}