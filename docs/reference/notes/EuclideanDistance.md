!!! warning "Obsolete since v0.6"
    Replaced by [`EuclideanCost`](@ref comp-EuclideanCost). This component is hidden from the toolbar but still works in saved files.

Builds the cost matrix for [`Match`](@ref comp-Match) from two sets of feature vectors. `Demand` and `Supply` are trees with one branch per object, each branch holding that object's feature vector, which is the layout [`FeatureVector`](@ref comp-FeatureVector) produces. For every demand vector `d` and supply vector `s` the cost is

```math
\mathrm{cost}_{ij} = \left\lceil \lVert d_i - s_j \rVert_2^{\,p} \right\rceil
```

where `p` is `Power`: the Euclidean distance over all features is raised to `Power` and then rounded up to the next integer (`Math.Ceiling`). `DistanceMatrix` has one branch per demand item, with paths `{0}`, `{1}`, ..., and item `j` of branch `i` is the cost of assigning supply `j` to demand `i`. The metric is symmetric: swapping a demand and a supply vector gives the same cost. With `Power` 2 a large mismatch costs more relative to several small ones, so the assignment avoids single bad fits.

**Scaling.** Because costs are integers rounded up, the scale of the features decides the resolution. Any distance between 0 and 1 becomes a cost of 1, so features in the range 0 to 1 (scores, ratios) leave the solver almost nothing to distinguish. Features with very different magnitudes (a length in mm next to a score) let the large one dominate. Scale each feature across demand and supply together, for example with [`PairNormalize`](@ref comp-PairNormalize) (or [`Normalize`](@ref comp-Normalize) on a combined list), whose default `Factor` of 100 maps the largest value to 100.

**Notes**

- Error if the sets are empty or any vector has a different number of values than the first demand vector.
- Warning "Are you sure you want a power <= 0?" for `Power` ≤ 0.
- Costs saturate at the largest 32-bit integer (2147483647); large raw features with `Power` 2 or 3 can reach it, which makes those pairs indistinguishable.
- Lengths are in the Rhino document's units; DCT does not convert them.
