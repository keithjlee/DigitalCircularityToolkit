Solves the linear assignment problem on a [cost matrix](@ref comp-CostMatrix): it pairs demand items (rows) with supply items (columns) so that the total cost of the pairs is as low as possible, with every supply item used at most once. The result is the exact optimum.

- `Assignment` has one entry per demand item: `A[i] = j` means supply `j` serves demand `i`; `-1` means demand `i` gets nothing.
- `Cost` is the sum of the costs of the assigned pairs.
- `Unassigned` lists the demand items with `-1`.

**Who stays unassigned.** With more demand than supply, the leftover demand items get `-1`, chosen so that the cost of the rest is minimal. With forbidden pairs (cost ∞, e.g. [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost) with `ForbidShortfall`), Assign first serves as many demand items as possible, then minimizes the cost; items that cannot be served by any remaining supply get `-1`. It never fails because of forbidden pairs.

**Speed.** When the matrix comes from one feature with a convex cost (the length-matching case: [`EuclideanCost`](@ref comp-EuclideanCost) with `Power` ≥ 1, [`ManhattanCost`](@ref comp-ManhattanCost), or [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost) with powers ≥ 1 or ForbidShortfall), the optimal assignment never "crosses": shorter demand gets shorter supply. Assign then sorts both sides instead of running the general solver, which takes milliseconds for thousands of pieces, and adds a remark saying so. The general solver is the shortest augmenting path method (Crouse 2016, the algorithm behind SciPy's `linear_sum_assignment`).

The Set components ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)) expand inventory by quantity before the cost matrix is built, so a supply index refers to one physical piece; their `Indexer` output maps it back to the inventory object.

**Notes**

- `AutoRun` (default true) solves on every change. When false, the last result is kept until AutoRun is true again (wire a Button for large problems). The result is not saved with the file.
- Use `Assignment` with List Item on the supply Set's `Objects` (with **Wrap off**, so `-1` gives null rather than the last item), [`MatchLines`](@ref comp-MatchLines) to draw the pairs, and [`AlignToObject`](@ref comp-AlignToObject) to place them.
- Ties: when several assignments have the same lowest cost, Assign always returns the same one for the same matrix.
