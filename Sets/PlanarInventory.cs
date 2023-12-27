using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Sets
{
    public class PlanarInventory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlanarInventory class.
        /// </summary>
        public PlanarInventory()
          : base("PlanarSet", "PlanarSet",
              "A set of planar elements",
              "DigitalCircularityToolkit", "Sets")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "PlanarObjs", "Set of objects that form an inventory", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("EffectivePlane", "PlaneSurface", "Representative plane of object", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Representative plane centered on object", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveLength", "L", "Length of plane (PCA1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveWidth", "W", "Width of plane (PCA2)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "t", "Thickness of plane", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indexer", "Inds", "Index of output with respect to input (when there are objects with more than one quantity)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "Obj", "Collected objects", GH_ParamAccess.list);
            pManager.AddPointParameter("Centroids", "Centroid", "Object centroids", GH_ParamAccess.list);
            pManager.AddCurveParameter("Hulls", "Hulls", "Object hulls", GH_ParamAccess.list);

            pManager.HideParameter(7);
            pManager.HideParameter(8);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<PlanarObject> objs = new List<PlanarObject>();
            if (!DA.GetDataList(0, objs)) return;

            // collectors
            List<PlaneSurface> surfs = new List<PlaneSurface>();
            List<Plane> planes = new List<Plane>();
            List<double> lengths = new List<double>();
            List<double> widths = new List<double>();
            List<double> thicknesses = new List<double>();
            List<int> indices = new List<int>();
            List<Point3d> centroids = new List<Point3d>();
            List<Polyline> hulls = new List<Polyline>();

            for (int i = 0; i < objs.Count; i++)
            {
                int qty = objs[i].Quantity;

                for (int j = 0; j < qty; j++)
                {
                    surfs.Add(objs[i].EffectivePlane);
                    planes.Add(objs[i].Plane);
                    lengths.Add(objs[i].Dimension1);
                    widths.Add(objs[i].Dimension2);
                    thicknesses.Add(objs[i].Thickness);
                    indices.Add(i);
                    centroids.Add(objs[i].Localbox.Center);
                    hulls.Add(objs[i].Hull);
                }
            }

            DA.SetDataList(0, surfs);
            DA.SetDataList(1, planes);
            DA.SetDataList(2, lengths);
            DA.SetDataList(3, widths);
            DA.SetDataList(4, thicknesses);
            DA.SetDataList(5, indices);
            DA.SetDataList(6, objs);
            DA.SetDataList(7, centroids);
            DA.SetDataList(8, hulls);
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
            get { return new Guid("E9DD4189-EE4D-4501-AE0F-2EF6D2352895"); }
        }
    }
}