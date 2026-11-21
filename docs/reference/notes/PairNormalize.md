Rescales two lists of numbers by one shared value, so that the largest value across both lists becomes `Factor`. The shared maximum is `M = max(max(A), max(B))`, and every value in both lists is multiplied by `Factor / M`:

```math
a_i' = a_i \cdot \frac{F}{M}, \qquad b_j' = b_j \cdot \frac{F}{M}
```

`Normalizer` outputs `M` itself (not the multiplier), so an original value can be recovered as `v' * M / Factor`.

**Why normalize before building a cost matrix.** The distance components ([`EuclideanDistance`](@ref comp-EuclideanDistance), [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), [`ManhattanDistance`](@ref comp-ManhattanDistance)) round every distance up to the next integer before [`Match`](@ref comp-Match) solves the assignment. Features with small values (for example lengths in meters between 0 and 1) mostly round up to the same cost of 1 and can no longer be told apart, and features of very different magnitude are weighted unequally, because the largest one dominates the distance. Scaling each feature to 0–`Factor` addresses both; with the default of 100, one cost unit is 1% of the largest value.

**Why a pair.** A cost matrix compares demand feature vectors with supply feature vectors, so the same feature must be on the same scale on both sides. Feed the demand values into `A` and the supply values into `B` (for example the `Length` of the designed members and of the reclaimed stock); both are divided by the same maximum, which keeps their distances meaningful. Two separate [`Normalize`](@ref comp-Normalize) components would each use their own maximum and distort the comparison. Send `NormalizedA` and `NormalizedB` to the matching inputs of the demand and supply [`FeatureVector`](@ref comp-FeatureVector) components, using one PairNormalize per feature.

**Notes**

- The divisor is the largest value, not the largest absolute value. If the shared maximum is not positive, the component stops with the error "The largest value is ...; normalizing needs at least one positive value".
