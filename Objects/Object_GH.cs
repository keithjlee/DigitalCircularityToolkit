using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace DigitalCircularityToolkit.Objects
{
    public class Object_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Object_GH class.
        /// </summary>
        public Object_GH()
          : base("Object_GH", "Obj",
              "A design object",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geo", "Geometry of object", GH_ParamAccess.list);
            pManager.AddIntegerParameter("NumSamples", "n", "Target number of samples for analysis", GH_ParamAccess.item, 50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Design object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //GeometryBase geo = null;
            List<GeometryBase> geos = null;
            int n = 50;

            if (!DA.GetDataList(0, geos)) return;
            DA.GetData(1, ref n);

            // convert to object
            Object obj = new Object();

            if (geos.Count == 1)
            {
                GeometryBase geo = geos[0];

                var curve = geo as Curve;
                if (curve != null) obj = new Object(curve, n);

                var brep = geo as Brep;
                if (brep != null) obj = new Object(brep, n);

                var mesh = geo as Mesh;
                if (mesh != null) obj = new Object(mesh, n);
            }
            

            //var points = geo as List<Point3d>();
            //if (points != null) obj = new Object(points, n);

            DA.SetData(0, obj);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2A40404E-753E-4BE2-8183-3DAB27A28552"); }
        }
    }
}