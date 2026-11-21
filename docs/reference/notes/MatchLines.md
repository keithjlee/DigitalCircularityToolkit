Draws a line from each demand point to the supply point it was assigned. For each position `i` in `AssignmentIndices`, the line runs from `DemandPoints[i]` to `SupplyPoints[A[i]]`. The output list has one entry per demand point, in the same order, so it stays parallel to the demand list.

Connect `AssignmentIndices` to the `Assignment` output of [`Match`](@ref comp-Match). The points are usually object centroids: for demand, the centroids of the design elements; for supply, the `Centroids` output of a Set component ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)). `SupplyPoints` must be in the same order as the supply columns of the cost matrix, which means the expanded list from the Set component, not the original objects. The lines are a visual check of the matching, for example to see whether items come from a nearby part of the stock.

**Notes**

- Unassigned demand (index `-1`, when there is more demand than supply) gives a null entry at that position, with the remark "N demand point(s) are unassigned (index -1); their lines are null".
- Error if `DemandPoints` and `AssignmentIndices` have different lengths, or if an index other than `-1` is outside the range of `SupplyPoints`; no lines are output in either case.
