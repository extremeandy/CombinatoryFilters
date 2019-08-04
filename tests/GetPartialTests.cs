using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class GetPartialTests
    {
        private readonly Random _random;
        private readonly RandomCharFilterGenerator _generator;

        public GetPartialTests()
        {
            _random = new Random();
            _generator = new RandomCharFilterGenerator(_random);
        }

        [Fact]
        public void GetPartial_Example()
        {
            // All the numbers from -5 to 10, EXCEPT numbers from 2 to 6
            var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[]
            {
                new NumericRangeFilter(-5, 10),
                new InvertedFilter<NumericRangeFilter>(new NumericRangeFilter(2, 6)), 
            }, CombinationOperator.All);

            // Exclude filters with negative values
            var partialFilter = filter.GetPartial(f => f.LowerBound >= 0);

            var positiveValues = new[] {1, 3, 5, 7, 12};
            var prefilteredValues = positiveValues.Where(partialFilter.IsMatch).ToList();
            Assert.Equal(new[] { 1, 7, 12 }, prefilteredValues);

            var additionalValues = new[] { -7, -4, 11 };
            var combinedValues = prefilteredValues.Concat(additionalValues);

            var finalValues = combinedValues.Where(filter.IsMatch);
            Assert.Equal(new[] { 1, 7, -4 }, finalValues);
        }

        [Fact]
        public void GetPartial_FiltersSubset()
        {
            bool IsComplex(IFilterNode<CharFilter> filter)
            {
                bool IsComplexInner(IFilterNode<CharFilter> inner, int invertedCount)
                {
                    if (invertedCount > 1)
                    {
                        return true;
                    }

                    return inner.Match(
                        combine => combine.Filters.Any(f => IsComplexInner(f, invertedCount)),
                        invert => IsComplexInner(invert.FilterToInvert, invertedCount + 1),
                        leaf => false);
                }

                return IsComplexInner(filter, 0);
            }

            var scenarioToCount = new Dictionary<Func<IFilterNode<CharFilter>, bool>, int>
            {
                {f => true, 100}, // Any scenario
                {IsComplex, 10},
            };

            while (scenarioToCount.Any(kvp => kvp.Value > 0))
            {
                var numSets = _random.Next(1000);

                var strings = Enumerable.Range(1, numSets).Select(_ => _generator.GetRandomString(_random.Next(RandomCharFilterGenerator.Chars.Length)))
                    .ToHashSet();

                const int nodeCount = 10;

                var filter = _generator.GetRandomFilter(nodeCount);

                var charsToExclude = Enumerable.Range(0, _random.Next(nodeCount))
                    .Select(_ => _generator.GetRandomChar())
                    .ToHashSet();
                var partial = filter.GetPartial(f => !charsToExclude.Contains(f.Character));

                var partialResult = strings.Where(s => partial.IsMatch(s))
                    .ToHashSet();
                var finalResult = partialResult.Where(s => filter.IsMatch(s))
                    .ToHashSet();

                if (partialResult.Count == strings.Count || finalResult.Count == partialResult.Count)
                {
                    // Trivial, ignore.
                    continue;
                }

                var originalResult = strings.Where(s => filter.IsMatch(s))
                    .ToHashSet();

                Assert.Equal(originalResult, finalResult);

                foreach (var kvp in scenarioToCount.ToList())
                {
                    if (kvp.Key(filter))
                    {
                        scenarioToCount[kvp.Key]--;
                    }
                }
            }
        }
    }
}
