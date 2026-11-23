# Feature vectors and cost matrices

Matching needs a number for every possible pairing: how bad is it to use supply piece `j` for demand part `i`? DCT builds that number in two steps. A **feature vector** describes each piece by the properties that matter, and a **distance** between a demand vector and a supply vector becomes the **cost** of the pairing.

## Feature vectors

A feature vector is a short list of numbers per piece, for example `[EffectiveLength]` for truss members or `[EffectiveLength, EffectiveWidth]` for panels. [`FeatureVector`](@ref comp-FeatureVector) assembles them: each input is one property for all pieces, and the output has one branch per piece:

```
{0}: L0, W0
{1}: L1, W1
...
```

Build one feature vector for the demand and one for the supply, with the same properties in the same input order. Distances compare the vectors position by position.

Useful features:

- effective dimensions from the Set components (length, width, height, thickness, radius, area);
- shape descriptors from [`HarmonicAnalysisReal`](@ref comp-HarmonicAnalysisReal) and [`HarmonicAnalysisComplex`](@ref comp-HarmonicAnalysisComplex) (see [Shape descriptors](shape-descriptors.md)), which already output one branch per object;
- any other per-piece numbers computed in Grasshopper, such as a structural demand from an analysis plugin.

## Cost components

| Component | Cost for demand `d` and supply `s` | Use when |
|:----------|:-----------------------------------|:---------|
| [`EuclideanCost`](@ref comp-EuclideanCost) | `‖s − d‖^Power` | Pieces should be as close as possible in every feature; over- and undersize are equally bad. |
| [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost) | `‖s − d‖^Power1` if `s ≥ d` in every feature, else `‖s − d‖^Power2`, or forbidden | Supply can be cut down but not extended: too-small pieces cost more, or (with `ForbidShortfall`) are never used. |
| [`ManhattanCost`](@ref comp-ManhattanCost) | `Σ |s − d|` | Differences should add up linearly. |

Each produces one **[`CostMatrix`](@ref comp-CostMatrix)**: one row per demand piece, one column per supply piece. That is the input of [`Assign`](@ref comp-Assign). The matrix travels as a single item, so even thousands of pieces on each side move between components without delay; [`CostMatrixToTree`](@ref comp-CostMatrixToTree) shows the individual numbers when you want to inspect them.

## Exact costs and forbidden pairs

Costs are decimal numbers, computed without rounding, and powers can be decimals too (`Power` 1.5). A cost can also be **∞**, which means "never assign this pair". [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost) with `ForbidShortfall` produces ∞ for every supply piece that is smaller than the demand in some feature; [`Assign`](@ref comp-Assign) then serves as many demand parts as possible with allowed pairs and marks the rest `-1`.

Scaling still matters when features have different units: a length in millimetres dominates a thickness ratio in a Euclidean distance. Scale features of different kinds before combining them, with [`PairNormalize`](@ref comp-PairNormalize) (one shared maximum for a demand list and a supply list) or [`Normalize`](@ref comp-Normalize). A single feature, such as length alone, needs no scaling.

## Before v0.6: integer costs

The earlier components ([`EuclideanDistance`](@ref comp-EuclideanDistance), [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), [`ManhattanDistance`](@ref comp-ManhattanDistance), with [`Match`](@ref comp-Match)) are hidden from the toolbar but keep working in saved files. They output a tree of integers: every cost is rounded **up** to a whole number, so any distance between 0 and 1 becomes 1 and features had to be scaled up (e.g. `PairNormalize` with `Factor` 100) for the solver to tell pieces apart. To upgrade a definition, replace the distance component with its `…Cost` counterpart and `Match` with `Assign`; the inputs are the same.
