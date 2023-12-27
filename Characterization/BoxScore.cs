using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class BoxScore : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BoxScore class.
        /// </summary>
        public BoxScore()
          : base("BoxScore", "BoxScore",
              "Measure how well a box abstracts an input object",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BoxObject", "BoxObj", "Object to measure", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("BoxScore", "Score", "Box score. Optimal value is 0.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BoxObject obj = new BoxObject();
            if (!DA.GetData(0, ref obj)) return;

            double vbox = obj.EffectiveBox.Volume;
            double vhull = obj.Hull.Volume();

            double score = (vbox - vhull) / vbox;

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
            get { return new Guid("0343696C-BE70-46FB-9F4B-26662C2D7D27"); }
        }
    }
}