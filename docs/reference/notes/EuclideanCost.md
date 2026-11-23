Builds a [cost matrix](@ref comp-CostMatrix) for [`Assign`](@ref comp-Assign) from the Euclidean distance between demand and supply feature vectors. `Demand` and `Supply` are trees with one branch per object, each holding that object's feature vector (the layout of [`FeatureVector`](@ref comp-FeatureVector)). Row `i` of the matrix is demand `i`, column `j` is supply `j`:

```math
\mathrm{cost}_{ij} = \lVert s_j - d_i \rVert_2^{\,p}
```

with `p` = `Power` (default 1, any number greater than 0). Costs are exact decimal numbers; nothing is rounded, so features do not need to be scaled up to avoid rounding (the old [`EuclideanDistance`](@ref comp-EuclideanDistance) rounded every cost up to an integer). Over- and undersized supply are penalized equally; use [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost) when supply can be cut down but not extended.

**Notes**

- With one feature and `Power` ≥ 1, [`Assign`](@ref comp-Assign) solves the problem by sorting (see [Matching](../concepts/matching.md)), which takes milliseconds even for thousands of pieces.
- Errors: empty sets, feature vectors of different lengths, a non-finite or null value, `Power` ≤ 0.
- A cost too large for a double becomes ∞ (that pair can never be assigned).
