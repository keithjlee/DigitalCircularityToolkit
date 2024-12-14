using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit
{
    public class HausdorffDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Hausdorff_GH class.
        /// </summary>
        public HausdorffDistance()
          : base("HausdorffDistance (DCT)", "Hausdorff",
              "Compute the Hausdorff distance between two curves",
              "DigitalCircularityToolkit", "Distance")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("First curve", "A", "First curve", GH_ParamAccess.item);
            pManager.AddCurveParameter("Second curve", "B", "Second curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Sample count", "N", "Number of samples to use for the Hausdorff distance calculation", GH_ParamAccess.item, 50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HausdorffDistance", "HD", "Hausdorff distance between the two curves", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve A = null;
            Curve B = null;
            int N = 50;

            if (!DA.GetData(0, ref A)) return;
            if (!DA.GetData(1, ref B)) return;
            if (!DA.GetData(2, ref N)) return;

            //divide each curve
            A.DivideByCount(N, true, out Point3d[] Apoints);
            B.DivideByCount(N, true, out Point3d[] Bpoints);

            double hd = Distance.Hausdorff.ComputeDistance(Apoints, Bpoints);

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
                return IconLoader.HausdorffDistanceIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0EA65DAF-D86A-485E-BE41-E7BF102A5723"); }
        }
    }
}