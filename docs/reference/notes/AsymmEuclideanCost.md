Builds a [cost matrix](@ref comp-CostMatrix) for [`Assign`](@ref comp-Assign) that treats supply larger than the demand differently from supply that falls short, for example a reclaimed member that can be cut to length but not extended. `Demand` and `Supply` are trees with one branch per object, each holding its feature vector (the layout of [`FeatureVector`](@ref comp-FeatureVector)). For demand `i` and supply `j`, with `dist` the Euclidean distance between their vectors:

```math
\mathrm{cost}_{ij} = \begin{cases} \mathrm{dist}_{ij}^{\,p} & \text{if } s_{jk} \ge d_{ik} \text{ for every feature } k \\ \mathrm{dist}_{ij}^{\,q} & \text{otherwise (or } \infty \text{ with ForbidShortfall)} \end{cases}
```

with `p` = `Power1` (default 1) and `q` = `Power2` (default 2). If the supply is smaller in even one feature, the whole distance is raised to `Power2`.

**ForbidShortfall.** Set it to true to make undersized supply impossible instead of expensive: those pairs get cost ∞ and [`Assign`](@ref comp-Assign) never uses them. `Power2` is then ignored. Demand items for which no supply is large enough stay unassigned (`-1`); Assign reports how many.

Costs are exact decimal numbers. Unlike the old [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), nothing is rounded up, so the penalty also works for distances below 1. A larger `Power2` still makes a distance below 1 *smaller*: scale the features (or use ForbidShortfall) when distances can be small.

**Notes**

- The order matters: `Demand` is the "from" set and `Supply` the "to" set.
- With one feature (e.g. length) and `Power1`, `Power2` ≥ 1, or with ForbidShortfall, [`Assign`](@ref comp-Assign) solves by sorting, in milliseconds for thousands of pieces.
- Remark when `Power2` ≤ `Power1` without ForbidShortfall (undersized supply is then not penalized more). Errors: empty or ragged feature sets, non-finite values, a power ≤ 0.
