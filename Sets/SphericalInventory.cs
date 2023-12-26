using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Sets
{
    public class SphericalInventory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SphericalInventory class.
        /// </summary>
        public SphericalInventory()
          : base("SphereSet", "SphereSet",
              "A set of spherical elements",
              "DigitalCircularityToolkit", "Sets")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "SphereObjs", "Set of objects that form an inventory", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("EffectiveSphere", "Sphere", "Representative sphere of object", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Object plane centered in sphere", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveRadius", "r", "Radius of effective sphere", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indexer", "Inds", "Index of output with respect to input (when there are objects with more than one quantity)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "Obj", "Collected objects", GH_ParamAccess.list);
            pManager.AddPointParameter("Centroids", "Centroid", "Object centroids", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<SphericalObject> objs = new List<SphericalObject>();
            if (!DA.GetDataList(0, objs)) return;

            List<Sphere> spheres = new List<Sphere>();
            List<Plane> planes = new List<Plane>();
            List<double> radii = new List<double>();
            List<int> indices = new List<int>();
            List<Point3d> centroids = new List<Point3d>();

            for (int i = 0; i < objs.Count; i++)
            {
                int qty = objs[i].Quantity;

                for (int j = 0; j < qty; j++)
                {
                    spheres.Add(objs[i].EffectiveSphere);
                    planes.Add(objs[i].Plane);
                    radii.Add(objs[i].EffectiveRadius);
                    indices.Add(i);
                    centroids.Add(objs[i].Localbox.Center);
                }
            }

            DA.SetDataList(0, spheres);
            DA.SetDataList(1, planes);
            DA.SetDataList(2, radii);
            DA.SetDataList(3, indices);
            DA.SetDataList(4, objs);
            DA.SetDataList(5, centroids);
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
            get { return new Guid("A69EEDA3-EB23-4359-864F-20F7C1BCFB5D"); }
        }
    }
}