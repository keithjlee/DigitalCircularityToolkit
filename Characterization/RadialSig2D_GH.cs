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
    public class RadialSig2D_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Radial class.
        /// </summary>
        public RadialSig2D_GH()
          : base("RadialSig2D", "Radial2D",
              "Get the radial signature of the 2D convex hull",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignObject", "Obj", "Object to analyze", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of radial samples to take", GH_ParamAccess.item, 20);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Signature", "Sig", "Radial distance signature", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            int n = 20;
            DA.GetData(1, ref n);

            //double[] fft = RadialSignature.Harmonic2D(obj, n);
            double[] fft = RadialSignature.Harmonic2DReal(obj, n);

            DA.SetDataList(0, fft);
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