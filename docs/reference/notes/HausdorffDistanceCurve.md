Measures how far apart two curves are as shapes. Each curve is divided by arc length into `Sample count` equal segments (Rhino's divide-by-count with end points included), and the Hausdorff distance between the two point sets is computed by brute force:

```math
H(A,B) = \max\big(h(A,B),\, h(B,A)\big), \qquad h(X,Y) = \max_{x \in X} \min_{y \in Y} \lVert x - y \rVert
```

`h(A,B)` is the largest distance from a sample of `A` to the nearest sample of `B`. The result is the larger of the two directions, so it is symmetric: it is the farthest any part of one curve lies from the other curve. It does not depend on curve direction or start point, which makes it suitable for closed curves such as outlines and cross-sections.

Use it to compare outlines once they share a position and orientation, for example after [`AlignToObject`](@ref comp-AlignToObject), [`AlignToPlane`](@ref comp-AlignToPlane) or [`PlanarOutline`](@ref comp-PlanarOutline). For open curves where the order along the curve matters, use [`FrechetDistance`](@ref comp-FrechetDistance). For point sets, use [`HausdorffDistancePoints`](@ref comp-HausdorffDistancePoints).

**Notes**

- The result is a length in the Rhino document's units. It is measured between samples, so it is an approximation; more samples give a closer value. Time grows with the product of the two point counts.
- Errors: "Sample count must be at least 1"; "Could not divide the curves".
- Since v0.5 both directions are measured; earlier versions returned only `h(A,B)`.
