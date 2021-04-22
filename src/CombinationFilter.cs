using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public sealed class CombinationFilter<TLeafNode> : InternalFilterNode, ICombinationFilter<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        private readonly bool _isCollapsed;
        private readonly IFilterNode<TLeafNode>[] _filtersArray;
        private readonly Lazy<int> _hashCode;

        /// <summary>
        /// Will only be used if we try to compare combination filters with a slightly different generic parameter.
        /// </summary>
        private readonly Lazy<HashSet<IFilterNode>> _filtersSetUntyped;

        public CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator = default, bool preserveOrder = false)
            : this(filters, @operator, preserveOrder, isCollapsed: false)
        {
        }

        public CombinationFilter(IFilterNode<TLeafNode>[] filters, CombinationOperator @operator = default, bool preserveOrder = false)
            : this(filters, @operator, preserveOrder, isCollapsed: false)
        {
        }

        internal CombinationFilter(IEnumerable<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool preserveOrder, bool isCollapsed)
            : this(filters.ToArray(), @operator, preserveOrder, isCollapsed)
        {
        }

        internal CombinationFilter(IFilterNode<TLeafNode>[] filters, CombinationOperator @operator, bool preserveOrder, bool isCollapsed)
        {
            _isCollapsed = isCollapsed;
            _filtersArray = filters;
            var filtersSet = new HashSet<IFilterNode<TLeafNode>>(filters);
            FiltersSet = filtersSet;
            Operator = @operator;
            PreserveOrder = preserveOrder;

            if (PreserveOrder)
            {
                Filters = filters;
            }
            else
            {
                Filters = FiltersSet;
            }

            _filtersSetUntyped = new Lazy<HashSet<IFilterNode>>(() => new HashSet<IFilterNode>(filters));

            _hashCode = new Lazy<int>(() =>
            {
                var filterHashCodes = PreserveOrder
                    ? filters.Select(x => x.GetHashCode())
                    : filters.Select(x => x.GetHashCode()).OrderBy(x => x);

                var hashCode = filterHashCodes.Aggregate(0, (acc, h) => (acc * 397) ^ h);
                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                
                // Note: Don't include PreserveOrder in the hashcode, because we do allow for PreserveOrder to differ and still have equality
                // between CombinationFilters if the number of filters contained is zero.

                return hashCode;
            });
        }

        IReadOnlyCollection<IFilterNode> ICombinationFilter.Filters => Filters;

        public CombinationOperator Operator { get; }

        public bool PreserveOrder { get; }

        public readonly HashSet<IFilterNode<TLeafNode>> FiltersSet;
        
        public IReadOnlyCollection<IFilterNode<TLeafNode>> Filters { get; }

        public TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var mappedInnerFilters = new TResult[_filtersArray.Length];
            for (var i = 0; i < _filtersArray.Length; i++)
            {
                mappedInnerFilters[i] = _filtersArray[i].Aggregate(combine, invert, transform);
            }

            return combine(mappedInnerFilters, Operator);
        }

        public TResult Match<TResult>(
            Func<ICombinationFilter<TLeafNode>, TResult> combine,
            Func<IInvertedFilter<TLeafNode>, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            return combine(this);
        }

        public IFilterNode<TResultLeafNode> Map<TResultLeafNode>(
            Func<TLeafNode, TResultLeafNode> mapFunc)
            where TResultLeafNode : class, ILeafFilterNode
        {
            var innerFilters = Filters.Select(f => f.Map(mapFunc));
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator, PreserveOrder);
        }

        public IFilterNode<TResultLeafNode> Bind<TResultLeafNode>(Func<TLeafNode, IFilterNode<TResultLeafNode>> bindFunc)
            where TResultLeafNode : class, ILeafFilterNode
        {
            var innerFilters = Filters.Select(f => f.Bind(bindFunc));
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator, PreserveOrder);
        }

        public IFilterNode<TLeafNode> Collapse()
        {
            if (_isCollapsed)
            {
                return this;
            }

            return Operator.Match(
                () =>
                {
                    var collapsedInnerFilters = Filters.Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerFilters.Any(f => f.IsFalse()))
                    {
                        return FilterNode<TLeafNode>.False;
                    }

                    var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.IsTrue());
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator, PreserveOrder, isCollapsed: true);
                    return collapsedCombinationFilter.Filters.Count == 1
                        ? collapsedCombinationFilter.Filters.Single()
                        : collapsedCombinationFilter;
                },
                () =>
                {
                    var collapsedInnerFilters = Filters.Select(f => f.Collapse())
                        .ToList();

                    if (collapsedInnerFilters.Any(f => f.IsTrue()))
                    {
                        return FilterNode<TLeafNode>.True;
                    }

                    var nonTrivialFilters = collapsedInnerFilters.Where(f => !f.IsFalse());
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator, PreserveOrder, isCollapsed: true);
                    return collapsedCombinationFilter.Filters.Count == 1
                        ? collapsedCombinationFilter.Filters.Single()
                        : collapsedCombinationFilter;
                });
        }

        public bool Any(Func<TLeafNode, bool> predicate)
            => Filters.Any(f => f.Any(predicate));

        public bool All(Func<TLeafNode, bool> predicate)
            => Filters.All(f => f.All(predicate));

        public bool IsTrue()
            => Operator.Match(
                () => Filters.All(f => f.IsTrue()),
                () => Filters.Any(f => f.IsTrue()));

        public bool IsFalse()
            => Operator.Match(
                () => Filters.Any(f => f.IsFalse()),
                () => Filters.All(f => f.IsFalse()));

        public override bool Equals(IFilterNode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!(other is ICombinationFilter combinationOther)
                || Operator != combinationOther.Operator)
            {
                return false;
            }

            if (PreserveOrder != combinationOther.PreserveOrder)
            {
                return _filtersArray.Length == 0 && combinationOther.Filters.Count == 0;
            }

            if (PreserveOrder)
            {
                return Filters.SequenceEqual(combinationOther.Filters);
            }

            if (other is CombinationFilter<TLeafNode> concreteOther)
            {
                return FiltersSet.SetEquals(concreteOther.FiltersSet);
            }

            // Fall back when the types aren't exactly equivalent, which may be possible if we are comparing
            // with a combination filter that has a type parameter when there is an inheritance relationship
            // between the types.
            return _filtersSetUntyped.Value.SetEquals(combinationOther.Filters);
        }

        public override int GetHashCode() => _hashCode.Value;

        public override string ToString()
        {
            if (Filters.Count == 0)
            {
                return Operator.Match(() => "TRUE", () => "FALSE");
            }

            var delimeter = Operator.Match(() => " AND ", () => " OR ");
            return string.Join(delimeter, Filters.Select(f => $"({f})"));
        }
    }
}