Measures how different two curves are as paths. Each curve is divided by arc length into `Sample count` equal segments (Rhino's divide-by-count with end points included, so an open curve gives `Sample count` + 1 points). The component then computes the discrete Fréchet distance between the two point sequences: the smallest possible "leash length" when one walker moves along each sequence from start to end, both moving forward only. It is computed with a dynamic programme over all point pairs, so time grows with the product of the two point counts.

Because the walk follows the point order, the result depends on curve direction: a curve and its reverse can have a large Fréchet distance even though they occupy the same place. Unlike [`HausdorffDistanceCurve`](@ref comp-HausdorffDistanceCurve), which only compares the point sets, it distinguishes curves that visit the same region in a different order. Use it to compare shapes of open curves that have been brought into a common position, for example with [`AlignToObject`](@ref comp-AlignToObject) or [`AlignToPlane`](@ref comp-AlignToPlane). The output is a single number per pair of curves; to use it as a matching cost, arrange the values yourself as a tree with one branch per demand curve and one item per supply curve, and feed it to [`Match`](@ref comp-Match).

**Notes**

- The result is a length in the Rhino document's units. It is a discrete approximation; more samples give a value closer to the continuous Fréchet distance.
- Remark "Frechet distances will work best for open curves; consider using the Hausdorff distance instead." when either curve is closed (the start point of a closed curve is arbitrary).
- Errors: "Sample count must be at least 1"; "Could not divide the curves".
