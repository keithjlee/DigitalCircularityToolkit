using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class PointCloud : GH_Component
    {
        private List<Point3d> points;

        /// <summary>
        /// Initializes a new instance of the PointCloud class.
        /// </summary>
        public PointCloud()
          : base("PointCloud", "PtCloud",
              "Convert a collection of points into a PointCloud object",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        public PointCloud(List<Point3d> points)
        {
            this.points = points;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Collection of points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PointCloud", "PtCloud", "Point Cloud", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();

            if (!DA.GetDataList(0, points)) return;

            PointCloud pointcloud = new PointCloud(points);

            DA.SetData(0, pointcloud);
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
            get { return new Guid("400349B3-C3E7-4023-AC4C-98673F25BA8C"); }
        }
    }
}