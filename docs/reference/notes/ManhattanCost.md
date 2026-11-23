Builds a [cost matrix](@ref comp-CostMatrix) for [`Assign`](@ref comp-Assign) from the Manhattan (city-block) distance between demand and supply feature vectors: the sum of the absolute differences of the features,

```math
\mathrm{cost}_{ij} = \sum_k \lvert s_{jk} - d_{ik} \rvert .
```

Differences add up linearly, so one large difference counts no more than several small ones adding to the same total (Euclidean distance weights the large one more). Costs are exact decimal numbers; the old [`ManhattanDistance`](@ref comp-ManhattanDistance) rounded them up to integers.

**Notes**

- With one feature, [`Assign`](@ref comp-Assign) solves by sorting.
- Errors: empty or ragged feature sets, non-finite or null values.
