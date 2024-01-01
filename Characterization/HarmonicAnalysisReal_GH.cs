using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class HarmonicAnalysisReal_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HarmonicAnalysisReal_GH class.
        /// </summary>
        public HarmonicAnalysisReal_GH()
          : base("HarmonicAnalysisReal", "HarmonicsReal",
              "Get the 2D Fourier shape descriptor of a real-valued signature",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Signature", "DistSig", "Shape signature as a list of real-valued numbers", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HarmonicFeatureVector", "FV", "Invariant harmonic feature vector", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> sig = new List<double>();

            if (!DA.GetDataList(0, sig)) return;
            double[] descriptor = HarmonicAnalysis.Harmonic(sig.ToArray());
            DA.SetDataList(0, descriptor);
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
            get { return new Guid("0A5ADC36-9E00-4DE2-B1DC-784EEE5E89A8"); }
        }
    }
}