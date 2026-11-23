`Object` analyzes one piece of geometry and wraps the result in a design object: a local coordinate frame, the object's extents in that frame, sampled points and convex hulls. All other DCT object types ([`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject), [`SphericalObject`](@ref comp-SphericalObject)) run exactly the same analysis and add their own parameters on top, so this note also describes them.

**Input geometry.** Curves, Breps, meshes and point clouds are analyzed directly. Surfaces and extrusions are converted to Breps, and SubDs to a mesh (`Mesh.CreateFromSubD` at density 1), before analysis. The input geometry is never modified; the converted geometry is what the object stores and what the `Geometry` output returns.

**Sampling (`NumSamples`).** Every object keeps a set of sample points, used for its hulls and radius (and, in Legacy mode, for its axes):

- Curve: `NumSamples` segments of equal arc length (`Curve.DivideByCount`). An open curve gives `NumSamples + 1` points including both ends; a closed curve gives `NumSamples` points.
- Brep: the Brep's wireframe curves (`Brep.GetWireframe` at density 1) are divided with a share of `NumSamples` proportional to each curve's length (at least 2 segments per curve, interior division points only), and all Brep vertices are added. The total is therefore approximately `NumSamples`.
- Mesh: all mesh vertices. `NumSamples` is ignored.
- Point cloud: all points. `NumSamples` is ignored.

The points are not weighted, so regions with denser sampling (for example finely meshed areas) pull the axes towards them.

**Local frame.** The `Orientation` input chooses how the axes are found; see [Orientation](../concepts/orientation.md) for the modes. **Auto** (the default for newly placed components) uses the principal axes of the shape itself (mesh triangles weighted by area, a Brep's render mesh, the region of a closed planar curve, curves by length), resolves square or round shapes with the minimum-area rectangle, and takes the edges of box Breps. **Legacy** (files saved before v0.7) uses the principal axes of the sample points described above: the covariance matrix of the samples, its eigenvectors sorted by decreasing variance. The optional `Plane` input sets the axes directly. Axis directions follow a fixed rule: `PCA1` has a non-negative world X component (if that component is zero, Y, then Z decide), `PCA2` a non-negative world Y component (ties: Z, then X), and `PCA3 = PCA1 × PCA2`, so the frame is always right-handed.

**Centroid, box and dimensions.** In Auto, PCA and MinBox the centroid is the centroid of the shape (surface, region, curve or points, as weighted for the axes); in Legacy it is the area centroid for a closed planar curve and the mean of the sample points otherwise. A bounding box of the full geometry (not of the samples) is then computed in the frame `PCA1`/`PCA2`/`PCA3`. `Length`, `Width` and `Height` are the extents of that box along `PCA1`, `PCA2` and `PCA3`, in document units. The local plane (X = `PCA1`, Y = `PCA2`) is placed at the center of this box, which in general differs from the centroid. A 2D convex hull of the sample points projected onto the local plane is stored; the 3D convex hull of the samples is built only when first requested. Use [`ObjectProperties`](@ref comp-ObjectProperties) to read all of these values.

**Typed objects: `Quantity`, buffers, `ID`.** The typed constructors add:

- `Quantity`: how many identical copies the object represents. The Set components ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)) repeat the object that many times in their outputs. It does not change the analysis.
- Buffers (`LengthBuffer`, `WidthBuffer`, `HeightBuffer`, `RadiusBuffer`): factors that scale the measured dimension into an *effective* dimension, for example `EffectiveLength = LengthBuffer × Length`. They affect the effective geometry and the Set outputs, not the measured `Length`/`Width`/`Height`. Values must be greater than 0.
- `ID`: a text label carried with the object and returned by the Set components and `ObjectProperties`.

`Object` itself has no quantity or buffers. Use it for objects that will be inspected, scored ([`LineScore`](@ref comp-LineScore), [`PlaneScore`](@ref comp-PlaneScore)), reoriented ([`OverridePCA`](@ref comp-OverridePCA), [`RotatePCA`](@ref comp-RotatePCA), [`AlignToPlane`](@ref comp-AlignToPlane)) or outlined ([`PlanarOutline`](@ref comp-PlanarOutline), [`PlanarHull`](@ref comp-PlanarHull)) rather than collected into a Set.

**Notes**

- Errors: "Unsupported geometry type ... Supported: curves, Breps/surfaces/extrusions, meshes, SubDs and point clouds." and "Could not convert the ... for analysis".
- With Grasshopper's Parallel Computing option enabled on the component, items are analyzed in parallel.
- Remark "Some objects have no preferred direction (square, round or symmetric) ...": the axes of those objects were chosen by the fixed rule (the side closest to world X). Set `Plane` if their direction matters. Error for an `Orientation` other than 0–3.
- For a planar curve or a flat point set, `Height` is 0 and the 3D hull is an empty mesh.
- `NumSamples` is a target, not an exact count; more samples give more stable axes for irregular geometry at a higher cost.
