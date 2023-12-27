using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Sets
{
    public class BoxInventory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BoxInventory class.
        /// </summary>
        public BoxInventory()
          : base("BoxSet", "BoxSet",
              "A set of box elements",
              "DigitalCircularityToolkit", "Sets")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "BoxObjs", "Set of objects that form an inventory", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("EffectiveBox", "Box", "Representative box of object", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Representative plane centered on object", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveLength", "L", "Length of box (PCA1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveWidth", "W", "Width of box (PCA2)", GH_ParamAccess.list);
            pManager.AddNumberParameter("EffectiveHeight", "H", "Height of box", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indexer", "Inds", "Index of output with respect to input (when there are objects with more than one quantity)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "Obj", "Collected objects", GH_ParamAccess.list);
            pManager.AddPointParameter("Centroids", "Centroid", "Object centroids", GH_ParamAccess.list);
            pManager.AddMeshParameter("Hulls", "Hulls", "Object hulls", GH_ParamAccess.list);

            pManager.HideParameter(7);
            pManager.HideParameter(8);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BoxObject> objs = new List<BoxObject>();
            if (!DA.GetDataList(0, objs)) return;

            // collectors
            List<Box> boxes = new List<Box>();
            List<Plane> planes = new List<Plane>();
            List<double> lengths = new List<double>();
            List<double> widths = new List<double>();
            List<double> heights = new List<double>();
            List<int> indices = new List<int>();
            List<Point3d> centroids = new List<Point3d>();
            List<Mesh> hulls = new List<Mesh>();

            for (int i = 0; i < objs.Count; i++)
            {
                int qty = objs[i].Quantity;

                for (int j = 0; j < qty; j++)
                {
                    boxes.Add(objs[i].EffectiveBox);
                    planes.Add(objs[i].Plane);
                    lengths.Add(objs[i].EffectiveLength);
                    widths.Add(objs[i].EffectiveWidth);
                    heights.Add(objs[i].EffectiveHeight);
                    indices.Add(i);
                    centroids.Add(objs[i].Localbox.Center);
                    hulls.Add(objs[i].Hull);
                }
            }

            DA.SetDataList(0, boxes);
            DA.SetDataList(1, planes);
            DA.SetDataList(2, lengths);
            DA.SetDataList(3, widths);
            DA.SetDataList(4, heights);
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
            get { return new Guid("7C6C17A8-B363-46C3-BB56-174FE2555D6C"); }
        }
    }
}