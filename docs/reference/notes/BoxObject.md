`BoxObject` abstracts a piece of geometry as an oriented box. It runs the same sampling and PCA analysis as [`Object`](@ref comp-Object) (see that note for how `NumSamples`, the PCA frame and `Length`/`Width`/`Height` are defined) and then stores `Quantity`, the three buffers and `ID`.

**Effective box.** The starting box is the bounding box of the full geometry aligned with `PCA1`, `PCA2` and `PCA3`. `EffectiveBox` is that box scaled about its center by `LengthBuffer` along `PCA1`, `WidthBuffer` along `PCA2` and `HeightBuffer` along `PCA3`, so

```math
L_\text{eff} = f_L \cdot \text{Length}, \quad W_\text{eff} = f_W \cdot \text{Width}, \quad H_\text{eff} = f_H \cdot \text{Height}
```

A buffer below 1 shrinks the box (for example to keep only usable material); above 1 enlarges it (for example to add clearance).

**Workflow.** Use it for blocks, timber offcuts, stones and other volumetric elements that are not dominated by one or two dimensions. Collect supply and demand objects with [`BoxSet`](@ref comp-BoxSet), which expands them by `Quantity` and outputs the effective boxes and dimensions for [`FeatureVector`](@ref comp-FeatureVector). [`BoxScore`](@ref comp-BoxScore) measures how well the box represents the object.

**Notes**

- Error: "Buffer must be > 0" when any buffer ≤ 0, plus the geometry errors listed under [`Object`](@ref comp-Object).
- The box bounds the whole geometry, so a single protrusion enlarges it.
- `Quantity` is not validated; a value ≤ 0 makes the object contribute no rows to [`BoxSet`](@ref comp-BoxSet).
- The effective box is derived from the current axes, so it follows changes made with [`OverridePCA`](@ref comp-OverridePCA).
