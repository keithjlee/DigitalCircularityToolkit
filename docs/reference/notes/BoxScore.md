Measures how much of an object's effective box is empty, by comparing two volumes:

- `V_box`: the volume of the object's effective box, i.e. its bounding box in the `PCA1`/`PCA2`/`PCA3` frame, scaled about its center by `LengthBuffer`, `WidthBuffer` and `HeightBuffer` (the box shown by the `EffectiveBox` output of [`BoxObject`](@ref comp-BoxObject)).
- `V_hull`: the volume of the 3D convex hull of the object's sample points.

```math
\text{score} = F \cdot \frac{\lvert V_\text{box} - V_\text{hull} \rvert}{V_\text{box}}
```

An object that fills its box (a rectangular block, with all buffers at 1) scores close to 0. With the default `Factor` of 100 the score is the percentage of the box not covered by the hull: a well-sampled cylinder in its box scores about 21 (1 − π/4). Buffers greater than 1 enlarge the box and so raise the score.

**Where it sits.** The input must be a [`BoxObject`](@ref comp-BoxObject). Use the score to judge whether a box is a reasonable abstraction of a reclaimed element, or to flag objects whose box would overstate their material. [`LineScore`](@ref comp-LineScore) and [`PlaneScore`](@ref comp-PlaneScore) measure proportions instead of volumes, so their values are not on the same scale.

**Notes**

- The hull is built from the sample points (for Breps these include all vertices), while the box encloses the full geometry. For curved objects a low `NumSamples` gives a smaller hull and a higher score.
- The 3D hull is computed the first time a component needs it and then kept with the object.
- For a flat object (all samples in one plane) the hull is an empty mesh and the box volume is zero or close to it, so the score is not meaningful; use [`PlaneScore`](@ref comp-PlaneScore).
- `Factor` is a plain multiplier. The input description says it must be at least 100, but no check is made.
