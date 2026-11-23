using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class BoxObject_GH : Framework.ObjectConstructorComponent
    {
        /// <summary>
        /// Initializes a new instance of the BoxObject_GH class.
        /// </summary>
        public BoxObject_GH()
          : base("BoxObject (DCT)", "BoxObj",
              "A volumetric box object",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "Geo", "Geometry of object", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Target number of samples for analysis", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("Quantity", "qty", "Quantity of object", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("LengthBuffer", "fL", "Scale the box length by fL", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("WidthBuffer", "fW", "Scale the box width by fW", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("HeightBuffer", "fH", "Scale the box height by fH", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("ID", "ID", "Object identifier", GH_ParamAccess.item, "NoID");

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BoxObject", "BoxObj", "BoxObject class", GH_ParamAccess.item);
            pManager.AddBoxParameter("EffectiveBox", "Box", "Effective box representation of object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override Func<DesignObject> Prepare(IGH_DataAccess DA, out string error)
        {
            error = null;
            GeometryBase geo = null;
            int n = 50;
            int qty = 1;
            double buffer1 = 1;
            double buffer2 = 1;
            double buffer3 = -1;
            string id = "NoID";

            if (!DA.GetData(0, ref geo)) return null;
            geo = ObjectInput.Normalize(geo, out error);
            if (geo == null) return null;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);

            error = ObjectInput.CheckCounts(geo, n, qty);
            if (error != null) return null;
            DA.GetData(3, ref buffer1);
            DA.GetData(4, ref buffer2);
            DA.GetData(5, ref buffer3);
            DA.GetData(6, ref id);

            if (buffer1 <= 0 || buffer2 <= 0 || buffer3 <= 0)
            {
                error = "Buffer must be > 0";
                return null;
            }

            return () => BoxObject.Create(geo, n, qty, buffer1, buffer2, buffer3, id);
        }

        protected override void SetOutputs(IGH_DataAccess DA, DesignObject obj)
        {
            DA.SetData(0, obj);
            DA.SetData(1, ((BoxObject)obj).EffectiveBox);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.BoxObjectIcon; //.BOXOBJECT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B6C628AD-3711-4C8D-ACA7-EE12946DC14A"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}