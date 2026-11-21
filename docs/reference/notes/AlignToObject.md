Moves `SourceObject` onto `TargetObject` by matching their local frames. Each design object has a `LocalPlane`: X along PCA1, Y along PCA2, Z along PCA3, with its origin at the center of the object's oriented bounding box. The component builds the plane-to-plane transform from the source's frame to the target's frame and applies it to the source. After alignment the two bounding-box centers coincide and the source's PCA1 (length), PCA2 and PCA3 axes lie along the target's.

The move is rigid (rotation plus translation); nothing is scaled. A supply object that is longer than its target therefore overhangs it by the same amount at both ends. The aligned object keeps its concrete type (a `LinearObject` stays a `LinearObject`, with its quantity and buffers), and its analysis is moved with it rather than recomputed, so `Length`, `Width` and `Height` do not change. The input object is not modified; a copy is returned.

Use it after [`Match`](@ref comp-Match) to place each assigned supply object at the position of its demand object: pick the supply objects with List Item using the `Assignment` indices (from the `Objects` output of the Set component, so the indices line up) and connect the demand objects as `TargetObject`. `Geometry` gives the moved geometry for display or baking.

**Notes**

- Both inputs must be DCT design objects, for example from [`LinearObject`](@ref comp-LinearObject), [`BoxObject`](@ref comp-BoxObject) or a Set component.
- The component does not handle unassigned demand. Remove entries with `Assignment` `-1` first: List Item with Wrap on turns `-1` into the last supply item.
- `Transform` is the transform that was applied; use it to move related geometry the same way.
- The sign of each PCA axis comes from the object analysis, so a source can land flipped end to end relative to its target. Use [`RotatePCA`](@ref comp-RotatePCA) to turn it, or [`OverridePCA`](@ref comp-OverridePCA) to set the axes of the source before alignment.
