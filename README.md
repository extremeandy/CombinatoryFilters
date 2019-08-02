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
var filter5To10Or8To15 = new CombinationFilter<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Or);
var filter9To12 = new NumericRangeFilter(9, 12);
var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10Or8To15, filter9To12 }, CombinationOperator.And);
```

#### Inversion

Any filter can be inverted using `.Invert()`.

### Testing a single value

You can test a single value as follows:

```csharp
var filter = new NumericRangeFilter(5, 10);
var isMatch = filter.IsMatch(7);
```

### Preserving ordering of filters

`CombinationFilter` stores filters as an `IImmutableSet`. If you wish to preserve the order of your filters, use `OrderedCombinationFilter` instead.

## Advanced usage

`IFilterNode<>` supports `Map`, `Match` and `Aggregate` for mapping and reducing filters. 

### `Map` usage

In this example, we reduce the range of the leaf node filters by increasing the lower bound by `1` and decreasing the upper bound by `1`. The structure of all the `And`, `Or` and `Invert` operations remains unchanged.

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
