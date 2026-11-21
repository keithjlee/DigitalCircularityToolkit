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

            pManager.AddBooleanParameter("Normalize", "Norm", "Divide by |c1| (the radius of the best-fitting circle), so the descriptor ignores size; the first value is then 1", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("HarmonicFeatureVector", "FV", "|c1|, |c-1|, |c2|, |c-2|, ... after orienting the samples counter-clockwise. Invariant to position, rotation, start point, direction and mirroring; to scale only with Normalize.", GH_ParamAccess.list);
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
            bool normalize = false;
            DA.GetData(2, ref normalize);
            double[] descriptor = Numerics.FourierDescriptors.FromComplex(sig, normalize, out double scale);
            if (normalize && !(scale > 0))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot normalize: |c1| (the fundamental) is 0");
                return;
            }
            HarmonicAnalysis.SetFeatureVector(this, DA, descriptor);
        }

        /// <summary>
        /// Normalize (input 2) was added in v0.5; files saved with v0.4 have 2 inputs and must open without IO errors.
        /// </summary>
        public override bool Read(GH_IO.Serialization.GH_IReader reader) =>
            Framework.AppendedInputs.Read(this, reader, 2, base.Read);

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