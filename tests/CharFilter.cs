using System;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class CharFilter : Filter<string>, IEquatable<CharFilter>, IComparable<CharFilter>
    {
        public CharFilter(char c)
        {
            Character = c;
        }

        public char Character { get; }

        public bool Equals(CharFilter other)
        {
            if (other is null)
            {
                return false;
            }

            return Character == other.Character;
        }

        public override bool Equals(object obj)
            => obj is CharFilter other && Equals(other);

        public override int GetHashCode() => Character.GetHashCode();

        public override bool IsMatch(string item) => item.Contains(Character);

        public override string ToString() => $"'{Character}'";

        public int CompareTo(CharFilter other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return Character.CompareTo(other.Character);
        }
    }
}