# Orientation

DCT measures every object in its own frame: three perpendicular axes `PCA1`, `PCA2`, `PCA3` and the box of the object aligned with them. This makes the measurements independent of how a piece happens to lie in the model: a beam scanned at an angle has the same `Length` as the same beam lying along the X axis.

Every object constructor has an `Orientation` input (and an optional `Plane`) that decides how the axes are found. Newly placed components use **Auto**; definitions saved before v0.7 use **Legacy**, so they keep their results.

## The modes

| Mode | Axes from | Use when |
|:---|:---|:---|
| **Auto** (default) | the principal axes of the *shape*; where two axes are ambiguous, the minimum-area rectangle; box Breps: their edges | almost always |
| **PCA** | the principal axes of the shape, never adjusted | you want the mass distribution, even for square shapes |
| **MinBox** | the third principal axis, then the tightest rectangle around the shape in the plane of the other two | you want the smallest box (an L-shaped plate aligned with its legs) |
| **Legacy** | principal axes of the sample points, as in v0.6 and earlier | reproducing old results |
| `Plane` input | the plane's X and Y axes | you know the direction (grain, a design axis) |

## Principal axes of the shape

The principal axes are the directions in which the object's material is spread the most (`PCA1`, the length of a beam), the most perpendicular to that (`PCA2`, the width of a board), and the least (`PCA3`, the thickness).

From v0.7 they are computed from the shape itself, weighting every part by its size:

- meshes: every triangle by its area;
- Breps: a fast render mesh of the surfaces (only the orientation comes from it; the dimensions are measured on the exact Brep);
- closed planar curves: the enclosed region (an outline stands for a plate);
- other curves: by length;
- point clouds: every point equally.

This makes the axes independent of how finely an object is meshed or sampled. Legacy mode uses the sample points instead (curve divisions, the Brep's wireframe, mesh vertices), which pulls the axes towards densely sampled areas: in our tests, remeshing half of a scanned rock more finely turned its Legacy axes by 24°, and its Auto axes by 0.1°.

## Square, round and symmetric shapes

When two principal spreads are nearly equal (within 5%), the axes in their plane are undefined: a square plate can be turned freely in its plane. Auto then uses the **minimum-area rectangle** around the shape in that plane, which always has one side flush with an edge of the outline, so squares, rectangles and polygons align with a side:

- a square or near-square plate: `PCA1` along a side, `PCA3` through the thickness;
- a beam with a square or round cross-section: `PCA1` along the beam, `PCA2`/`PCA3` from the cross-section;
- a box Brep: the axes are its edges exactly, longest first.

Some shapes still have no preferred direction: an exact square, a round section, a cube, a sphere. Auto then picks, among the equivalent directions, the one closest to world X (or world Y), so the result is at least reproducible, and the component adds a remark. Use the `Plane` input when the direction matters for such pieces.

## Direction of the axes

An axis has no inherent sign: `PCA1` and `-PCA1` describe the same line. DCT fixes the sign with a deterministic rule, so results are reproducible:

- `PCA1` points towards positive world X (if it is perpendicular to X, towards positive Y, then positive Z);
- `PCA2` points towards positive world Y (ties: Z, then X);
- `PCA3 = PCA1 × PCA2`, so the frame is always right-handed.

A `Plane` input is used as given, including its direction.

## Changing the orientation afterwards

- [`OverridePCA`](@ref comp-OverridePCA) gives an existing object the axes of a plane you supply and re-measures it, the same as the constructors' `Plane` input but downstream (for example after a Set).
- [`RotatePCA`](@ref comp-RotatePCA) rotates an object about one of its own axes.
- [`AlignToPlane`](@ref comp-AlignToPlane) moves an object so its local plane coincides with a target plane; [`Knoll`](@ref comp-Knoll) does this for a whole inventory, laying the objects out on a grid.
- [`AlignToObject`](@ref comp-AlignToObject) moves one object onto another by matching their local frames. It is the usual last step after [Matching](matching.md).
