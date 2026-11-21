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

## Distance metrics

| Component | Cost for demand `d` and supply `s` | Use when |
|:----------|:-----------------------------------|:---------|
| [`EuclideanDistance`](@ref comp-EuclideanDistance) | `‖d − s‖^Power` | Pieces should be as close as possible in every feature; over- and undersize are equally bad. |
| [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance) | `‖d − s‖^Power1` if `s ≥ d` in every feature, else `‖d − s‖^Power2` | Supply can be cut down but not extended: too-small pieces get the higher power. |
| [`ManhattanDistance`](@ref comp-ManhattanDistance) | `Σ |d − s|` | Differences should add up linearly. |

Each produces a `DistanceMatrix`: a tree with one branch per demand piece and one cost per supply piece in every branch. That is the input of [`Match`](@ref comp-Match).

## Costs are integers

The distance components round every cost **up** to a whole number. This is a legacy of the original assignment solver, and it makes the scale of the features matter:

- Any distance between 0 and 1 becomes a cost of 1. Features in the range 0 to 1 (ratios, scores, lengths in meters for small parts) leave the solver little to distinguish.
- Features of very different magnitudes are weighted unequally: a length in millimetres dominates a thickness ratio.
- In [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), a higher `Power2` penalizes undersized pieces only when distances are larger than 1.

Scale the features before building feature vectors. [`PairNormalize`](@ref comp-PairNormalize) rescales a demand list and a supply list with one shared maximum, so that the largest value of the pair becomes `Factor` (default 100); use one per feature. [`Normalize`](@ref comp-Normalize) does the same for a single list. With millimetre lengths, as in the [examples](../workflows/linear.md), distances are already large and normalization is optional.
