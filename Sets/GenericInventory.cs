using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Sets
{
    public class GenericInventory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GenericInventory class.
        /// </summary>
        public GenericInventory()
          : base("GenericSet", "GenericSet",
              "A generic collection of objects",
              "DigitalCircularityToolkit", "Sets")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "Obj", "Set of objects that form an inventory", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centroid", "Centroid", "Centroid of object", GH_ParamAccess.list);
            pManager.AddVectorParameter("PCA1", "PCA1", "Principal component vector 1", GH_ParamAccess.list);
            pManager.AddVectorParameter("PCA2", "PCA2", "Principal component vector 2", GH_ParamAccess.list);
            pManager.AddVectorParameter("PCA3", "PCA3", "Principal component vector 3", GH_ParamAccess.list);
            pManager.AddNumberParameter("Length", "L", "Length (PCA1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "W", "Width (PCA2)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "H", "Height (PCA3)", GH_ParamAccess.list);
            pManager.AddBoxParameter("BoundingBox", "BB", "Bounding box", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "Obj", "Collected objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<DesignObject> objs = new List<DesignObject>();
            if (!DA.GetDataList(0, objs)) return;

            //collectors
            List<Point3d> centroids = new List<Point3d>();
            List<Vector3d> PCA1 = new List<Vector3d>();
            List<Vector3d> PCA2 = new List<Vector3d>();
            List<Vector3d> PCA3 = new List<Vector3d>();
            List<double> lengths = new List<double>();
            List<double> widths = new List<double>();
            List<double> heights = new List<double>();
            List<Box> boxes = new List<Box>();

            foreach (DesignObject obj in objs)
            {
                centroids.Add(obj.Centroid);
                PCA1.Add(obj.PCA1);
                PCA2.Add(obj.PCA2);
                PCA3.Add(obj.PCA3);
                lengths.Add(obj.Length);
                widths.Add(obj.Width);
                heights.Add(obj.Height);
                boxes.Add(obj.Localbox);
            }

            DA.SetDataList(0, centroids);
            DA.SetDataList(1, PCA1);
            DA.SetDataList(2, PCA2);
            DA.SetDataList(3, PCA3);
            DA.SetDataList(4, lengths);
            DA.SetDataList(5, widths);
            DA.SetDataList(6, heights);
            DA.SetDataList(7, boxes);
            DA.SetDataList(8, objs);
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
            get { return new Guid("E2B14ADA-4614-42AF-9E04-F26C7F69AB3D"); }
        }
    }
}