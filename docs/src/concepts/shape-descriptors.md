# Shape descriptors

Dimensions say how big a piece is, not what shape it has. For irregular pieces, such as stones for a paving or offcuts for a mosaic, DCT describes the outline with Fourier descriptors: a short list of numbers that captures the shape and can go straight into a cost matrix.

## From object to outline to signature

1. **Outline:** [`PlanarOutline`](@ref comp-PlanarOutline) projects an object onto a plane (by default its own local plane) and returns its outline. [`PlanarHull`](@ref comp-PlanarHull) returns the convex hull instead.
2. **Signature:** [`RadialSignature`](@ref comp-RadialSignature) samples the outline at `NumSamples` equally spaced points and describes each point relative to the curve's centroid, in the plane:
   - `ComplexSignature`: the point as a complex number `u + iv`;
   - `DistanceSignature`: its distance from the centroid.
3. **Descriptor:** a Fourier transform of the signature gives the descriptor.

## Harmonics

Think of walking once around the outline. A **harmonic** `k` is a pattern that repeats `k` times during that walk: an ellipse varies twice per trip, a square four times, a five-pointed star five times. The descriptors list how strong each harmonic is, lowest first. Low harmonics carry the overall proportions; high harmonics carry small bumps and noise. That is why truncating the descriptor (`FVLength`) to the first few values keeps the shape and drops the noise.

| | [`HarmonicAnalysisReal`](@ref comp-HarmonicAnalysisReal) | [`HarmonicAnalysisComplex`](@ref comp-HarmonicAnalysisComplex) |
|:--|:--|:--|
| Input | `DistanceSignature` | `ComplexSignature` |
| Output | amplitude of the radius variation for harmonics 1, 2, 3, ... | radii of the circles `c1, c-1, c2, c-2, ...` that trace the outline |
| Values for N samples | N/2 | N − 1 |
| Distinguishes | how the radius varies | the full 2D shape |

Both are in model units and do not depend on the number of samples, and both are unchanged by moving, rotating, mirroring, reversing or re-starting the curve. They do depend on size; set `Normalize` to true to compare shapes regardless of size. The reference entries explain the values in detail, including how `|c1|` and `|c-1|` describe the best-fitting ellipse.

## Distances between curves and point sets

To compare two specific curves or point sets directly, rather than through feature vectors:

- [`HausdorffDistanceCurve`](@ref comp-HausdorffDistanceCurve) and [`HausdorffDistancePoints`](@ref comp-HausdorffDistancePoints): the largest distance from any point of one set to the nearest point of the other, in both directions.
- [`FrechetDistance`](@ref comp-FrechetDistance): like the Hausdorff distance, but respecting the order of points along both curves.

These return a single number per pair and are useful for checking a match or ranking a few candidates.
