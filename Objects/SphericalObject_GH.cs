using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class SphericalObject_GH : Framework.ObjectConstructorComponent
    {
        /// <summary>
        /// Initializes a new instance of the SphericalObject_GH class.
        /// </summary>
        public SphericalObject_GH()
          : base("SphericalObject (DCT)", "SphereObj",
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
            pManager.AddTextParameter("ID", "ID", "Object identifier", GH_ParamAccess.item, "NoID");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SphereObject", "SphereObj", "SphericalObject class", GH_ParamAccess.item);
            pManager.AddBrepParameter("EffectiveSphere", "Sphere", "Effective sphere representation of object", GH_ParamAccess.item);
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
            Vector3d pca_user = new Vector3d(0, 0, 0);
            string id = "NoID";

            if (!DA.GetData(0, ref geo)) return null;
            geo = ObjectInput.Normalize(geo, out error);
            if (geo == null) return null;
            DA.GetData(1, ref n);
            DA.GetData(2, ref qty);

            error = ObjectInput.CheckCounts(geo, n, qty);
            if (error != null) return null;
            DA.GetData(3, ref buffer);
            DA.GetData(4, ref id);

            if (buffer <= 0)
            {
                error = "Buffer must be > 0";
                return null;
            }

            return () =>
            {
                SphericalObject obj = new SphericalObject();
                if (geo is Curve curve) obj = new SphericalObject(curve, n, qty, buffer, pca_user);
                if (geo is Brep brep) obj = new SphericalObject(brep, n, qty, buffer, pca_user);
                if (geo is Mesh mesh) obj = new SphericalObject(mesh, qty, buffer, pca_user);
                if (geo is PointCloud pointcloud) obj = new SphericalObject(pointcloud, qty, buffer, pca_user);
                obj.ID = id;
                return obj;
            };
        }

        protected override void SetOutputs(IGH_DataAccess DA, DesignObject obj)
        {
            DA.SetData(0, obj);
            DA.SetData(1, ((SphericalObject)obj).EffectiveSphere);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.SphericalObjectIcon; //.SPHERICALOBJECT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("67D25FA8-D4B8-452B-92D2-9D976A6BD1C5"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}