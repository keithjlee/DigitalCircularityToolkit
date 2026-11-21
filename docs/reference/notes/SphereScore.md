Measures how much of an object's effective sphere is empty, by comparing a reference sphere volume with the volume of the object's 3D convex hull. The radius is the object's effective radius: `RadiusBuffer` times the largest distance from the center of the object's bounding box to any of its sample points (the sphere shown by the `EffectiveSphere` output of [`SphericalObject`](@ref comp-SphericalObject)). The score is

```math
\text{score} = F \cdot \frac{\lvert V_\text{sphere} - V_\text{hull} \rvert}{V_\text{sphere}}
```

where `V_hull` is the volume of the convex hull of the sample points. If `V_hull` is larger than `V_sphere`, `V_hull` is set to 0 and the score becomes `Factor`.

**Where it sits.** The input must be a [`SphericalObject`](@ref comp-SphericalObject). Use the score to judge whether a sphere is a reasonable abstraction of a reclaimed element. [`BoxScore`](@ref comp-BoxScore) is the equivalent volume ratio for boxes.

**Notes**

- In the current version, `V_sphere` is computed as `π·r³` rather than `(4/3)·π·r³` (the `4 / 3` in the source is an integer division and evaluates to 1). As a result, any object whose hull volume exceeds `π·r³`, including a well-sampled sphere with `RadiusBuffer` 1, gets the worst score (`Factor`) instead of a value near 0, and lower scores are not comparable with [`BoxScore`](@ref comp-BoxScore). Treat the output with caution until this is fixed.
- The hull is built from the sample points, so a low `NumSamples` gives a smaller hull. It is computed the first time a component needs it and then kept with the object.
- For a flat object (all samples in one plane) the hull is an empty mesh with zero volume, and the score equals `Factor`.
- `Factor` is a plain multiplier. The input description says it must be at least 100, but no check is made.
