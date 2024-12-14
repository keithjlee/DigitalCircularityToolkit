using System;
using System.Collections.Generic;

using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class PairNormalize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PairNormalize class.
        /// </summary>
        public PairNormalize()
          : base("PairNormalize (DCT)", "PairNormalize",
              "Normalize two lists of number by the overall maximum value",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("A", "A", "First list of numbers", GH_ParamAccess.list);
            pManager.AddNumberParameter("B", "B", "Second list of numbers", GH_ParamAccess.list);
            pManager.AddNumberParameter("Factor", "F", "Factor to multiply after normalization", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("NormalizedA", "A", "Normalized values of A multiplied by optional factor", GH_ParamAccess.list);
            pManager.AddNumberParameter("NormalizedB", "B", "Normalized values of B multiplied by optional factor", GH_ParamAccess.list);
            pManager.AddNumberParameter("Normalizer", "N", "Value used for normalization", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> A = new List<double>();
            List<double> B = new List<double>();
            double fac = 100;

            if (!DA.GetDataList(0, A)) return;
            if (!DA.GetDataList(1, B)) return;
            DA.GetData(2, ref fac);

            double max = Math.Max(A.Max(), B.Max());
            double factor = fac / max;

            var normalized_A = A.Select(x => x * factor);
            var normalized_B = B.Select(x => x * factor);

            DA.SetDataList(0, normalized_A);
            DA.SetDataList(1, normalized_B);
            DA.SetData(2, max);
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
                return IconLoader.PairNormalizeIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("84D8E73F-8773-47B2-AAF2-226EDFF621F7"); }
        }
    }
}