using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class SphericalObject_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SphericalObject_GH class.
        /// </summary>
        public SphericalObject_GH()
          : base("SphericalObject", "SphereObj",
              "A spherical object",
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
            pManager.AddNumberParameter("RadiusBuffer", "fR", "Scale the sphere radius by fR", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "SphericalObject class", GH_ParamAccess.item);
            pManager.AddBrepParameter("EffectiveSphere", "Sphere", "Effective sphere representation of object", GH_ParamAccess.item);
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
            Vector3d pca_user = new Vector3d(0, 0, 0);

            // populate
            if (!DA.GetData(0, ref geo)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);
            DA.GetData(3, ref buffer);

            if (buffer <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Buffer must be > 0");
            }

            // Instantiate
            SphericalObject obj = new SphericalObject();

            var curve = geo as Curve;
            if (curve != null) obj = new SphericalObject(curve, n, qty, buffer, pca_user);

            var brep = geo as Brep;
            if (brep != null) obj = new SphericalObject(brep, n, qty, buffer, pca_user);

            var mesh = geo as Mesh;
            if (mesh != null) obj = new SphericalObject(mesh, qty, buffer, pca_user);

            var pointcloud = geo as PointCloud;
            if (pointcloud != null) obj = new SphericalObject(pointcloud, qty, buffer, pca_user);

            // return
            DA.SetData(0, obj);
            DA.SetData(1, obj.EffectiveSphere);
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
            get { return new Guid("67D25FA8-D4B8-452B-92D2-9D976A6BD1C5"); }
        }
    }
}