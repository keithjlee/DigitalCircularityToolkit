Computes the Hausdorff distance between two point sets by brute force:

```math
H(A,B) = \max\big(h(A,B),\, h(B,A)\big), \qquad h(X,Y) = \max_{x \in X} \min_{y \in Y} \lVert x - y \rVert
```

For every point of one set the distance to its nearest point in the other set is found; `h` is the largest of these, and the result is the larger of the two directions. A small value means every point of each set has a close neighbor in the other. The point order and the point counts do not matter, and the two sets may have different sizes.

Use it to compare sampled geometry directly, for example the `SampledPoints` output of [`ObjectProperties`](@ref comp-ObjectProperties) for two objects after they have been brought into the same position with [`AlignToObject`](@ref comp-AlignToObject), or point clouds of scanned elements. For curves, [`HausdorffDistanceCurve`](@ref comp-HausdorffDistanceCurve) samples the curves for you.

**Notes**

- The result is a length in the Rhino document's units.
- Time grows with the product of the two set sizes (every pair is compared), which is slow for large point clouds.
- Error "Both point sets must contain at least one point" if either list is empty.
- Since v0.5 both directions are measured; earlier versions returned only `h(A,B)`.
