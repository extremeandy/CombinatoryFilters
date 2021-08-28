using System;

namespace ExtremeAndy.CombinatoryFilters
{
    public static partial class FilterNodeExtensions
    {
        public static Func<TItemToTest, bool> GetPredicate<TFilter, TItemToTest>(this IFilterNode<TFilter> filter)
            where TFilter : IFilter<TItemToTest>
            => filter.GetPredicate<TFilter, TItemToTest>(leafFilter => leafFilter.IsMatch);

        private static Func<TItemToTest, bool> GetPredicate<TFilter, TItemToTest>(
            this IFilterNode<TFilter> filterNode,
            Func<TFilter, Func<TItemToTest, bool>> itemPredicate)
            => filterNode.Aggregate(
                Combine,
                Invert,
                leafFilterNode => itemPredicate(leafFilterNode.Filter));

        private static Func<TItemToTest, bool> Combine<TItemToTest>(
            Func<TItemToTest, bool>[] innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AllReducer()
                => relatedItemCollection =>
                {
                    for (var i = 0; i < innerResults.Length; i++)
                    {
                        if (!innerResults[i](relatedItemCollection))
                        {
                            return false;
                        }
                    }

                    return true;
                };

            Func<TItemToTest, bool> AnyReducer()
                => relatedItemCollection =>
                {
                    for (var i = 0; i < innerResults.Length; i++)
                    {
                        if (innerResults[i](relatedItemCollection))
                        {
                            return true;
                        }
                    }

                    return false;
                };

            return @operator.Match(AllReducer, AnyReducer);
        }

        private static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);
    }
}