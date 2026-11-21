Rescales a list of numbers so that its largest value becomes `Factor`. Every value is multiplied by the same number, `Factor / max(Values)`:

```math
v_i' = v_i \cdot \frac{F}{\max_j v_j}
```

With the default `Factor` of 100, the largest value becomes 100 and the others become a percentage of it. Ratios between values are kept; the result is unitless.

**Why normalize before building a cost matrix.** The distance components ([`EuclideanDistance`](@ref comp-EuclideanDistance), [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), [`ManhattanDistance`](@ref comp-ManhattanDistance)) round every distance up to the next integer before [`Match`](@ref comp-Match) solves the assignment. Two consequences follow:

- Small values lose precision. If a feature is measured in meters and lies between 0 and 1, distances of 0.2 and 0.9 both become a cost of 1, and the solver cannot tell the candidates apart. Scaling the feature to 0–100 makes one cost unit equal to 1% of the largest value.
- Features of different magnitude are weighted unequally. In a feature vector holding a length in millimeters (thousands) and an aspect ratio (around 1), the length dominates every distance. Normalizing each feature to the same range gives them comparable weight; a different `Factor` per feature acts as a deliberate weight.

**Where it sits.** Normalize one property list (for example the `Length` output of [`ObjectProperties`](@ref comp-ObjectProperties) or [`LinearSet`](@ref comp-LinearSet)'s `EffectiveLength`) before it goes into an input of [`FeatureVector`](@ref comp-FeatureVector).

**Notes**

- Each call uses its own maximum. If demand and supply are normalized with two separate Normalize components, they are scaled by different numbers and their distances are no longer comparable. Use [`PairNormalize`](@ref comp-PairNormalize) to scale both by a shared maximum.
- The divisor is the largest value, not the largest absolute value. If it is not positive (all values zero or negative), the component stops with the error "The largest value is ...; normalizing needs at least one positive value".
