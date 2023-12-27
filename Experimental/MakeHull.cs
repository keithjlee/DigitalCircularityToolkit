using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.GeometryProcessing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Experimental
{
    public class MakeHull : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MakeHull class.
        /// </summary>
        public MakeHull()
          : base("MakeHull", "Hull",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("ConvexHull", "Hull", "Convex hull of points", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();
            if (!DA.GetDataList(0, pts)) return;

            Mesh hull = Hulls.MakeHull(pts);

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
            get { return new Guid("74166579-BC5F-4C5A-B828-8BF9FE63FC32"); }
        }
    }
}