# Matching

Given a [cost matrix](cost-matrices.md), [`Assign`](@ref comp-Assign) finds the assignment with the lowest total cost: every demand part gets a different supply piece, and no piece is used twice. This is the linear assignment problem. DCT solves it exactly with a shortest augmenting path solver (Crouse 2016, the algorithm of SciPy's `linear_sum_assignment`): 2000 × 2000 pieces in about 0.1 s.

**Length matching is instant.** When the cost comes from a single feature with a convex cost (length alone, with [`EuclideanCost`](@ref comp-EuclideanCost), [`ManhattanCost`](@ref comp-ManhattanCost) or [`AsymmEuclideanCost`](@ref comp-AsymmEuclideanCost)), the best assignment never crosses: shorter members get shorter pieces. Assign detects this and solves by sorting, in milliseconds even for thousands of pieces.

## Reading the result

`Assignment` has one entry per demand part: `Assignment[i] = j` means supply piece `j` (an index into the expanded supply Set) goes to demand part `i`. `Cost` is the total cost of the assigned pairs, and `Unassigned` lists the demand parts that got nothing.

- **More supply than demand** (the usual case): the unused pieces simply do not appear in `Assignment`.
- **More demand than supply**: some demand parts cannot be served. Their entry is `-1`, `Cost` leaves them out, and the component says how many. The solver chooses which parts to leave out so that the cost of the rest is as low as possible.
- **Forbidden pairs** (cost ∞): Assign first serves as many demand parts as possible with allowed pairs, then minimizes the cost. Parts that no remaining piece may serve get `-1`.

## Using the result

1. **Draw the assignment:** [`MatchLines`](@ref comp-MatchLines) draws a line from each demand centroid to its assigned supply centroid. Unassigned parts get an empty line.
2. **Place the matched pieces:** pick the supply objects with Grasshopper's **List Item** (the Set's `Objects` output as the list, `Assignment` as the index), then move each one onto its demand object with [`AlignToObject`](@ref comp-AlignToObject). The move is rigid, so a longer piece overhangs its member equally at both ends, showing the material to be cut.
3. **Trace back to the inventory:** the supply Set's `Indexer` and `IDs`, indexed by `Assignment`, tell which inventory item each part uses.

!!! warning "Check for -1"
    Grasshopper's List Item wraps negative indices by default, so `-1` silently picks the last item. When demand can exceed supply, filter out the `-1` entries before List Item, or turn off its Wrap option.

## Running on demand

Very large problems can be slow to re-solve on every change. Set `AutoRun` to false (for example with a Button) to solve only when you ask. The last result is kept until the next run, but it is not saved with the file.

## Good assignments

The assignment is only as good as the cost matrix. If the result looks wrong, look at the costs first: features that are not normalized, an asymmetric metric with the wrong demand/supply order, or, with the pre-v0.6 components, integer rounding that makes many costs equal (see [Feature vectors and cost matrices](cost-matrices.md)).

## Before v0.6: Match

[`Match`](@ref comp-Match) does the same on the integer cost trees of the pre-v0.6 distance components. It is hidden from the toolbar but works in saved files, and since v0.6 it uses the same solver as Assign. The library it used before (LapjvCSharp) sometimes returned slightly suboptimal assignments on rectangular problems, so reopened files can show a slightly lower `Cost` and different pairs.
