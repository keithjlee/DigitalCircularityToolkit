Builds a cost matrix for [`Match`](@ref comp-Match) from the Manhattan (L1) distance between feature vectors: the sum of the absolute differences of all features, rounded up to an integer.

```math
\mathrm{cost}_{ij} = \left\lceil \textstyle\sum_k \lvert d_{ik} - s_{jk} \rvert \right\rceil
```

`Demand` and `Supply` are trees with one branch per object, each holding that object's feature vector, as produced by [`FeatureVector`](@ref comp-FeatureVector). `DistanceMatrix` has one branch per demand item, and item `j` of branch `i` is the cost of supply `j` for demand `i`. Unlike [`EuclideanDistance`](@ref comp-EuclideanDistance), each feature contributes in proportion to its difference, so one large mismatch is not weighted more than several small ones that add up to the same amount. There is no power input.

Use it where differences in separate features should simply add up, for example total offcut length over several dimensions. The same scaling advice applies as for the other distance matrices: costs are integers rounded up, so scale features across demand and supply together (for example with [`PairNormalize`](@ref comp-PairNormalize), default `Factor` 100) so that differences are well above 1 and no single feature dominates by its units.

**Notes**

- The output branches are numbered `{0}`, `{1}`, ... like the Euclidean components. (Before v0.5.1 they reused the paths of the `Demand` tree; [`Match`](@ref comp-Match) reads branches in order, so either works.)
- Error if the sets are empty or the vectors differ in length.
- Since v0.5 the sum is rounded up like the other metrics; earlier versions rounded down.
