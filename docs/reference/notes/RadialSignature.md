Turns a curve into a *shape signature*: a list of sample positions measured from the curve's center, ready for Fourier analysis. The steps are:

1. **Sampling.** The curve is divided into `NumSamples` segments of equal arc length (`Curve.DivideByCount`). A closed curve gives `NumSamples` points; an open curve gives `NumSamples + 1`, including both ends. The samples are equally spaced along the curve, not at equal angles around the center.
2. **Plane.** If `Plane` is not supplied, a plane is fitted to the sample points. Either way, the plane's origin is then moved to the curve's centroid; a supplied plane keeps its orientation.
3. **Center.** For a closed planar curve the centroid is the centroid of the enclosed area (`AreaMassProperties`), the same point a design object uses. For open or non-planar curves it is the length centroid (the center of mass of the curve as a wire). Before v0.5.1 the length centroid was always used.
4. **Signature.** Each sample point is expressed in the plane's coordinates `(u, v)`; any distance off the plane is discarded. `ComplexSignature` holds `u + iv` for each sample, and `DistanceSignature` holds its magnitude, the in-plane distance from the centroid. `ProjectedCurve` is the curve projected onto the plane, and `Rays` are lines from the centroid to each projected sample, for checking the result visually.

**Where it sits.** Feed `DistanceSignature` into [`HarmonicAnalysisReal`](@ref comp-HarmonicAnalysisReal) and `ComplexSignature` into [`HarmonicAnalysisComplex`](@ref comp-HarmonicAnalysisComplex) to get a fixed-length shape descriptor per curve, which can then go to a distance component such as [`EuclideanDistance`](@ref comp-EuclideanDistance). Typical inputs are planar outlines of reclaimed parts, for example from [`PlanarOutline`](@ref comp-PlanarOutline) or [`PlanarHull`](@ref comp-PlanarHull).

**Notes**

- The directions of a fitted plane's axes are arbitrary, so the raw `u`, `v` values may appear rotated or mirrored. The harmonics components are invariant to both.
- `N` samples resolve harmonics up to `N/2`, so use at least twice as many samples as the highest harmonic you need.
- Remark for an open curve: "Curve is open: the signature is treated as periodic, so the jump from end back to start adds high harmonics".
- Errors: "NumSamples must be at least 2", "Could not divide the curve", "Could not fit a plane to the curve (is it a straight line?). Supply a Plane.", "Could not compute the curve centroid".
- Distances are in model units.
