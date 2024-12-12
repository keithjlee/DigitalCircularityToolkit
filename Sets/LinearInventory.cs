using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Sets
{
    public class LinearInventory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LinearInventory class.
        /// </summary>
        public LinearInventory()
          : base("LinearSet", "LinearSet",
              "A set of linear elements",
              "DigitalCircularityToolkit", "Sets")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "LinearObjs", "Set of objects that form an inventory", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("EffectiveLine", "Line", "Representative line of object", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveLength", "L", "Length (PCA1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Area", "A", "Cross-section area of object", GH_ParamAccess.list);
            pManager.AddPlaneParameter("CentroidPlane", "Plane", "Plane centered at middle of line with Z axis parallel to PCA1", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indexer", "Inds", "Index of output with respect to input (when there are objects with more than one quantity)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "Obj", "Collected objects", GH_ParamAccess.list);
            pManager.AddPointParameter("Centroids", "Centroid", "Object centroids", GH_ParamAccess.list);
            pManager.AddCurveParameter("Hulls", "Hulls", "Object hulls", GH_ParamAccess.list);
            pManager.AddTextParameter("IDs", "IDs", "Object IDs", GH_ParamAccess.list);

            pManager.HideParameter(6);
            pManager.HideParameter(7);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<LinearObject> objs = new List<LinearObject>();
            if (!DA.GetDataList(0, objs)) return;

            // collectors
            List<Line> lines = new List<Line>();
            List<double> lengths = new List<double>();
            List<double> areas = new List<double>();
            List<Plane> planes = new List<Plane>();
            List<int> indices = new List<int>();
            List<Point3d> centroids = new List<Point3d>();
            List<Polyline> hulls = new List<Polyline>();
            List<string> ids = new List<string>();

            for (int i = 0; i < objs.Count; i++)
            {
                int qty = objs[i].Quantity;

                for (int j = 0; j < qty; j++)
                {
                    lines.Add(objs[i].EffectiveLine);
                    lengths.Add(objs[i].EffectiveLength);
                    areas.Add(objs[i].Area);
                    planes.Add(objs[i].CrossSectionPlane);
                    indices.Add(i);
                    centroids.Add(objs[i].Localbox.Center);
                    hulls.Add(objs[i].Hull);
                    ids.Add(objs[i].ID);
                }
            }

            DA.SetDataList(0, lines);
            DA.SetDataList(1, lengths);
            DA.SetDataList(2, areas);
            DA.SetDataList(3, planes);
            DA.SetDataList(4, indices);
            DA.SetDataList(5, objs);
            DA.SetDataList(6, centroids);
            DA.SetDataList(7, hulls);
            DA.SetDataList(8, ids);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; //.LINEARSET;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7248CB53-EB12-4122-82CD-313802D511F9"); }
        }
    }
}