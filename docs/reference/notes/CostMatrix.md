`CostMatrix` is the parameter type that carries a cost matrix from a cost component ([`EuclideanCost`](@ref comp-EuclideanCost), [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost), [`ManhattanCost`](@ref comp-ManhattanCost)) to [`Assign`](@ref comp-Assign). It is not on the toolbar.

- A matrix has one row per demand item and one column per supply item. Costs are decimal numbers; `∞` marks a pair that may never be assigned.
- It travels as one item, not as a tree of numbers, so even 2000 × 2000 matrices move between components without delay. Panels and tooltips show its size and the share of forbidden pairs, e.g. `CostMatrix 120×340 (12% forbidden)`.
- To see the individual costs, use [`CostMatrixToTree`](@ref comp-CostMatrixToTree).
- Matrices cannot be internalized; they are rebuilt from the feature vectors.
