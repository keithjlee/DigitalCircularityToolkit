`LinearObject` abstracts a piece of geometry as a line with a cross-section area. It runs the same sampling and PCA analysis as [`Object`](@ref comp-Object) (see that note for how `NumSamples`, the PCA frame and `Length` are defined) and then stores `Quantity`, `LengthBuffer`, `Area` and `ID`.

**Effective line.** The `EffectiveLine` lies along `PCA1`, centered on the center of the object's PCA-aligned bounding box, with length

```math
L_\text{eff} = \text{LengthBuffer} \times \text{Length}
```

where `Length` is the extent of the geometry along `PCA1`. A `LengthBuffer` below 1 shortens the line (for example to allow for trimming damaged ends); above 1 lengthens it.

**Area.** If `Area` is greater than 0 it is stored as given. Otherwise it is estimated once, at construction, as `Width × Height`: the rectangular cross-section of the bounding box measured along `PCA2` and `PCA3`. This is a bounding rectangle, not the true section area, and it is not affected by `LengthBuffer`.

**Workflow.** Use it for bars, beams, studs and other members whose length dominates, on both the supply side (scanned or modelled reclaimed elements) and the demand side (design lines). Collect the objects with [`LinearSet`](@ref comp-LinearSet), which expands them by `Quantity` and outputs effective lengths, areas and lines for [`FeatureVector`](@ref comp-FeatureVector) and [`Match`](@ref comp-Match). [`LineScore`](@ref comp-LineScore) measures how well a line represents the object.

**Notes**

- Error: "Buffer must be > 0" when `LengthBuffer` ≤ 0, plus the geometry errors listed under [`Object`](@ref comp-Object).
- For a straight line, or any curve lying in a plane, `Height` is 0 (and `Width` is 0 for a straight line), so the estimated area is 0. Supply `Area` explicitly in that case.
- `Quantity` is not validated; a value ≤ 0 makes the object contribute no rows to [`LinearSet`](@ref comp-LinearSet).
- The estimated area is not recomputed if the axes are later changed with [`OverridePCA`](@ref comp-OverridePCA).
