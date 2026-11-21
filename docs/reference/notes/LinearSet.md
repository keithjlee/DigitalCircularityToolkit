`LinearSet` turns a list of [`LinearObject`](@ref comp-LinearObject)s into flat, per-instance lists ready for feature vectors and matching. Each object is repeated `Quantity` times in a row, so an object with `Quantity` 3 produces three identical consecutive entries in every expanded output. The number of entries is the sum of all quantities.

**Outputs per instance.**

- `EffectiveLine`, `EffectiveLength`: the buffered line and its length, `LengthBuffer × Length`, measured along `PCA1` (see [`LinearObject`](@ref comp-LinearObject)).
- `Area`: the given or estimated cross-section area.
- `CentroidPlane`: a plane at the center of the object's PCA-aligned bounding box with X = `PCA2` and Y = `PCA3`, so its Z axis is parallel to `PCA1` (the plane of the cross-section).
- `Centroids`: the center of the PCA-aligned bounding box (the midpoint of `EffectiveLine`), not the PCA centroid reported by [`ObjectProperties`](@ref comp-ObjectProperties).
- `Hulls`: the planar convex hull of each object's samples in its local XY plane.
- `Indexer`: for each entry, the index of the source object in the input list. Use it to map a matched instance back to its original object or geometry (for example with List Item).
- `IDs`: the object `ID` for each entry.

`Objects` is not expanded: it returns the input list unchanged.

**Workflow.** Build one `LinearSet` for the demand (design members) and one for the supply (inventory). Feed per-instance values such as `EffectiveLength` and `Area` into [`FeatureVector`](@ref comp-FeatureVector), compute a cost matrix with [`EuclideanDistance`](@ref comp-EuclideanDistance) or [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), and solve it with [`Match`](@ref comp-Match). `Centroids` of both sets go into [`MatchLines`](@ref comp-MatchLines) to draw the assignment.

**Notes**

- Null entries in the input are skipped with the warning "N null object(s) ignored"; `Indexer` still refers to positions in the original input list.
- An object with `Quantity` ≤ 0 contributes no entries.
