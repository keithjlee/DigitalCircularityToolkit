using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class PlanarObject_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlanarObject_GH class.
        /// </summary>
        public PlanarObject_GH()
          : base("PlanarObject", "PlaneObj",
              "A planar object",
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
            pManager.AddNumberParameter("LengthBuffer", "Buffer1", "Reduce the effective length of the element by buffer * length", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("WidthBuffer", "Buffer2", "Reduce the effective width of the element by buffer * width", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness of element (-1 for auto estimation)", GH_ParamAccess.item, -1.0);
            pManager.AddVectorParameter("PCAOverride", "ForcePCA1", "Override the calculated PCA1 vector", GH_ParamAccess.item, new Vector3d(0, 0, 0));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "LinearObject class", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("EffectivePlane", "Plane", "Effective plane representation of object", GH_ParamAccess.item);
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
            double buffer1 = 1;
            double buffer2 = 1;
            double thickness = -1;
            Vector3d pca_user = new Vector3d(0, 0, 0);

            // populate
            if (!DA.GetData(0, ref geo)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);
            DA.GetData(3, ref buffer1);
            DA.GetData(4, ref buffer2);
            DA.GetData(5, ref thickness);
            DA.GetData(6, ref pca_user);

            if (buffer1 <= 0 || buffer2 <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Buffer must be > 0");
            }

            PlanarObject obj = new PlanarObject();

            var curve = geo as Curve;
            if (curve != null) obj = new PlanarObject(curve, n, qty, buffer1, buffer2, thickness, pca_user);

            var brep = geo as Brep;
            if (brep != null) obj = new PlanarObject(brep, n, qty, buffer1, buffer2, thickness, pca_user);

            var mesh = geo as Mesh;
            if (mesh != null) obj = new PlanarObject(mesh, qty, buffer1, buffer2, thickness, pca_user);

            var pointcloud = geo as PointCloud;
            if (pointcloud != null) obj = new PlanarObject(pointcloud, qty, buffer1, buffer2, thickness, pca_user);

            // return
            DA.SetData(0, obj);
            DA.SetData(1, obj.EffectivePlane);
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
            get { return new Guid("21C2A735-15B5-4DED-AF15-1075C3CD62D4"); }
        }
    }
}