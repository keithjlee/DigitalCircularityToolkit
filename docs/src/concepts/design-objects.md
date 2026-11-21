# Design objects

A design object is DCT's description of one piece of geometry: where it is, how it is oriented, how big it is, and which simple shape stands in for it. Every DCT workflow starts by turning both the design (demand) and the inventory (supply) into design objects, so that a drawn line and a scanned timber beam can be compared on the same terms.

## What DCT measures

The object constructors sample the geometry, find its principal axes with PCA (see [Orientation](orientation.md)), and measure it in that frame:

| Property | Meaning |
|:---------|:--------|
| `PCA1`, `PCA2`, `PCA3` | Principal axes: the directions of greatest, intermediate and least spread of the sample points. Always a right-handed frame. |
| `Length`, `Width`, `Height` | Extents of the geometry's bounding box along `PCA1`, `PCA2` and `PCA3`, in document units. |
| `LocalPlane` | X = `PCA1`, Y = `PCA2`, at the center of that bounding box. Used to move and align objects. |
| `Centroid` | The mean of the sample points (the area centroid for closed planar curves). In general not the box center. |
| Hulls | A 2D convex hull of the samples in the local plane, and a 3D convex hull computed only when needed. |

Curves, Breps, meshes and point clouds are analyzed directly; surfaces, extrusions and SubDs are converted first. [`ObjectProperties`](@ref comp-ObjectProperties) exposes all of these values. The [`Object`](@ref comp-Object) reference entry describes the sampling and measurement in detail.

## Four abstractions

Most matching questions are about one or two dimensions, not about the exact shape. DCT offers four abstractions, each with its own *effective* geometry:

| Constructor | Abstraction | Effective geometry | Typical elements |
|:------------|:------------|:-------------------|:-----------------|
| [`LinearObject`](@ref comp-LinearObject) | Line with a cross-section area | `EffectiveLine` along `PCA1` | Beams, studs, bars, tubes |
| [`PlanarObject`](@ref comp-PlanarObject) | Rectangle with a thickness | `EffectivePlane` in the local plane | Boards, panels, plates, slabs |
| [`BoxObject`](@ref comp-BoxObject) | Oriented box | `EffectiveBox` | Blocks, offcuts, bricks |
| [`SphericalObject`](@ref comp-SphericalObject) | Sphere | `EffectiveSphere` enclosing the samples | Rubble, stones, nodes |

All four run the same analysis; they differ in which effective dimensions they output. Choose the abstraction that matches the decision you are making: a truss member only needs a length (and perhaps an area), a façade panel needs a length and a width. The abstraction scores ([`LineScore`](@ref comp-LineScore), [`PlaneScore`](@ref comp-PlaneScore), [`BoxScore`](@ref comp-BoxScore), [`SphereScore`](@ref comp-SphereScore)) measure how well an abstraction fits a given object, for example to sort an inventory into linear and planar pieces.

## Quantity, buffers and ID

The typed constructors add three kinds of data that do not change the analysis:

- **`Quantity`**: how many identical pieces this object stands for. The Set components repeat the object that many times (see [Inventories](inventories.md)).
- **Buffers** (`LengthBuffer`, `WidthBuffer`, `HeightBuffer`, `RadiusBuffer`): factors that scale a measured dimension into an effective one. A `LengthBuffer` of 0.95 allows 5% for trimming damaged ends; a value above 1 adds clearance. Buffers must be greater than 0.
- **`ID`**: a label (for example an inventory number) carried through to the Set outputs, so matched pieces can be traced back.

## Objects are values

Design objects travel through Grasshopper as generic data. Components that change an object ([`OverridePCA`](@ref comp-OverridePCA), [`RotatePCA`](@ref comp-RotatePCA), [`AlignToPlane`](@ref comp-AlignToPlane), [`AlignToObject`](@ref comp-AlignToObject), [`Knoll`](@ref comp-Knoll)) return a modified copy and never change the upstream object. They also keep its type: a `LinearObject` stays a `LinearObject`, with its quantity, buffers and ID, so the result can go straight into a Set component.
