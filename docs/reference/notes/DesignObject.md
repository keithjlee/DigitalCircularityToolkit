`DesignObject` is the parameter type behind every DCT object input and output (since v0.6). It is not a component on the toolbar; it is listed here because it decides what can be wired where.

- **Panels and tooltips** show the object type, `ID` and extents, for example `LinearObject 'B-12' L=3200 W=100 H=50`.
- **Type checks:** an input that needs a particular type (e.g. [`LinearSet`](@ref comp-LinearSet)) reports `Invalid cast: DesignObject » LinearObject` when given another kind of object. Inputs that take any object ([`ObjectProperties`](@ref comp-ObjectProperties), [`AlignToPlane`](@ref comp-AlignToPlane), …) accept all of them.
- **Conversions:** an object wired into a **Plane** input gives its local plane, into a **Box** input its local box, and into a **Geometry**, Curve, Brep or Mesh input its source geometry.
- Objects coming from a Data parameter or a script component are accepted as long as they are DCT objects.
- Objects are immutable: every modifier returns a new object, and the same object can feed any number of components without them affecting each other. Objects cannot be internalized in a parameter; keep the geometry and rebuild them.
