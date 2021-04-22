using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Unordered set of <see cref="IFilterNode{TLeafNode}"/> to be combined according to
    /// then given <see cref="CombinationOperator"/>
    /// </summary>
    /// <typeparam name="TLeafNode"></typeparam>
    public abstract class CombinationFilterBase<TLeafNode> : CombinationFilterBase, ICombinationFilter<TLeafNode>
        where TLeafNode : class, ILeafFilterNode
    {
        private readonly bool _isCollapsed;
        protected readonly IFilterNode<TLeafNode>[] FiltersArray;

        protected CombinationFilterBase(IReadOnlyCollection<IFilterNode<TLeafNode>> filters, CombinationOperator @operator, bool isCollapsed)
            : base(filters, @operator)
        {
            FiltersArray = filters.ToArray();
            _isCollapsed = isCollapsed;
        }

        public new IReadOnlyCollection<IFilterNode<TLeafNode>> Filters => FiltersArray;

        public TResult Aggregate<TResult>(
            Func<TResult[], CombinationOperator, TResult> combine,
            Func<TResult, TResult> invert,
            Func<TLeafNode, TResult> transform)
        {
            var mappedInnerFilters = new TResult[FiltersArray.Length];
            for (var i = 0; i < FiltersArray.Length; i++)
            {
                mappedInnerFilters[i] = FiltersArray[i].Aggregate(combine, invert, transform);
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
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator);
        }

        public IFilterNode<TResultLeafNode> Bind<TResultLeafNode>(Func<TLeafNode, IFilterNode<TResultLeafNode>> bindFunc)
            where TResultLeafNode : class, ILeafFilterNode
        {
            var innerFilters = Filters.Select(f => f.Bind(bindFunc));
            return new CombinationFilter<TResultLeafNode>(innerFilters, Operator);
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
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator, isCollapsed: true);
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
                    var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, Operator, isCollapsed: true);
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
    }

    public abstract class CombinationFilterBase : InternalFilterNode, ICombinationFilter
    {
        protected CombinationFilterBase(IEnumerable<IFilterNode> filters, CombinationOperator @operator = default)
            : this(new HashSet<IFilterNode>(filters), @operator)
        {
        }

        protected CombinationFilterBase(HashSet<IFilterNode> filters, CombinationOperator @operator = default)
        {
            Filters = filters;
            Operator = @operator;
        }

        public IReadOnlyCollection<IFilterNode> Filters { get; }

        public CombinationOperator Operator { get; }

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