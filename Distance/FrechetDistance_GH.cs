﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit
{
    public class FrechetDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FrechetDistance class.
        /// </summary>
        public FrechetDistance()
          : base("FrechetDistance (DCT)", "Frechet",
              "Compute the Frechet distance between two curves",
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
            pManager.AddIntegerParameter("Sample count", "N", "Number of samples to use for the Frechet distance calculation", GH_ParamAccess.item, 50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("FrechetDistance", "FD", "Frechet distance between the two curves", GH_ParamAccess.item);
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

            if (A.IsClosed || B.IsClosed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Frechet distances will work best for open curves; consider using the Hausdorff distance instead.");
            }

            //divide each curve
            A.DivideByCount(N, true, out Point3d[] Apoints);
            B.DivideByCount(N, true, out Point3d[] Bpoints);

            double fd = Frechet.FrechetDistance(Apoints, Bpoints);

            DA.SetData(0, fd);
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
                return IconLoader.FrechetDistanceIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("17CB1D16-9277-477B-955E-2C45AAB7200F"); }
        }
    }
}