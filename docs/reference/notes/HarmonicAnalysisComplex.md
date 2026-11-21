Describes the shape of a planar outline by its Fourier coefficients, using the full 2D sample positions `z = u + iv` (the `ComplexSignature` of [`RadialSignature`](@ref comp-RadialSignature)) rather than only their distances. It therefore distinguishes shapes that [`HarmonicAnalysisReal`](@ref comp-HarmonicAnalysisReal) can confuse.

**What the values mean.** The outline is written as a sum of circles, each traced at a whole number of turns per trip around the curve:

```math
z(t) = \sum_k c_k\, e^{i k t}
```

`|c_k|` is the radius of circle `k` in model units. Positive `k` turns counter-clockwise `k` times per trip; negative `k` turns clockwise `|k|` times. In practice:

- `|c1|` is the radius of the best-fitting circle traced counter-clockwise: the overall size.
- `|c-1|` adds the clockwise circle. Together, `c1` and `c-1` form the best-fitting ellipse, with semi-axes `|c1| + |c-1|` and `|c1| − |c-1|`; `|c-1| = 0` means round, `|c-1|` close to `|c1|` means very elongated.
- Higher pairs `|c2|, |c-2|, |c3|, ...` add finer features. A shape with `n`-fold symmetry shows up mainly at `c(1−n)` and `c(1+n)`: a square gives `|c-3|` and `|c5|`, a triangle `|c-2|` and `|c4|`.

A circle centered at the origin gives `|c1| = R` and zeros elsewhere, whatever the sample count.

**Computation.** First the samples are made counter-clockwise: if the polygon through them has negative signed area, every value is conjugated (mirrored). Then `c_k = (1/N) Σ_j z_j e^(−2πi·jk/N)` is computed with an FFT, and the output is `|c1|, |c-1|, |c2|, |c-2|, ...`: `N − 1` values for `N` samples. For even `N`, the coefficient at `N/2` appears once, at the end. `c0` (the mean position of the samples) is left out. Taking magnitudes removes rotation and start point, and the counter-clockwise step removes direction and mirroring, which matters because a fitted plane's normal, and so the apparent direction of the curve, is arbitrary.

**`FVLength`** counts values, not harmonic pairs: `FVLength = 4` keeps `|c1|, |c-1|, |c2|, |c-2|`. Because the order is by `|k|`, truncating keeps the lowest frequencies on both sides. These describe the overall form; the high ones describe corners, fine detail and sampling noise. Truncating also gives every curve a vector of the same length, which the distance components require.

**`Normalize`** divides every value by `|c1|`, so the descriptor describes shape only, not size. The first value is then always 1 and can be ignored.

**Where it sits.** The output list is one feature vector per curve (one branch per curve when the component runs over several signatures). Connect it to [`EuclideanDistance`](@ref comp-EuclideanDistance) or [`ManhattanDistance`](@ref comp-ManhattanDistance) as `Demand`/`Supply`, then to [`Match`](@ref comp-Match).

**Notes**

- `Signature` accepts the values from `ComplexSignature`, Grasshopper complex numbers, and plain numbers (imaginary part 0). Anything else gives the error "Signature item ... is not a complex number".
- Errors: "Signature needs at least 2 samples", "FVLength must be at least 1", "Cannot normalize: |c1| (the fundamental) is 0".
- Remark when `FVLength` is larger than the number of values available: the output is shortened to all available values. Use more samples for more values.
- `Normalize` was added in v0.5; definitions saved with v0.4 open with it set to False. The v0.5 values differ from v0.4 (see the release notes).
