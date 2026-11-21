`BoxSet` turns a list of [`BoxObject`](@ref comp-BoxObject)s into flat, per-instance lists ready for feature vectors and matching. Each object is repeated `Quantity` times in a row, so an object with `Quantity` 3 produces three identical consecutive entries in every expanded output. The number of entries is the sum of all quantities.

**Outputs per instance.**

- `EffectiveBox`: the PCA-aligned bounding box scaled about its center by the buffers (see [`BoxObject`](@ref comp-BoxObject)).
- `Plane`: the local plane, with X = `PCA1` and Y = `PCA2`, at the center of the box.
- `EffectiveLength`, `EffectiveWidth`, `EffectiveHeight`: the box extents along `PCA1`, `PCA2` and `PCA3`, each multiplied by its buffer.
- `Centroids`: the center of the PCA-aligned bounding box, not the PCA centroid reported by [`ObjectProperties`](@ref comp-ObjectProperties).
- `Hulls`: the 3D convex hull of each object's sample points, as a mesh.
- `Indexer`: for each entry, the index of the source object in the input list. Use it to map a matched instance back to its original object or geometry.
- `IDs`: the object `ID` for each entry.

`Objects` is not expanded: it returns the input list unchanged.

**Workflow.** Build one `BoxSet` for the demand and one for the supply. Feed per-instance dimensions into [`FeatureVector`](@ref comp-FeatureVector), compute a cost matrix (for example [`EuclideanDistance`](@ref comp-EuclideanDistance)) and solve it with [`Match`](@ref comp-Match). `Centroids` of both sets go into [`MatchLines`](@ref comp-MatchLines) to draw the assignment.

**Notes**

- `Hulls` is computed only when it is connected or the component preview is on; the hulls are then built in parallel. A hull is an empty mesh when an object's samples are collinear or coplanar.
- Null entries in the input are skipped with the warning "N null object(s) ignored"; `Indexer` still refers to positions in the original input list.
- An object with `Quantity` ≤ 0 contributes no entries.
