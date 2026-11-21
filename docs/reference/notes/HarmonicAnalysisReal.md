Describes the shape of a real-valued signature, typically the `DistanceSignature` of [`RadialSignature`](@ref comp-RadialSignature), by its Fourier harmonics. Think of the signature as a radius `r(t)` that varies as you travel once around the curve (`t` from 0 to 2π, advancing with arc length because the samples are equally spaced along the curve). Any such periodic signal is a sum of waves:

```math
r(t) = A_0 + \sum_{k \ge 1} A_k \cos(k\,t + \varphi_k)
```

`A_0` is the mean radius. Harmonic `k` is a wave with `k` bumps per trip around the outline, and `A_k` is its amplitude in model units. As a guide for a distance signature:

- `A_1`: one bulge, i.e. the outline is lopsided about its centroid (egg shape).
- `A_2`: two bulges, i.e. elongation (ellipse, long rectangle).
- `A_3`, `A_4`, ...: three-lobed, four-cornered (square) shapes, and so on.

A circle centered at the centroid has all `A_k = 0`. The output is `A_1, A_2, ..., A_⌊N/2⌋` for `N` samples; `A_0` and the phases `φ_k` are left out, which is why the result does not change with the start point, direction, in-plane rotation or mirroring.

**Computation.** The Fourier-series coefficients `c_k = (1/N) Σ_j s_j e^(−2πi·jk/N)` are computed with an FFT. Then `A_k = 2|c_k|`, except at `k = N/2` for even `N`, where `A_k = |c_k|`. Because of the `1/N`, the values do not depend on the number of samples.

**`FVLength`** keeps the first values only, lowest harmonics first. Low harmonics describe the overall form; high ones describe corners, fine detail and sampling noise, and are the least reliable. Truncating also gives every curve a vector of the same length, which the distance components require.

**`Normalize`** divides every amplitude by `A_0`, the mean of the signature (for a distance signature, the mean radius). The descriptor then describes shape only, not size, and is unitless: `A_2 = 0.1` means a variation of 10% of the mean radius.

**Where it sits.** The output list is one feature vector per curve (one branch per curve when the component runs over several signatures). Connect it to [`EuclideanDistance`](@ref comp-EuclideanDistance) or [`ManhattanDistance`](@ref comp-ManhattanDistance) as `Demand`/`Supply`, then to [`Match`](@ref comp-Match). [`HarmonicAnalysisComplex`](@ref comp-HarmonicAnalysisComplex) is the alternative for the `ComplexSignature`; it keeps more information about the shape.

**Notes**

- Errors: "Signature needs at least 2 samples", "FVLength must be at least 1", "Cannot normalize: the signature's mean is 0".
- Remark when `FVLength` is larger than the number of harmonics available: the output is shortened to all available harmonics. Use more samples for more harmonics.
- `Normalize` was added in v0.5; definitions saved with v0.4 open with it set to False. The v0.5 values differ from v0.4 (see the release notes).
