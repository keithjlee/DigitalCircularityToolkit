using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class SphereScore : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SphereScore class.
        /// </summary>
        public SphereScore()
          : base("SphereScore (DCT)", "SphereScore",
              "Measure how well a sphere abstracts an input object",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("SphericalObject", "SphereObj", "Object to measure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "F", "Multiplier applied to the score (default 100: the score reads as a percentage)", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("SphereScore", "Score", "Sphere score. Optimal value is 0.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SphericalObject obj = new SphericalObject();
            if (!DA.GetData(0, ref obj)) return;

            double factor = 100;
            DA.GetData(1, ref factor);

            // B16: 4.0 / 3.0 (was the integer 4 / 3 = 1), and no zeroing of hulls larger than the sphere
            // (that made a well-sampled sphere score worst). The sphere encloses the samples when RadiusBuffer >= 1.
            double vsphere = 4.0 / 3.0 * Math.PI * Math.Pow(obj.EffectiveRadius, 3);
            if (!(vsphere > 0))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The effective sphere has no volume (all samples at one point): SphereScore is undefined");
                return;
            }

            double vhull = obj.Hull.Volume();

            double score = (vsphere - vhull) / vsphere * factor;

            DA.SetData(0, Math.Abs(score));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.SphereScoreIcon; //.SPHERESCORE;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4FEEA68B-24CB-4186-BD12-120CBCD9AB6B"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}