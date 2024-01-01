﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class HarmonicAnalysis_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HarmonicAnalysis_GH class.
        /// </summary>
        public HarmonicAnalysis_GH()
          : base("HarmonicAnalysis", "Harmonics",
              "Get the 2D Fourier shape descriptor of a radial signature",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Signature", "Sig", "Shape signature as either a list of complex numbers or list of real numbers (ComplexSig or DistSig)", GH_ParamAccess.list);
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
            List<System.Numerics.Complex> sig_complex = new List<System.Numerics.Complex>();
            List<double> sig_real = new List<double>();

            if (DA.GetDataList(0, sig_complex))
            {
                double[] descriptor = HarmonicAnalysis.Harmonic(sig_complex.ToArray());
                DA.SetDataList(0, descriptor);
            }
            else if (DA.GetDataList(0, sig_real))
            {
                double[] descriptor = HarmonicAnalysis.Harmonic(sig_real.ToArray());
                DA.SetDataList(0, descriptor);
            }
            else
            {
                return;
            }
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
            get { return new Guid("A991FD8B-F03C-4924-88A4-C372B7E7383D"); }
        }
    }
}