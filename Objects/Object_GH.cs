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
          : base("Object", "Obj",
              "A design object",
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
            pManager.AddTextParameter("ID", "ID", "Object identifier", GH_ParamAccess.item, "NoID");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Design object", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geo", "Passed through geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geo = null;
            int n = 50;
            Vector3d pca_user = new Vector3d(0, 0, 0);
            string id = "NoID";

            if (!DA.GetData(0, ref geo)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref id);

            // convert to object
            DesignObject obj = new DesignObject();

            var curve = geo as Curve;
            if (curve != null) obj = new DesignObject(curve, n, pca_user);

            var brep = geo as Brep;
            if (brep != null) obj = new DesignObject(brep, n, pca_user);

            var mesh = geo as Mesh;
            if (mesh != null) obj = new DesignObject(mesh, n, pca_user);

            var pointcloud = geo as PointCloud;
            if (pointcloud != null) obj = new DesignObject(pointcloud, n, pca_user);

            obj.ID = id;

            DA.SetData(0, obj);
            DA.SetData(1, obj.Geometry);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.OBJECT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2A40404E-753E-4BE2-8183-3DAB27A28552"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}