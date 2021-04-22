# CombinatoryFilters

> Functional filter abstraction for creating, applying, mapping, and reducing combinatory filter structures

## Installation

```
dotnet add package ExtremeAndy.CombinatoryFilters
```

## Usage

1. Define your filter interface(s) and/or class(es). Here's an example of a simple filter which checks whether an integer is between `UpperBound` and `LowerBound`.

```csharp
public class NumericRangeFilter : LeafFilterNode<NumericRangeFilter>, IRealisableLeafFilterNode<int>
{
    public NumericRangeFilter(int lowerBound, int upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public int LowerBound { get; }

    public int UpperBound { get; }

    public bool IsMatch(int item) => LowerBound <= item && item <= UpperBound;
}
```

2. Optionally implement `IEquatable<IFilterNode>` on your filter class. If this is not done, then calling `.Equals()` on an `IFilterNode` in your filter tree will default to reference equality when comparing your leaf filters.

3. Create an instance of your filter and apply it to some values

```csharp
var filter = new NumericRangeFilter(5, 10);
var values = new[] { 1, 3, 5, 9, 11 };
var expectedFilteredValues = new[] { 5, 9 };

var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
var filteredValues = values.Where(filterPredicate);

Assert.Equal(expectedFilteredValues, filteredValues);
```

### Complex filters

You can assemble arbitrarily complex filters as follows:

```csharp
var filter5To10 = new NumericRangeFilter(5, 10);
var filter8To15 = new NumericRangeFilter(8, 15);
var filter5To10Or8To15 = new CombinationFilter<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Any);
var filter9To12 = new NumericRangeFilter(9, 12);
var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12 }, CombinationOperator.All);
```

#### Inversion

Any filter can be inverted using `.Invert()`.

### Testing a single value

You can test a single value as follows:

```csharp
var filter = new NumericRangeFilter(5, 10);
var isMatch = filter.IsMatch(7);
```

However, `IsMatch` causes an allocation and is not recommended for testing many items. Instead, use `filter.GetPredicate`:

```csharp
var filter = new NumericRangeFilter(5, 10);
var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
var lotsOfIntegers = Enumerable.Range(0, 1000000);
var matches = lotsOfIntegers.Where(filterPredicate);
```

### Preserving ordering of filters

`CombinationFilter` defaults to storing filters as a `HashSet<T>`. If you wish to preserve the order of your filters, pass `preserveOrder: true` in the constructor of `CombinationFilter`.

## Advanced usage

`IFilterNode<>` supports `Map`, `Match` and `Aggregate` for mapping and reducing filters.

### `Map` usage

In this example, we reduce the range of the leaf node filters by increasing the lower bound by `1` and decreasing the upper bound by `1`. The structure of all the `All`, `Any` and `Invert` operations remains unchanged.

```csharp
var shortenedFilters = filter.Map(f =>
{
    var newLowerBound = f.LowerBound + 1;
    var newUpperBound = f.UpperBound - 1;
    return new NumericRangeFilter(newLowerBound, newUpperBound);
});
```

### `Aggregate` usage

In this example, we want to compute the length of the longest filter interval, or infinity if any filter is inverted.

```csharp
var longestIntervalLength = filter.Aggregate<double>(
    (lengths, _) => lengths.Max(),
    length => double.PositiveInfinity,
    f => f.UpperBound - f.LowerBound);
```

### `GetPartial` usage

`GetPartial` provides a way to compute a partial filter, which is a kind of subset of a filter. When applied, a partial filter is guaranteed to return a superset of the result that the original filter would have returned when applied. This is a special case of the `Relax` operation, where leaf nodes are maximally relaxed (i.e. replaced with `True`) if the predicate is satisfied.

This is useful for performing pre-filtering on an incomplete dataset that doesn't (yet) contain all the information required to apply the final filter.

This is normally quite a trivial problem, but when there are `InvertedFilter`s and `CombinationFilters` in the mix, computing the minimal partial filter is not intuitive or easy to demonstrate.

Here is a contrived example (_note: this doesn't do anything useful, just demonstrates usage_):

```csharp
// All the numbers from -5 to 10, excluding numbers from 2 to 6
var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[]
{
    new NumericRangeFilter(-5, 10),
    new InvertedFilter<NumericRangeFilter>(new NumericRangeFilter(2, 6)),
}, CombinationOperator.All);

// Exclude filters with negative values
var partialFilter = filter.GetPartial(f => f.LowerBound >= 0);

// Initially we only have positive numbers
var positiveValues = new[] { 1, 3, 5, 7, 12 };
var prefilteredValues = positiveValues.Where(partialFilter.GetPredicate<NumericRangeFilter, int>()).ToList();
Assert.Equal(new[] { 1, 7, 12 }, prefilteredValues);

// Now we include some additional values
var additionalValues = new[] { -7, -4, 11 };
var combinedValues = prefilteredValues.Concat(additionalValues);

// Finally we apply our 'full' filter
var finalValues = combinedValues.Where(filter.GetPredicate<NumericRangeFilter, int>());
Assert.Equal(new[] { 1, 7, -4 }, finalValues);
```

### `Relax` usage

`Relax` provides a way to relax a filter by relaxing its leaf nodes.

Example TBD.
