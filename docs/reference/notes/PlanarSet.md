`PlanarSet` turns a list of [`PlanarObject`](@ref comp-PlanarObject)s into flat, per-instance lists ready for feature vectors and matching. Each object is repeated `Quantity` times in a row, so an object with `Quantity` 3 produces three identical consecutive entries in every expanded output. The number of entries is the sum of all quantities.

**Outputs per instance.**

- `EffectivePlane`: the buffered rectangular surface in the object's local plane (see [`PlanarObject`](@ref comp-PlanarObject)).
- `Plane`: the local plane, with X = `PCA1` and Y = `PCA2`, at the center of the PCA-aligned bounding box.
- `EffectiveLength`, `EffectiveWidth`: `LengthBuffer × Length` along `PCA1` and `WidthBuffer × Width` along `PCA2`.
- `Thickness`: the given `Thickness`, or the extent along `PCA3` when none was given. No buffer is applied.
- `Centroids`: the center of the PCA-aligned bounding box, not the PCA centroid reported by [`ObjectProperties`](@ref comp-ObjectProperties).
- `Hulls`: the planar convex hull of each object's samples in its local plane.
- `Indexer`: for each entry, the index of the source object in the input list. Use it to map a matched instance back to its original object or geometry.
- `IDs`: the object `ID` for each entry.

`Objects` is not expanded: it returns the input list unchanged.

**Workflow.** Build one `PlanarSet` for the demand and one for the supply. Feed per-instance values such as `EffectiveLength`, `EffectiveWidth` and `Thickness` into [`FeatureVector`](@ref comp-FeatureVector), compute a cost matrix (for example [`EuclideanDistance`](@ref comp-EuclideanDistance)) and solve it with [`Match`](@ref comp-Match). `Centroids` of both sets go into [`MatchLines`](@ref comp-MatchLines) to draw the assignment.

**Notes**

- Null entries in the input are skipped with the warning "N null object(s) ignored"; `Indexer` still refers to positions in the original input list.
- An object with `Quantity` ≤ 0 contributes no entries.
