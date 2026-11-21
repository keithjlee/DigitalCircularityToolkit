`PlanarObject` abstracts a piece of geometry as a rectangular plate with a thickness. It runs the same sampling and PCA analysis as [`Object`](@ref comp-Object) (see that note for how `NumSamples`, the PCA frame and `Length`/`Width`/`Height` are defined) and then stores `Quantity`, the two buffers, `Thickness` and `ID`.

**Effective plane.** `EffectivePlane` is a planar surface in the local plane (X = `PCA1`, Y = `PCA2`), centered on the center of the PCA-aligned bounding box, with side lengths

```math
L_\text{eff} = \text{LengthBuffer} \times \text{Length}, \qquad W_\text{eff} = \text{WidthBuffer} \times \text{Width}
```

Because the surface passes through the box center, it lies at mid-thickness of the object.

**Thickness.** If `Thickness` is greater than 0 that value is used. Otherwise the thickness is the object's `Height`, its extent along `PCA3`. No buffer is applied to the thickness.

**Workflow.** Use it for panels, boards, sheets and other flat elements. Collect supply and demand objects with [`PlanarSet`](@ref comp-PlanarSet), which expands them by `Quantity` and outputs effective lengths, widths and thicknesses for [`FeatureVector`](@ref comp-FeatureVector). [`PlaneScore`](@ref comp-PlaneScore) measures how well a plane represents the object, and [`PlanarOutline`](@ref comp-PlanarOutline) gives its outline for shape descriptors.

**Notes**

- Error: "Buffer must be > 0" when `LengthBuffer` or `WidthBuffer` ≤ 0, plus the geometry errors listed under [`Object`](@ref comp-Object).
- For a curve or point set lying in a plane, `Height` is 0, so the automatic thickness is 0. Supply `Thickness` explicitly in that case.
- `Quantity` 0 makes the object contribute no rows to [`PlanarSet`](@ref comp-PlanarSet); a negative `Quantity`, or `NumSamples` < 1 for a curve or Brep, is an error.
- The output `PlanarObject` carries all of the above and can also be read with [`ObjectProperties`](@ref comp-ObjectProperties), which reports the unbuffered dimensions.
