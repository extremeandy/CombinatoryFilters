namespace ExtremeAndy.CombinatoryFilters
{
    public abstract class InternalFilterNode : FilterNode, IInternalFilterNode
    {
        public abstract bool Equals(IFilterNode other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is IFilterNode other && Equals(other);
        }

        public abstract override int GetHashCode();
    }
}