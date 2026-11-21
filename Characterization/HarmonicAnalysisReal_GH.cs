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
          : base("HarmonicAnalysisReal (DCT)", "HarmonicsReal",
              "Fourier shape descriptor of a real-valued signature (e.g. DistanceSignature): the amplitude of harmonics 1, 2, 3, ... in model units, independent of sample count. N samples give N/2 harmonics.",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Signature", "DistSig", "Shape signature as a list of real-valued numbers", GH_ParamAccess.list);
            pManager.AddIntegerParameter("FVLength", "|FV|", "Number of harmonics to keep (lowest first). Default: all", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HarmonicFeatureVector", "FV", "Amplitudes A1, A2, ... of r(t) = A0 + Σ Ak·cos(k·t + φk). Invariant to start point, direction, rotation and mirroring; not to scale.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var sig = new List<double>();
            if (!DA.GetDataList(0, sig)) return;
            if (sig.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Signature needs at least 2 samples");
                return;
            }
            HarmonicAnalysis.SetFeatureVector(this, DA, FourierDescriptors.FromReal(sig));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.HarmonicsRealIcon; //.HARMONICSREAL;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0A5ADC36-9E00-4DE2-B1DC-784EEE5E89A8"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
    }
}