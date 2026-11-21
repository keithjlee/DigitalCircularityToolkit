`SphericalObject` abstracts a piece of geometry as a sphere. It runs the same sampling and PCA analysis as [`Object`](@ref comp-Object) (see that note for how `NumSamples` and the PCA frame are defined) and then stores `Quantity`, `RadiusBuffer` and `ID`.

**Effective sphere.** The center is the center of the object's PCA-aligned bounding box. The radius is the largest distance from that center to any of the sample points, so the sphere encloses all samples (not necessarily every point of the exact geometry between samples). The effective radius is

```math
r_\text{eff} = \text{RadiusBuffer} \times r
```

and `EffectiveSphere` is the sphere with that center and radius. Both are computed once, when the object is constructed.

**Workflow.** Use it for compact, roughly isotropic elements such as rubble, stones or nodes, where orientation does not matter. Collect supply and demand objects with [`SphereSet`](@ref comp-SphereSet), which expands them by `Quantity` and outputs effective radii for [`FeatureVector`](@ref comp-FeatureVector). [`SphereScore`](@ref comp-SphereScore) measures how well the sphere represents the object.

**Notes**

- Error: "Buffer must be > 0" when `RadiusBuffer` ≤ 0, plus the geometry errors listed under [`Object`](@ref comp-Object).
- The radius depends on the sampling: for meshes and point clouds all vertices or points are used; for curves and Breps more `NumSamples` gives a tighter estimate.
- `Quantity` 0 makes the object contribute no rows to [`SphereSet`](@ref comp-SphereSet); a negative `Quantity`, or `NumSamples` < 1 for a curve or Brep, is an error.
- When the axes are changed with [`OverridePCA`](@ref comp-OverridePCA), the sphere is recomputed about the new box center.
