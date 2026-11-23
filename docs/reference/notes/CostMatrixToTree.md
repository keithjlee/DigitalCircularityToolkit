Shows a [cost matrix](@ref comp-CostMatrix) as a Grasshopper tree of numbers, for panels, charts or your own processing: branch `{i}` holds the costs of demand item `i`, one per supply item, in supply order. Forbidden pairs appear as `∞`.

A matrix travels between DCT components as a single item, which is fast; turning a large matrix into a tree creates rows × columns numbers (four million for 2000 × 2000), so use this component for inspection rather than in the main workflow.
