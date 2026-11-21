`OverridePCA` replaces the computed principal axes of a design object with a user-defined frame. Use it when PCA picks an orientation that is not the meaningful one, for example when the grain direction of a board is known, when a square or circular object has ambiguous axes, or when demand and supply should be measured in a common orientation.

**How it works.** The component works on a copy of the input object; the upstream object is not changed. The `PrincipalPlane` input defines the new frame: its X axis becomes `PCA1`, its Y axis `PCA2` and its Z axis `PCA3`. The plane's origin is ignored; the frame is anchored at the center of the object's current bounding box. The plane's axes are used as given, so the sign rule of the automatic analysis (described under [`Object`](@ref comp-Object)) does not apply.

With the new frame, the component recomputes the bounding box of the geometry, so `Length`, `Width` and `Height` become the extents along the new axes; moves the local plane to the center of the new box; and rebuilds the planar convex hull of the samples in the new local plane. The sample points, centroid and 3D hull are unchanged.

The output keeps the input's object type, `Quantity`, buffers and `ID`. Quantities derived from the box when read (the effective line, plane or box, and the automatic `Thickness` of a [`PlanarObject`](@ref comp-PlanarObject)) follow the new axes.

**Workflow.** Place it between an object constructor ([`Object`](@ref comp-Object), [`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject), [`SphericalObject`](@ref comp-SphericalObject)) and the matching Set component, or before [`ObjectProperties`](@ref comp-ObjectProperties) or [`PlanarOutline`](@ref comp-PlanarOutline). To rotate about an existing axis instead, see [`RotatePCA`](@ref comp-RotatePCA).

**Notes**

- Values derived from the box are updated too: the estimated `Area` of a [`LinearObject`](@ref comp-LinearObject) (a given `Area` is kept) and the sphere and radius of a [`SphericalObject`](@ref comp-SphericalObject), recomputed about the new box center. The planar hull lies in the new local plane. Before v0.5.1 these kept their original values.
- No output is produced if either input is missing.
