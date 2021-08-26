using System.Collections.Generic;

namespace ExtremeAndy.CombinatoryFilters
{
    internal class ReadOnlyListComparer<T> : IComparer<IReadOnlyList<T>>
    {
        private readonly IComparer<T> _comparer;

        public ReadOnlyListComparer()
            : this(Comparer<T>.Default)
        {
        }

        public ReadOnlyListComparer(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(IReadOnlyList<T> first, IReadOnlyList<T> second)
        {
            if (ReferenceEquals(first, second))
            {
                return 0;
            }

            if (first is null)
            {
                return -1;
            }

            if (second is null)
            {
                return 1;
            }

            var firstLength = first.Count;
            var secondLength = second.Count;

            for (var i = 0; i < firstLength && i < secondLength; i++)
            {
                var valueCompare = _comparer.Compare(first[i], second[i]);
                if (valueCompare != 0)
                {
                    return valueCompare;
                }
            }

            // If first has more elements than second, then first must be greater than second.
            // If second has more elements than first, then first must be less than second.
            // If both sequences have exhausted elements, then they must be equal.
            return firstLength.CompareTo(secondLength);
        }

        public static readonly ReadOnlyListComparer<T> Default = new ReadOnlyListComparer<T>();
    }
}