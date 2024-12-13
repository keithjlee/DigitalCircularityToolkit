using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class LinearObject_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LinearElement class.
        /// </summary>
        public LinearObject_GH()
          : base("LinearObject", "LineObj",
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
            pManager.AddNumberParameter("LengthBuffer", "fL", "Scale the plane length by fL", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Area", "Area", "Area of element (-1 for auto estimation)", GH_ParamAccess.item, -1.0);
            pManager.AddTextParameter("ID", "ID", "Object identifier", GH_ParamAccess.item, "NoID");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "LinearObject class", GH_ParamAccess.item) ;
            pManager.AddLineParameter("EffectiveLine", "Line", "Effective line representation of object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            GeometryBase geo = null;
            int n = 50;
            int qty = 1;
            double buffer = 1;
            double area = -1;
            Vector3d pca_user = new Vector3d(0, 0, 0);
            string id = "NoID";

            // populate
            if (!DA.GetData(0, ref geo)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);
            DA.GetData(3, ref buffer);
            DA.GetData(4, ref area);
            DA.GetData(5, ref id);

            if (buffer <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Buffer must be > 0");
            }

            // Instantiate
            LinearObject obj = new LinearObject();

            var curve = geo as Curve;
            if (curve != null) obj = new LinearObject(curve, n, qty, buffer, area, pca_user);

            var brep = geo as Brep;
            if (brep != null) obj = new LinearObject(brep, n, qty, buffer, area, pca_user);

            var mesh = geo as Mesh;
            if (mesh != null) obj = new LinearObject(mesh, qty, buffer, area, pca_user);

            var pointcloud = geo as PointCloud;
            if (pointcloud != null) obj = new LinearObject(pointcloud, qty, buffer, area, pca_user);

            //id
            obj.ID = id;

            // return
            DA.SetData(0, obj);
            DA.SetData(1, obj.EffectiveLine);
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