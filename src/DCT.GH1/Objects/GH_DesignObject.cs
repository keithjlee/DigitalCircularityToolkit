using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// Grasshopper wrapper for a <see cref="DesignObject"/> (DESIGN_OBJECT_SPEC §7, P3). Objects are immutable records,
    /// so duplicating the goo shares the object. Not serialized: objects cannot be internalized.
    /// Casts: to any DesignObject type it is (LinearObject, ...); to a plane (LocalPlane), a box (Localbox) or a
    /// geometry input (the source geometry). From: a DesignObject, or any goo wrapping one (e.g. a script output).
    /// </summary>
    public class GH_DesignObject : GH_Goo<DesignObject>
    {
        public GH_DesignObject() { }
        public GH_DesignObject(DesignObject obj) : base(obj) { }

        public override bool IsValid => Value != null;
        public override string IsValidWhyNot => Value == null ? "No design object" : string.Empty;
        public override string TypeName => Value?.GetType().Name ?? "DesignObject";
        public override string TypeDescription => "A design object: geometry with its PCA frame and abstraction parameters";
        public override IGH_Goo Duplicate() => new GH_DesignObject(Value);
        public override string ToString() => Value?.ToString() ?? "Null design object";
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case DesignObject obj:
                    Value = obj;
                    return true;
                case IGH_Goo goo when goo.ScriptVariable() is DesignObject wrapped:
                    Value = wrapped;
                    return true;
                default:
                    return false;
            }
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null) return false;

            // The runtime type: a LinearObject casts to LinearObject (GH_Goo<T> only checks the static type T).
            if (Value is Q self)
            {
                target = self;
                return true;
            }

            object result = null;
            Type q = typeof(Q);
            if (q == typeof(GH_Plane)) result = new GH_Plane(Value.LocalPlane);
            else if (q == typeof(Plane)) result = Value.LocalPlane;
            else if (q == typeof(GH_Box)) result = new GH_Box(Value.Localbox);
            else if (q == typeof(Box)) result = Value.Localbox;
            else if (Value.Geometry != null && typeof(IGH_GeometricGoo).IsAssignableFrom(q))
            {
                IGH_GeometricGoo geo = GH_Convert.ToGeometricGoo(Value.Geometry);
                if (geo is Q direct) result = direct;
                else if (geo != null && geo.CastTo(out Q converted)) result = converted;
            }

            if (result is Q typed)
            {
                target = typed;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// The parameter for design objects: typed wires, tooltips with the object type and extents. Hidden from the
    /// toolbar; used by the DCT components. Registration: <c>pManager.AddParameter(new Param_DesignObject(), ...)</c>.
    /// </summary>
    public class Param_DesignObject : GH_Param<GH_DesignObject>
    {
        public Param_DesignObject()
          : base("DesignObject (DCT)", "DObj", "DigitalCircularityToolkit design objects", "DigitalCircularityToolkit", "Objects", GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("BFE61750-9409-4744-BB89-E0D4C3C8A74C");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override System.Drawing.Bitmap Icon => IconLoader.ObjectIcon;
    }
}
