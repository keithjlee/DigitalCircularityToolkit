Measures how far an object is from being a line, from the proportions of its bounding box. The object's `Length`, `Width` and `Height` are the extents of its bounding box along `PCA1`, `PCA2` and `PCA3` (see [`Object`](@ref comp-Object)). The score is the larger of the two cross-section dimensions relative to the length, multiplied by `Factor`:

```math
\text{score} = F \cdot \frac{\max(\text{Width},\ \text{Height})}{\text{Length}}
```

A perfectly slender object scores 0. With the default `Factor` of 100 the score reads as a percentage: a score of 5 means the larger cross-section dimension is 5% of the length. A cube scores about 100.

**Where it sits.** Connect any DCT object: [`Object`](@ref comp-Object), [`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject) or [`SphericalObject`](@ref comp-SphericalObject). Use it to judge whether a reclaimed element is adequately represented as a line (for example, before modeling it as a [`LinearObject`](@ref comp-LinearObject)), or to filter an inventory by slenderness. [`PlaneScore`](@ref comp-PlaneScore), [`BoxScore`](@ref comp-BoxScore) and [`SphereScore`](@ref comp-SphereScore) measure the other abstractions; note that the box and sphere scores are volume ratios, so their values are not on the same scale as this one.

**Notes**

- The measured dimensions are used, not the effective ones: buffers such as `LengthBuffer` have no effect.
- `PCA1` is the direction of greatest spread of the samples, so `Length` is normally the largest dimension, but this is not enforced; for unusual shapes the ratio can exceed 1.
- `Factor` is a plain multiplier. The input description says it must be at least 100, but no check is made.
- The score depends on the orientation of the local frame, so [`OverridePCA`](@ref comp-OverridePCA) changes it.
