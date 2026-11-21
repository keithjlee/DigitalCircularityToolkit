Returns the "shadow" of a design object's geometry on a plane: the outline you would see looking straight down the plane's normal. Unlike [`PlanarHull`](@ref comp-PlanarHull), it works on the object's actual geometry, not its sampled points, and the outline can be non-convex.

**Projection plane.** Without a `Plane` input, the object's local plane is used (X along PCA1, Y along PCA2), so the outline is the object's top view in its own frame. A supplied `Plane` sets the orientation only. In both cases the origin is moved to the center of the object's oriented bounding box, so the outline lies at the object's location.

**How each geometry type is handled.**

- Curve: the curve is projected onto the plane and returned as is. An open curve stays open; this is a projection, not an outline.
- Mesh: the mesh outline on the plane (Rhino's `Mesh.GetOutlines`). The outline polylines are joined and the first joined curve is returned.
- Brep: the Brep is meshed with default meshing parameters, then treated as a mesh.
- Point cloud: the 2D convex hull of the points in the plane.

If the mesh outline fails (it can when faces are edge-on to the plane or project onto each other, as with a box seen along its own axes), and the mesh or Brep is closed, the component retries with the front-facing faces only, which cover the whole shadow of a closed shape. If there is still no outline (or the shape is open, so the retry does not apply), it falls back to the 2D convex hull of the mesh vertices (for a Brep, its vertices plus the vertices of a coarse render mesh) and warns.

**When to use it.** The outline is the input for 2D shape descriptors: feed it to [`RadialSignature`](@ref comp-RadialSignature) and then [`HarmonicAnalysisReal`](@ref comp-HarmonicAnalysisReal) or [`HarmonicAnalysisComplex`](@ref comp-HarmonicAnalysisComplex), or compare outlines directly with [`HausdorffDistanceCurve`](@ref comp-HausdorffDistanceCurve) or [`FrechetDistance`](@ref comp-FrechetDistance). Align objects first ([`AlignToPlane`](@ref comp-AlignToPlane)) if outlines of different objects should be compared in a common frame.

**Notes**

- Warning: "Could not compute the exact outline; returning the planar convex hull instead". The result is then convex and may be larger than the true shadow.
- Error: "Could not compute an outline for geometry of type …" when no curve could be produced; the output is empty.
- Only one curve is returned. For a shadow with holes or several disconnected parts, the other loops are dropped.
- The point-cloud result is always a convex hull, without a warning.
