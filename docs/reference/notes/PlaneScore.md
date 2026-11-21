Measures how far an object is from being flat, from the proportions of its bounding box. The object's `Length`, `Width` and `Height` are the extents of its bounding box along `PCA1`, `PCA2` and `PCA3` (see [`Object`](@ref comp-Object)); `Height` is the thickness perpendicular to the plane of greatest spread. The score is the thickness relative to the smaller of the two in-plane dimensions, multiplied by `Factor`:

```math
\text{score} = F \cdot \max\!\left(\frac{\text{Height}}{\text{Length}},\ \frac{\text{Height}}{\text{Width}}\right)
```

A perfectly flat object (including a planar curve) scores 0. With the default `Factor` of 100 the score reads as a percentage: a 20 mm board that is 200 mm wide and 2 m long scores 10. A cube scores about 100.

**Where it sits.** Connect any DCT object: [`Object`](@ref comp-Object), [`LinearObject`](@ref comp-LinearObject), [`PlanarObject`](@ref comp-PlanarObject), [`BoxObject`](@ref comp-BoxObject) or [`SphericalObject`](@ref comp-SphericalObject). Use it to judge whether a reclaimed element is adequately represented as a plane (for example, before modeling it as a [`PlanarObject`](@ref comp-PlanarObject)), or to filter an inventory. Compare with [`LineScore`](@ref comp-LineScore), which uses the same kind of ratio.

**Notes**

- The measured dimensions are used, not the effective ones: buffers and the `Thickness` of a [`PlanarObject`](@ref comp-PlanarObject) have no effect.
- For a straight object `Width` is 0, so the second ratio divides by zero and the score is not a finite number.
- `Factor` is a plain multiplier. The input description says it must be at least 100, but no check is made.
- The score depends on the orientation of the local frame, so [`OverridePCA`](@ref comp-OverridePCA) changes it.
