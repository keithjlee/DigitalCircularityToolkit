using System;
using System.Collections.Generic;
using Accord.Math;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using MathNet.Numerics.IntegralTransforms;

namespace DigitalCircularityToolkit.Characterization
{
    public class RadialSignature_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Radial class.
        /// </summary>
        public RadialSignature_GH()
          : base("RadialSignature", "RadialSig",
              "Get the radial signature of a curve",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Curve to analyze", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of radial samples to take", GH_ParamAccess.item, 20);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Signature", "Sig", "Radial distance signature", GH_ParamAccess.list);
            pManager.AddLineParameter("Rays", "Rays", "Rays used in analysis", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            if (!DA.GetData(0, ref curve)) return;

            int n = 20;
            DA.GetData(1, ref n);

            double[] signature = HarmonicAnalysis.HullSignature2D(curve, n, out Line[] rays);

            DA.SetDataList(0, signature);
            DA.SetDataList(1, rays);
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
            get { return new Guid("80025530-E9D2-4333-9B30-3D183BCCCDA8"); }
        }
    }
}