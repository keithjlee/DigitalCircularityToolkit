using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using DigitalCircularityToolkit.GeometryProcessing;
using Rhino.Geometry;
using Accord.Statistics.Kernels;

namespace DigitalCircularityToolkit.Experimental
{
    public class MakeHull2D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MakeHull2D class.
        /// </summary>
        public MakeHull2D()
          : base("MakeHull2d", "Hull2d",
              "Make a convex hull from a set of points",
              "DigitalCircularityToolkit", "Experimental")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Points to hullify", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane to project to", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("ConvexHull", "Hull", "Convex hull of points", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();
            if (!DA.GetDataList(0, pts)) return;

            Plane plane = new Plane();
            if (!DA.GetData(1, ref plane)) return;

            Polyline hull = Hulls.MakeHull2d(pts, plane);

            DA.SetData(0, hull);
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
            get { return new Guid("2C66A36C-864A-4C86-9382-06A4EB9AE1F1"); }
        }
    }
}