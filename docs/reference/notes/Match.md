Solves the linear assignment problem: it pairs demand items with supply items so that the sum of the costs of all pairs is as small as possible. The solver is LAPJV (Jonker-Volgenant, via the LapjvCSharp library), which finds the exact optimum.

`DistanceMatrix` is a tree with one branch per demand item and, in every branch, one cost per supply item: item `j` of branch `i` is the cost of assigning supply `j` to demand `i`. This is the output of [`EuclideanDistance`](@ref comp-EuclideanDistance), [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance) and [`ManhattanDistance`](@ref comp-ManhattanDistance). Branches are read in order; their path names do not matter. `Assignment` has one entry per demand item: `A[i] = j` means supply `j` goes to demand `i`. `Cost` is the sum of the costs of the assigned pairs.

Each supply item is used at most once. An inventory item with a quantity greater than one must therefore appear once per piece; the Set components ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)) expand objects by quantity, and their `Indexer` output maps a supply index back to the original object. With more supply than demand, the unused supply items do not appear in `Assignment`. With more demand than supply, the solver chooses which demand items stay unassigned so that the cost of the assigned pairs is minimal; those items get `-1`, and `Cost` leaves them out.

Downstream, use `Assignment` with List Item on the supply list (objects, centroids, geometry), draw the pairs with [`MatchLines`](@ref comp-MatchLines), and place supply objects with [`AlignToObject`](@ref comp-AlignToObject).

**Notes**

- `AutoRun` (default true) solves on every change. When it is false, the component keeps outputting the last computed result without re-solving, even if `DistanceMatrix` has changed; set it to true, for example with a button, to re-solve. The stored result is not saved with the file, so after reopening a file with `AutoRun` false there is no output until it runs once (remark: "AutoRun is false and no assignment has been computed yet. Set AutoRun to true (e.g. press the button) to run.").
- Remarks when demand exceeds supply: "|demand| > |supply|, not all demands will be assigned a real supply item" and "N demand item(s) have no supply item: their Assignment is -1, and Cost excludes them". Check for `-1` before indexing: Grasshopper's List Item with Wrap on turns `-1` into the last item.
- Errors: "DistanceMatrix is empty"; a branch with a different number of items than branch 0; a branch containing a null cost.
- `Cost` is the sum of the integer costs in `DistanceMatrix`, so it carries the rounding and scaling of the distance component that produced them.
