Assembles one feature vector per object from several property lists. Each input is a list holding one property for all objects, in the same object order (for example all lengths, all widths). Item `i` of every input goes into branch `{i}` of the output, in input order:

```
{0}: X1[0], X2[0], X3[0], ...
{1}: X1[1], X2[1], X3[1], ...
```

So the output has one branch per object, and every branch has as many values as there are inputs. This is the layout the distance components ([`EuclideanDistance`](@ref comp-EuclideanDistance), [`AsymmEuclideanDistance`](@ref comp-AsymmEuclideanDistance), [`ManhattanDistance`](@ref comp-ManhattanDistance)) expect for `Demand` and `Supply`.

**Adding inputs.** The component starts with one input, `X1`. Zoom in on the component on the canvas until the + and − buttons appear on the input side, and use them to add or remove inputs. Inputs are renamed `X1`, `X2`, ... in order after every change; at least one input always remains. Added inputs accept any data that converts to numbers.

**Where it sits.** Typical inputs are property lists such as the `Length`/`Width`/`Height` outputs of [`ObjectProperties`](@ref comp-ObjectProperties) or the effective dimensions output by [`LinearSet`](@ref comp-LinearSet), [`PlanarSet`](@ref comp-PlanarSet), [`BoxSet`](@ref comp-BoxSet) and [`SphereSet`](@ref comp-SphereSet). Build one FeatureVector for the demand and one for the supply, with the same properties connected to the same inputs: distances compare vectors position by position, so `X1` of the demand is compared with `X1` of the supply.

**Notes**

- Values are used as they are, with no scaling. A property measured in large numbers dominates the distance; scale properties first with [`Normalize`](@ref comp-Normalize), or with [`PairNormalize`](@ref comp-PairNormalize) to give demand and supply the same scale.
- All inputs must have the same number of items as `X1`; otherwise the component stops with the error "Data inputs must have the same length: ...", naming the input and both counts.
- Every input has list access. Give each input a single flat list. The output paths `{0}`, `{1}`, ... are created by the component and do not carry over the branch paths of the inputs.
