# Inventories

An inventory often contains several identical pieces: twenty studs of the same length, five sheets cut from the same stock. Instead of modelling each one, give a single design object a `Quantity`. The Set components then expand the objects into one entry per physical piece, which is what the matching step needs: [`Match`](@ref comp-Match) uses every supply entry at most once.

## Set components

There is one Set component per abstraction:

| Set | Input | Effective outputs per piece |
|:----|:------|:----------------------------|
| [`LinearSet`](@ref comp-LinearSet) | `LinearObject`s | `EffectiveLine`, `EffectiveLength`, `Area`, `CentroidPlane` |
| [`PlanarSet`](@ref comp-PlanarSet) | `PlanarObject`s | `EffectivePlane`, `EffectiveLength`, `EffectiveWidth`, `Thickness` |
| [`BoxSet`](@ref comp-BoxSet) | `BoxObject`s | `EffectiveBox`, `EffectiveLength`, `EffectiveWidth`, `EffectiveHeight` |
| [`SphereSet`](@ref comp-SphereSet) | `SphericalObject`s | `EffectiveSphere`, `EffectiveRadius` |

All Sets also output `Centroids`, `Hulls`, `IDs` and an `Indexer`.

## Expansion and the Indexer

Each object is repeated `Quantity` times, in input order. With three objects of quantity 2, 1 and 3, the expanded outputs have six entries and the `Indexer` is

```
0, 0, 1, 2, 2, 2
```

`Indexer[k]` is the position in the input list of the object that entry `k` came from. After matching, use it to find which original inventory item (and its geometry or ID) was assigned. The `Objects` output is *not* expanded: it returns the input list unchanged.

## Demand is a set too

Build a Set for the demand as well, even when every demand object has quantity 1. The demand Set gives the per-piece values for the demand feature vectors and the `Centroids` and `Objects` used to draw and place the matches.

## Tips

- Feed per-piece outputs of the same Set into one [`FeatureVector`](@ref comp-FeatureVector), so all its inputs have the same length and order.
- Empty items in the input (for example geometry a constructor rejected) are skipped with a warning; the `Indexer` keeps referring to positions in the original list.
- The hulls outputs (`Hulls`) are only computed when they are connected or the component's preview is on.
