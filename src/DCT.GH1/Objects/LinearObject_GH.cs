using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class LinearObject_GH : Framework.ObjectConstructorComponent
    {
        /// <summary>
        /// Initializes a new instance of the LinearElement class.
        /// </summary>
        public LinearObject_GH()
          : base("LinearObject (DCT)", "LineObj",
              "A linear object",
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
            pManager.AddNumberParameter("LengthBuffer", "fL", "Scale the line length by fL", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Area", "Area", "Area of element (-1 for auto estimation)", GH_ParamAccess.item, -1.0);
            pManager.AddTextParameter("ID", "ID", "Object identifier", GH_ParamAccess.item, "NoID");
            RegisterOrientationInputs(pManager);   // v0.7, appended: see LegacyInputCount
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_DesignObject(), "LinearObject", "LinearObj", "LinearObject class", GH_ParamAccess.item) ;
            pManager.AddLineParameter("EffectiveLine", "Line", "Effective line representation of object", GH_ParamAccess.item);
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
            double buffer = 1;
            double area = -1;
            string id = "NoID";

            if (!DA.GetData(0, ref geo)) return null;
            geo = ObjectInput.Normalize(geo, out error);
            if (geo == null) return null;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);

            error = ObjectInput.CheckCounts(geo, n, qty);
            if (error != null) return null;
            DA.GetData(3, ref buffer);
            DA.GetData(4, ref area);
            DA.GetData(5, ref id);

            if (buffer <= 0)
            {
                error = "Buffer must be > 0";
                return null;
            }

            AnalysisOptions options = ReadOrientation(DA, out error);
            if (options == null) return null;

            return () => LinearObject.Create(geo, n, qty, buffer, area, id, options);
        }

        protected override int LegacyInputCount => 6;

        protected override void SetOutputs(IGH_DataAccess DA, DesignObject obj)
        {
            DA.SetData(0, obj);
            DA.SetData(1, ((LinearObject)obj).EffectiveLine);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.LinearObjectIcon; //.LINEAROBJECT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("750C489F-8D4E-41CE-AC2B-2C4AD80916BE"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}