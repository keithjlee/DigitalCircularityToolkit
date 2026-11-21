Moves a design object so that its local frame coincides with `Plane`. The local frame is the object's `LocalPlane`: X along PCA1, Y along PCA2, Z along PCA3, with its origin at the center of the object's oriented bounding box. The component builds a plane-to-plane transform from that frame to `Plane` and applies it, so after alignment the bounding-box center sits on the plane origin, PCA1 points along the plane's X axis, PCA2 along its Y axis and PCA3 along its normal.

The move is rigid (rotation plus translation). The object keeps its concrete type (a `LinearObject` stays a `LinearObject`, with its quantity and buffers), and its analysis is moved with it rather than recomputed: the PCA axes, centroid, sampled points, local box and hulls are transformed, and `Length`, `Width` and `Height` do not change. The input object is not modified; a copy is returned.

Use it to lay a reclaimed object flat in a known orientation, for example before taking a [`PlanarOutline`](@ref comp-PlanarOutline) or for drawings and fabrication layouts. The aligned object can go straight into the Set components ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)). To lay out many objects at once, use [`Knoll`](@ref comp-Knoll); to align one object onto another, use [`AlignToObject`](@ref comp-AlignToObject).

**Notes**

- `Transform` is the plane-to-plane transform that was applied. Use it to move related geometry (labels, connection details) the same way.
- The sign of each PCA axis is fixed by the object analysis, so an object can land flipped end to end relative to what you expect. Use [`RotatePCA`](@ref comp-RotatePCA) afterwards to turn it.
