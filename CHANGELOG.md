# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project (will try to) adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 3.0.0

### Changed

- Use arrays in some functions like `Aggregate` so that we can benefit from using `for` loops without needing to allocate iterators or lambdas to combine the results
- Renamed `ICombinationFilterNode` to simply `ICombinationFilter`
- Removed `OrderedCombinationFilter` and added new parameter `PreserveOrder` to constructor of `CombinationFilter`.

### Fixed

- [3.0.1] fix: Collapsing a `CombinationFilter` should cause any inner `CombinationFilter`s of the same `Operator` to be flattened onto the outer `CombinationFilter`