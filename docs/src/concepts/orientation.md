# Orientation

DCT measures every object in its own frame, found by principal component analysis (PCA). This makes the measurements independent of how a piece happens to lie in the model: a beam scanned at an angle has the same `Length` as the same beam lying along the X axis.

## Principal axes

The constructors sample the geometry (see [`Object`](@ref comp-Object) for how each geometry type is sampled) and compute the covariance matrix of the sample points. Its eigenvectors, sorted by decreasing variance, are the principal axes:

- **`PCA1`**: the direction in which the points spread the most (the length direction of a beam).
- **`PCA2`**: the direction of greatest spread perpendicular to `PCA1` (the width of a board).
- **`PCA3`**: the direction of least spread (the thickness of a board).

## Direction of the axes

An eigenvector has no inherent sign: `PCA1` and `-PCA1` describe the same axis. DCT fixes the sign with a deterministic rule, so results are reproducible:

- `PCA1` points towards positive world X (if it is perpendicular to X, towards positive Y, then positive Z).
- `PCA2` points towards positive world Y (ties: Z, then X).
- `PCA3 = PCA1 × PCA2`, so the frame is always right-handed.

## When the axes are ambiguous

PCA defines an axis only when its variance differs from the others. Some shapes have equal variances, and the axes in that plane are then arbitrary:

- a straight line: `PCA2` and `PCA3` can rotate freely about `PCA1`;
- a square plate or a circular disc: `PCA1` and `PCA2` can rotate in the plate's plane;
- a sphere or a cube: all three axes are arbitrary.

The dimensions measured along arbitrary axes are still correct for symmetric shapes, but the frame's rotation is not meaningful. When orientation matters for these shapes, set it explicitly.

## Setting the orientation yourself

- [`OverridePCA`](@ref comp-OverridePCA) replaces the principal axes with the axes of a plane you supply, for example the known grain direction of a board, and re-measures the object in that frame.
- [`RotatePCA`](@ref comp-RotatePCA) rotates an object about one of its own principal axes.
- [`AlignToPlane`](@ref comp-AlignToPlane) moves an object so its local plane coincides with a target plane; [`Knoll`](@ref comp-Knoll) does this for a whole inventory, laying the objects out on a grid.
- [`AlignToObject`](@ref comp-AlignToObject) moves one object onto another by matching their local frames. It is the usual last step after [Matching](matching.md).
