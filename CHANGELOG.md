# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project (will try to) adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 4.0.0

Some major changes took place in this release to simplify things that were just way too complicated. Previously, leaf nodes were actually filters, i.e. they inherited from `ILeafFilterNode` and *also* implemented the filter logic.

E.g.

```csharp
public class CharFilter : LeafFilterNode<CharFilter>, IRealisableLeafFilterNode<string>, IEquatable<IFilterNode>

...

public bool IsMatch(string item) => ...
```

In this release, leaf nodes don't have an inheritance relationship with filter implementations; there is now a composition relationship. That is, leaf node "has" a filter, not "is" a filter.

This means we could safely remove all the generic constraints which were so confusing and just not working well in a language (c#) that doesn't support higher-kinded types.

Refer to the [README.md](README.md) for updated example usage. For an example on how to migrate, look at what changed in the tests in the commit tagged [version/4.0.0](https://github.com/extremeandy/CombinatoryFilters/blob/version/4.0.0), e.g. [CharFilter.cs](https://github.com/extremeandy/CombinatoryFilters/blob/version/4.0.0/tests/CharFilter.cs).

### Added

- Added `Filter<T>` class which provides a convenient base class for implementing realisable filters
- Added `IEquatable` to `IFilterNode`
- Added `Collapse` method to `IFilterNode`
- Added `IsCollapsed` bool to `IFilterNode` to indicate whether or not a filter has already been collapsed.
  - Calling `Collapse()` when this is `true` will have no effect and simply return the already collapsed node.
- Added `Sort` method to `IFilterNode<TFilter>` which recursively sorts nodes according to the supplied `IComparer<IFilterNode<TFilter>>`.
  - There is also a default comparer, `FilterNodeComparer`. The order of nodes of different types is leaf, inverted, combination.
  - When sorting adjacent leaf nodes, it will use the supplier `IComparer<TFilter>`, or `Comparer<TFilter>.Default` if none is specified (which means `TFilter` would need to implement `IComparable<TFilter>`, or a run-time exception would be thrown when calling `Sort()`)
  - When sorting adjacent combination nodes, it will use lexicographical sequence order
  - When sorting adjacent inverted nodes, it will sort by the inner nodes to be inverted

### Changed

- Renamed some classes/interfaces:
  | Before                      | After                    |
  | --------------------------- | ------------------------ |
  | `CombinationFilter`         | `CombinationFilterNode`  |
  | `ICombinationFilter`        | `ICombinationFilterNode` |
  | `InvertedFilter`            | `InvertedFilterNode`     |
  | `IInvertedFilter`           | `IInvertedFilterNode`    |
  | `IRealisableLeafFilterNode` | `IFilter`                |
- Changed `LeafFilterNode` to `sealed` and added `Filter` property
- Changed storage in `CombinationFilterNode` to always be an array, and removed `preserveOrder` from the constructor.
  - This means nodes are always ordered, and this will affect equality.
  - There are two ways to resolve this:
    1. Call `Sort()` on nodes before calling `Equals()`
    2. Use `IsEquivalentTo` instead of `Equals()`, which will internally use a `SetEquals` operation
- Changed the type of the leaf parameter of `Aggregate` and `Match` from `Func<TFilter, TResult>` to `Func<ILeafFilterNode<TFilter>, TResult>`
- Moved methods `IsTrue()` and `IsFalse()` from `IFilterNode<TFilter>` to `IFilterNode`:
- Moved extension method `IsEquivalentTo` into interface `IFilterNode`

### Removed

- Removed `InternalFilterNode` and `IInternalFilterNode`
- Removed convoluted/confusing generic constraints

## 3.0.0

### Changed

- Use arrays in some functions like `Aggregate` so that we can benefit from using `for` loops without needing to allocate iterators or lambdas to combine the results
- Renamed `ICombinationFilterNode` to simply `ICombinationFilter`
- Removed `OrderedCombinationFilter` and added new parameter `PreserveOrder` to constructor of `CombinationFilter`.

### Fixed

- [3.0.2] Collapsing a `CombinationFilter` should cause any inner `CombinationFilter`s of the same `Operator` to be flattened onto the outer `CombinationFilter`
- [3.0.3] Collapsing a `CombinationFilter` should cause any inner `CombinationFilter`s of the opposite `Operator` to be absorbed if the inner filters contains one of the outer filters