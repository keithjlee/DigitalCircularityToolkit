`Object` analyzes one piece of geometry and wraps the result in a design object: a local coordinate frame, the object's extents in that frame, sampled points and convex hulls. All other DCT object types ([`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject), [`SphericalObject`](@ref comp-SphericalObject)) run exactly the same analysis and add their own parameters on top, so this note also describes them.

**Input geometry.** Curves, Breps, meshes and point clouds are analyzed directly. Surfaces and extrusions are converted to Breps, and SubDs to a mesh (`Mesh.CreateFromSubD` at density 1), before analysis. The input geometry is never modified; the converted geometry is what the object stores and what the `Geometry` output returns.

**Sampling (`NumSamples`).** PCA runs on a set of sample points, not on the exact geometry:

- Curve: `NumSamples` segments of equal arc length (`Curve.DivideByCount`). An open curve gives `NumSamples + 1` points including both ends; a closed curve gives `NumSamples` points.
- Brep: the Brep's wireframe curves (`Brep.GetWireframe` at density 1) are divided with a share of `NumSamples` proportional to each curve's length (at least 2 segments per curve, interior division points only), and all Brep vertices are added. The total is therefore approximately `NumSamples`.
- Mesh: all mesh vertices. `NumSamples` is ignored.
- Point cloud: all points. `NumSamples` is ignored.

The points are not weighted, so regions with denser sampling (for example finely meshed areas) pull the axes towards them.

**Local frame (PCA).** The covariance matrix of the sample points is computed and its eigenvectors, sorted by decreasing eigenvalue (variance), give the principal axes: `PCA1` is the direction of greatest spread, `PCA2` the greatest spread perpendicular to it, `PCA3` the least. Eigenvectors have no inherent sign, so DCT fixes one: `PCA1` is flipped if needed so its world X component is non-negative (if that component is zero, Y, then Z decide); `PCA2` so its world Y component is non-negative (ties: Z, then X); and `PCA3 = PCA1 × PCA2`, so the frame is always right-handed. When two variances are equal (a straight line, a square plate, a sphere) the axes within that plane are mathematically undefined and their direction is arbitrary.

**Centroid, box and dimensions.** The centroid is the area centroid for a closed planar curve and the mean of the sample points otherwise. A bounding box of the full geometry (not of the samples) is then computed in the frame `PCA1`/`PCA2`/`PCA3`. `Length`, `Width` and `Height` are the extents of that box along `PCA1`, `PCA2` and `PCA3`, in document units. The local plane (X = `PCA1`, Y = `PCA2`) is placed at the center of this box, which in general differs from the centroid. A 2D convex hull of the sample points projected onto the local plane is stored; the 3D convex hull of the samples is built only when first requested. Use [`ObjectProperties`](@ref comp-ObjectProperties) to read all of these values.

**Typed objects: `Quantity`, buffers, `ID`.** The typed constructors add:

- `Quantity`: how many identical copies the object represents. The Set components ([`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet), [`SphereSet`](@ref comp-SphereSet)) repeat the object that many times in their outputs. It does not change the analysis.
- Buffers (`LengthBuffer`, `WidthBuffer`, `HeightBuffer`, `RadiusBuffer`): factors that scale the measured dimension into an *effective* dimension, for example `EffectiveLength = LengthBuffer × Length`. They affect the effective geometry and the Set outputs, not the measured `Length`/`Width`/`Height`. Values must be greater than 0.
- `ID`: a text label carried with the object and returned by the Set components and `ObjectProperties`.

`Object` itself has no quantity or buffers. Use it for objects that will be inspected, scored ([`LineScore`](@ref comp-LineScore), [`PlaneScore`](@ref comp-PlaneScore)), reoriented ([`OverridePCA`](@ref comp-OverridePCA), [`RotatePCA`](@ref comp-RotatePCA), [`AlignToPlane`](@ref comp-AlignToPlane)) or outlined ([`PlanarOutline`](@ref comp-PlanarOutline), [`PlanarHull`](@ref comp-PlanarHull)) rather than collected into a Set.

**Notes**

- Errors: "Unsupported geometry type ... Supported: curves, Breps/surfaces/extrusions, meshes, SubDs and point clouds." and "Could not convert the ... for analysis".
- With Grasshopper's Parallel Computing option enabled on the component, items are analyzed in parallel.
- For a planar curve or a flat point set, `Height` is 0 and the 3D hull is an empty mesh.
- `NumSamples` is a target, not an exact count; more samples give more stable axes for irregular geometry at a higher cost.
