using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class HarmonicAnalysisComplex_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HarmonicAnalysis_GH class.
        /// </summary>
        public HarmonicAnalysisComplex_GH()
          : base("HarmonicAnalysisComplex (DCT)", "HarmonicsComplex",
              "Fourier shape descriptor of a complex signature (e.g. ComplexSignature): the radius of each harmonic in model units, independent of sample count, ordered |c1|, |c-1|, |c2|, |c-2|, ... N samples give N-1 values.",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Signature", "ComplexSig", "Shape signature as a list of complex numbers (u + iv per sample)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("FVLength", "|FV|", "Number of values to keep (lowest frequencies first). Default: all", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HarmonicFeatureVector", "FV", "|c1|, |c-1|, |c2|, |c-2|, ... after orienting the samples counter-clockwise. Invariant to position, rotation, start point, direction and mirroring; not to scale.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var items = new List<IGH_Goo>();
            if (!DA.GetDataList(0, items)) return;
            List<System.Numerics.Complex> sig = HarmonicAnalysis.ToComplex(items, out string error);
            if (sig == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                return;
            }
            if (sig.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Signature needs at least 2 samples");
                return;
            }
            HarmonicAnalysis.SetFeatureVector(this, DA, FourierDescriptors.FromComplex(sig));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.HarmonicsComplexIcon; //.HARMONICSCOMPLEX;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A991FD8B-F03C-4924-88A4-C372B7E7383D"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
    }
}