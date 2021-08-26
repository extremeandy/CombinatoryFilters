using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public class FilterNodeComparer<TFilter> : IComparer<IFilterNode<TFilter>>
        where TFilter : IFilter
    {
        private readonly IComparer<TFilter> _filterComparer;
        private readonly ReadOnlyListComparer<IFilterNode<TFilter>> _filterNodeListComparer;

        public FilterNodeComparer()
            : this(Comparer<TFilter>.Default)
        {
        }

        public FilterNodeComparer(IComparer<TFilter> filterComparer)
        {
            _filterComparer = filterComparer;
            _filterNodeListComparer = new ReadOnlyListComparer<IFilterNode<TFilter>>(this);
        }

        public int Compare(IFilterNode<TFilter> x, IFilterNode<TFilter> y)
        {
            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            // Avoid tail recursion with loop.
            while (true)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x is ILeafFilterNode<TFilter> xLeaf)
                {
                    if (y is ILeafFilterNode<TFilter> yLeaf)
                    {
                        return _filterComparer.Compare(xLeaf.Filter, yLeaf.Filter);
                    }

                    // Leaf always comes first.
                    return -1;
                }

                if (x is IInvertedFilterNode<TFilter> xInverted)
                {
                    if (y is ILeafFilterNode<TFilter>)
                    {
                        // Leaf comes before inverted
                        return 1;
                    }

                    if (y is IInvertedFilterNode<TFilter> yInverted)
                    {
                        x = xInverted.NodeToInvert;
                        y = yInverted.NodeToInvert;
                        continue;
                    }

                    if (y is ICombinationFilterNode<TFilter>)
                    {
                        // Combination comes after inverted
                        return -1;
                    }
                }

                if (x is ICombinationFilterNode<TFilter> xCombination)
                {
                    if (y is ICombinationFilterNode<TFilter> yCombination)
                    {
                        var comparison = xCombination.Operator.CompareTo(yCombination.Operator);
                        if (comparison != 0)
                        {
                            return comparison;
                        }

                        comparison = _filterNodeListComparer.Compare(xCombination.Nodes, yCombination.Nodes);
                        return comparison;
                    }

                    // Combination always comes last
                    return 1;
                }

                throw new NotSupportedException();
            }
        }

        public static readonly FilterNodeComparer<TFilter> Default = new FilterNodeComparer<TFilter>();
    }
}
