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
          : base("SphereScore", "SphereScore",
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

            double vsphere = Math.PI * Math.Pow(obj.EffectiveRadius, 2);
            double vhull = obj.Hull.Volume();

            double score = (vsphere - vhull) / vsphere;

            DA.SetData(0, Math.Abs(score));
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
            get { return new Guid("4FEEA68B-24CB-4186-BD12-120CBCD9AB6B"); }
        }
    }
}