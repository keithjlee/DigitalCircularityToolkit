using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class LineScore : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LineScore class.
        /// </summary>
        public LineScore()
          : base("LineScore (DCT)", "LineScore",
              "Measure how well a line abstracts an input object",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to measure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "F", "Scale factor for distance, must be >= 100", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("LineScore", "Score", "Line score. Optimal value is 0.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            double factor = 100;
            DA.GetData(1, ref factor);

            double ratio1 = obj.Height / obj.Length;
            double ratio2 = obj.Width / obj.Length;

            if (ratio1 > ratio2)
            {
                DA.SetData(0, ratio1 * factor);
            }
            else
            {
                DA.SetData(0, ratio2 * factor);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.LineScoreIcon; //.LINESCORE;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7ACDF6AF-BB78-4DDB-B82B-3457F22BE838"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}