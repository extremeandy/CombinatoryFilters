using System;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class CharFilter : LeafFilterNode<CharFilter>, IRealisableLeafFilterNode<string>, IEquatable<IFilterNode>
    {
        public CharFilter(char c)
        {
            Character = c;
        }

        public char Character { get; }

        public bool Equals(IFilterNode other)
        {
            return other is CharFilter charOther
                   && Character == charOther.Character;
        }

        public override bool Equals(object obj)
        {
            return obj is IFilterNode other && Equals(other);
        }

        public override int GetHashCode() => Character.GetHashCode();

        public bool IsMatch(string item) => item.Contains(Character);

        public override string ToString() => $"'{Character}'";
    }
}