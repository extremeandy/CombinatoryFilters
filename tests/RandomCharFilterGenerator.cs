using System;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class RandomCharFilterGenerator
    {
        private readonly Random _random;

        public RandomCharFilterGenerator(Random random)
        {
            _random = random;
        }

        public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public IFilterNode<CharFilter> GetRandomFilter(int nodeCount)
        {
            if (nodeCount == 0)
            {
                return FilterNode<CharFilter>.True;
            }

            IFilterNode<CharFilter> GetRandomFilterInner(bool isInitial, bool parentIsInvert)
            {
                int GetFilterType()
                {
                    if (isInitial)
                    {
                        return 0;
                    }

                    while (true)
                    {
                        var result = _random.Next(7);
                        if (result == 1 && parentIsInvert)
                        {
                            continue;
                        }

                        return result;
                    }
                }

                var filterType = GetFilterType();
                switch (filterType)
                {
                    case 0:
                        var numChildren = Math.Max(_random.Next(4), 3);
                        var children = Enumerable.Range(0, numChildren)
                            .Select(_ => GetRandomFilterInner(false, false));

                        nodeCount = Math.Max(nodeCount - numChildren, 0);

                        var combinationOperator = _random.Next(2) == 0
                            ? CombinationOperator.All
                            : CombinationOperator.Any;

                        return new CombinationFilter<CharFilter>(children, combinationOperator);
                    case 1:
                        nodeCount--;
                        return new InvertedFilter<CharFilter>(GetRandomFilterInner(false, true));
                    default:
                        nodeCount--;
                        return new CharFilter(GetRandomChar());
                }
            }

            return GetRandomFilterInner(true, false);
        }

        public string GetRandomString(int length)
        {
            return new string(Enumerable.Range(0, length).Select(_ => GetRandomChar()).ToArray());
        }

        public char GetRandomChar()
        {
            return Chars[_random.Next(Chars.Length)];
        }
    }
}