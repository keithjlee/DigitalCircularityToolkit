Lays a list of design objects out in a flat grid on `OriginPlane`, each object turned so that its longest axis runs along the plane's Y direction ("knolling"). The layout is worked out from each object's oriented bounding box: `Length` is the extent along PCA1, `Width` the extent along PCA2.

**Layout.** The objects are placed in list order, row by row. Each row holds `ceil(N / NumRows)` objects, where N is the number of objects. Within a row, objects advance along the plane's X axis: the first object's box starts `ColSpacing` from the origin, and there is a gap of `ColSpacing` between neighboring boxes. Rows advance along the plane's Y axis: the first row starts `RowSpacing` from the origin, each row is as deep as the longest object in it, and there is a gap of `RowSpacing` between rows. Within a row, the objects' boxes are aligned on the edge nearest the plane's X axis. Each object is moved so that its bounding-box center lies in the plane, with PCA1 along the plane's +Y axis, PCA2 along its −X axis and PCA3 along its normal.

Each placement is a rigid move. The objects keep their concrete type and analysis (PCA axes, local box, hulls are moved, not recomputed), so knolled objects can feed the Set components or [`ObjectProperties`](@ref comp-ObjectProperties) directly. The input objects are not modified.

Use it to review an inventory visually, to check abstractions side by side, or as a starting point for cutting or storage layouts. To align a single object to a plane, use [`AlignToPlane`](@ref comp-AlignToPlane).

**Notes**

- `NumRows` is clamped to the range 1 to N. Because the row length is rounded up, the last row can be shorter than the others, and with some combinations the last rows are empty.
- Spacing values are in model units. Object heights (PCA3) are not used in the layout.
