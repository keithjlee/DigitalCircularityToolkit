using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class Normalize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Normalize class.
        /// </summary>
        public Normalize()
          : base("Normalize (DCT)", "Normalize",
              "Normalize a list of numbers by its max value",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Values", "Vals", "Values to normalize", GH_ParamAccess.list);
            pManager.AddNumberParameter("Factor", "F", "Factor to multiply after normalization", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("NormalizedValues", "Vals", "Normalized values multiplied by optional factor", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> vals = new List<double>();
            double fac = 100;

            if (!DA.GetDataList(0, vals)) return;
            DA.GetData(1, ref fac);

            double factor = fac / vals.Max();

            var normalized_vals = vals.Select(x => x * factor);

            DA.SetDataList(0, normalized_vals);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.NormalizeIcon; //.NORMALIZE;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("668B29B5-30B5-4A4A-B676-6575DF196F6F"); }
        }
    }
}