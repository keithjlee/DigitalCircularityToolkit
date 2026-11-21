Builds a cost matrix for [`Match`](@ref comp-Match) that penalizes supply items that are too small. `Demand` and `Supply` are trees with one branch per object, each branch holding that object's feature vector (the layout of [`FeatureVector`](@ref comp-FeatureVector)). For each pair the component computes the per-feature differences `s - d` (supply minus demand) and the Euclidean distance over all of them:

```math
\mathrm{dist}_{ij} = \lVert s_j - d_i \rVert_2, \qquad
\mathrm{cost}_{ij} = \begin{cases} \lceil \mathrm{dist}_{ij}^{\,p} \rceil & \text{if } s_{jk} \ge d_{ik} \text{ for every feature } k \\ \lceil \mathrm{dist}_{ij}^{\,q} \rceil & \text{otherwise} \end{cases}
```

with `p` = `Power1` and `q` = `Power2`. A pair is "valid" when the supply item is at least as large as the demand in every feature, for example a reclaimed member that is at least as long as the required one and can be cut down. Valid pairs use `Power1` (default 1); if the supply is smaller in even one feature, the whole distance, including the features where supply is larger, is raised to `Power2` (default 2). The result is rounded up to an integer (`Math.Ceiling`). `DistanceMatrix` has one branch per demand item, paths `{0}`, `{1}`, ..., and item `j` of branch `i` is the cost of supply `j` for demand `i`.

**Scaling.** The penalty works only when distances are larger than 1: for a distance below 1, raising it to `Power2` makes it smaller, and rounding up then gives a cost of 1 in both cases. Scale the features across demand and supply together, for example with [`PairNormalize`](@ref comp-PairNormalize) (default `Factor` 100), so typical distances are well above 1. See [`EuclideanDistance`](@ref comp-EuclideanDistance) for the effect of integer rounding.

**Notes**

- Too-small supply is penalized, not forbidden: if it is cheaper overall, [`Match`](@ref comp-Match) can still assign an invalid pair. Check the result.
- The order matters: `Demand` is the "from" set and `Supply` the "to" set; swapping them changes which pairs are valid.
- Error if the sets are empty or the vectors differ in length. Warning "Are you sure you want a power <= 0?" for `Power1` ≤ 0, and a remark when `Power2` ≤ `Power1` (undersized supply is then not penalized more).
- Costs saturate at 2147483647.
