using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit
{
    public class HausdorffDistancePoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HausdorffPoints_GH class.
        /// </summary>
        public HausdorffDistancePoints()
          : base("HausdorffDistancePoints (DCT)", "HausdorffPoints",
              "Compute the Hausdorff distance between two sets of points",
              "DigitalCircularityToolkit", "Distance")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("First set of points", "A", "First set of points", GH_ParamAccess.list);
            pManager.AddPointParameter("Second set of points", "B", "Second set of points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HausdorffDistancePoints", "HD", "Hausdorff distance between the two sets of points", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> A = new List<Point3d>();
            List<Point3d> B = new List<Point3d>();

            if (!DA.GetDataList(0, A)) return;
            if (!DA.GetDataList(1, B)) return;

            double hd = Distance.Hausdorff.ComputeDistance(A.ToArray(), B.ToArray());

            DA.SetData(0, hd);
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
                return IconLoader.HausdorffDistancePointsIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C1F08545-76D0-4E68-9C04-EC900E1FCC6E"); }
        }
    }
}