Rotates a design object by `RotationDegrees` about one of its own principal axes. The rotation axis passes through the object's centroid (the `Centroid` reported by [`ObjectProperties`](@ref comp-ObjectProperties)) and points along PCA1, PCA2 or PCA3, selected by `RotationAxis` (1, 2 or 3; the input offers these as named values). The angle follows the right-hand rule about the axis direction: a positive angle turns counterclockwise when looking from the tip of the axis back toward the centroid.

The rotation is rigid. The object keeps its concrete type and its analysis is rotated with it instead of recomputed: the geometry, sampled points, PCA axes, local plane, local box and hulls are all transformed. Rotating about PCA1 therefore leaves PCA1 where it is and turns PCA2 and PCA3 around it, and `Length`, `Width` and `Height` are unchanged. The input object is not modified.

Use it to correct the orientation of an object after [`AlignToPlane`](@ref comp-AlignToPlane), [`AlignToObject`](@ref comp-AlignToObject) or [`Knoll`](@ref comp-Knoll), for example to flip a member end for end (180° about PCA3) or to roll a board onto its other face (180° about PCA1). The rotated object can feed the Set components or any other component that takes a design object.

**Notes**

- Any `RotationAxis` value other than 1 or 2 rotates about PCA3.
- The pivot is the centroid, not the center of the bounding box (which [`AlignToPlane`](@ref comp-AlignToPlane) and [`Knoll`](@ref comp-Knoll) place). For an asymmetric object the two differ, so the bounding-box center can move after a rotation.
- `RotatedGeometry` is a list. For a point-cloud object it holds the individual rotated points; for any other geometry it holds the single rotated geometry.
