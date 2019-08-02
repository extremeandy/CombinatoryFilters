namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Defines the method we use to combine zero or more filters
    /// </summary>
    public enum CombinationOperator
    {
        /// <summary>
        /// All filters in the combination must be satisfied. If there are zero filters, then the result will always be true.
        /// </summary>
        All,

        /// <summary>
        /// At least one filter in the combination must be satisifed. If there are zero filters, then the result will always be false.
        /// </summary>
        Any
    }
}