Measures how much of an object's effective sphere is empty, by comparing a reference sphere volume with the volume of the object's 3D convex hull. The radius is the object's effective radius: `RadiusBuffer` times the largest distance from the center of the object's bounding box to any of its sample points (the sphere shown by the `EffectiveSphere` output of [`SphericalObject`](@ref comp-SphericalObject)). The score is

```math
\text{score} = F \cdot \frac{\lvert V_\text{sphere} - V_\text{hull} \rvert}{V_\text{sphere}}
```

where `V_sphere = (4/3)·π·r³` and `V_hull` is the volume of the convex hull of the sample points. With `RadiusBuffer` ≥ 1 the sphere encloses every sample, so the hull fits inside it and the score runs from 0 (the object fills its sphere) to `Factor` (the sphere is empty).

**Where it sits.** The input must be a [`SphericalObject`](@ref comp-SphericalObject). Use the score to judge whether a sphere is a reasonable abstraction of a reclaimed element. [`BoxScore`](@ref comp-BoxScore) is the equivalent volume ratio for boxes.

**Notes**

- Before v0.5.1 the sphere volume was computed as `π·r³` (an integer division in `4 / 3`) and a hull larger than that volume was counted as empty, so well-sampled spheres scored worst. Scores from earlier versions are not comparable.
- With `RadiusBuffer` < 1 the hull can be larger than the sphere; the absolute difference is then used.
- Warning "The effective sphere has no volume ..." and no output when all samples coincide.
- The hull is built from the sample points, so a low `NumSamples` gives a smaller hull. It is computed the first time a component needs it and then kept with the object.
- For a flat object (all samples in one plane) the hull is an empty mesh with zero volume, and the score equals `Factor`.
- `Factor` is a plain multiplier (default 100, so the score reads as a percentage).
