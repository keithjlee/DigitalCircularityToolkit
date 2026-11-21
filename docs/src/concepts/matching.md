# Matching

Given a cost for every demand–supply pair, [`Match`](@ref comp-Match) finds the assignment with the lowest total cost: every demand part gets a different supply piece, and no piece is used twice. This is the linear assignment problem. DCT solves it exactly with the Jonker–Volgenant algorithm (LAPJV), which solves problems with thousands of pieces in about a second.

## Reading the result

`Assignment` has one entry per demand part: `Assignment[i] = j` means supply piece `j` (an index into the expanded supply Set) goes to demand part `i`. `Cost` is the total cost of the assigned pairs.

- **More supply than demand** (the usual case): the unused pieces simply do not appear in `Assignment`.
- **More demand than supply**: some demand parts cannot be served. Their entry is `-1`, `Cost` leaves them out, and the component says how many. The solver chooses which parts to leave out so that the cost of the rest is as low as possible.

## Using the result

1. **Draw the assignment:** [`MatchLines`](@ref comp-MatchLines) draws a line from each demand centroid to its assigned supply centroid. Unassigned parts get an empty line.
2. **Place the matched pieces:** pick the supply objects with Grasshopper's **List Item** (the Set's `Objects` output as the list, `Assignment` as the index), then move each one onto its demand object with [`AlignToObject`](@ref comp-AlignToObject). The move is rigid, so a longer piece overhangs its member equally at both ends, showing the material to be cut.
3. **Trace back to the inventory:** the supply Set's `Indexer` and `IDs`, indexed by `Assignment`, tell which inventory item each part uses.

!!! warning "Check for -1"
    Grasshopper's List Item wraps negative indices by default, so `-1` silently picks the last item. When demand can exceed supply, filter out the `-1` entries before List Item, or turn off its Wrap option.

## Running on demand

Large problems can be slow to re-solve on every change. Set `AutoRun` to false (for example with a Button) to solve only when you ask. The last result is kept until the next run, but it is not saved with the file.

## Good assignments

The assignment is only as good as the cost matrix. If the result looks wrong, look at the costs first: features that are not normalized, an asymmetric metric with the wrong demand/supply order, or integer rounding that makes many costs equal (see [Feature vectors and cost matrices](cost-matrices.md)).
