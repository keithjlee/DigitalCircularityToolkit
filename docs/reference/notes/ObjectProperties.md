`ObjectProperties` unpacks the analysis stored in a design object. It accepts any DCT object: the output of [`Object`](@ref comp-Object), [`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject) or [`SphericalObject`](@ref comp-SphericalObject), including objects moved or reoriented by [`OverridePCA`](@ref comp-OverridePCA), [`RotatePCA`](@ref comp-RotatePCA), [`AlignToPlane`](@ref comp-AlignToPlane) or [`AlignToObject`](@ref comp-AlignToObject). Nothing is recomputed except the 3D hull, which is built on first request.

**What the outputs mean** (see the [`Object`](@ref comp-Object) note for how they are derived):

- `PCA1`, `PCA2`, `PCA3`: the unit axes of the right-handed local frame.
- `Length`, `Width`, `Height`: extents of the geometry along `PCA1`, `PCA2`, `PCA3`. These are the measured values; buffers of typed objects are not applied, so they can differ from the effective dimensions reported by the Set components.
- `BoundingBox`: the PCA-aligned bounding box of the geometry, as an oriented box in world coordinates.
- `LocalPlane`: the plane with X = `PCA1`, Y = `PCA2`, placed at the center of `BoundingBox`.
- `Centroid`: the area centroid for a closed planar curve, otherwise the mean of the sample points. It is generally not the same point as the origin of `LocalPlane`.
- `PlanarHull`: the convex hull of the sample points projected onto `LocalPlane`.
- `VolumeHull`: the 3D convex hull of the sample points, as a mesh.
- `SampledPoints`: the points the PCA was computed from (curve divisions, Brep wireframe samples and vertices, mesh vertices or point-cloud points).
- `Geometry`: the stored geometry (after any conversion of surfaces, extrusions or SubDs).

**Workflow.** Use it to check that the axes and dimensions match expectations before building feature vectors, or to take individual values (for example `Height` or `Centroid`) into custom calculations and [`FeatureVector`](@ref comp-FeatureVector).

**Notes**

- `VolumeHull` is computed only when it is connected or the component preview is on. It is an empty mesh when the samples are collinear or coplanar (for example a planar curve).
- `IDs` returns the single `ID` of the input object.
