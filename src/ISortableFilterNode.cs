using System;
using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    public interface ISortableFilterNode<TFilter> : IFilterNode<TFilter>, IComparable<ISortableFilterNode<TFilter>>
    {
        IFilterNode<TFilter> Sort(IComparer<TFilter> comparer);
    }
}