Returns the 2D convex hull of a design object, projected onto a plane, as a planar Brep (a flat filled region).

**How it is computed.** The component takes the object's sampled points, the points stored when the object was analyzed (their number and spacing come from the object component's sampling, for example `NumSamples`). The points are expressed in the coordinates of the projection plane and their convex hull is computed in that plane with Grasshopper's 2D convex hull solver; the result is a closed polyline in the plane, which is turned into a planar Brep.

The projection plane is the object's PCA1–PCA2 plane when `Plane` is not connected. When a `Plane` is supplied, only its orientation is used: its origin is moved to the center of the object's oriented bounding box. The hull therefore always passes through the object's own location, not through the supplied plane's origin.

Because the hull is built from the samples, it encloses the sampled points rather than the exact geometry: between samples (for example along a curved edge of a Brep) the hull can cut slightly inside the true silhouette. Increasing the object's sample count tightens it.

**When to use it.** Use it for a quick footprint of an object, such as the area of its top view (use Grasshopper's Area component on the output) or a convex region for nesting and clash checks. [`ObjectProperties`](@ref comp-ObjectProperties) exposes the convex hull of the same points in the PCA1–PCA2 plane as a curve (`PlanarHull`). For the exact, possibly non-convex silhouette of the geometry, use [`PlanarOutline`](@ref comp-PlanarOutline).

**Notes**

- The output is a convex polygon; concave features of the object (notches, L-shapes) are bridged over.
