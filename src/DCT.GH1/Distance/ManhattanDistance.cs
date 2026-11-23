using System;
using System.Collections.Generic;

using DigitalCircularityToolkit.Numerics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public class ManhattanDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ManhattanDistance class.
        /// </summary>
        public ManhattanDistance()
          : base("ManhattanDistance (DCT)", "DMManhattan",
              "Generate a distance matrix of manhattan distances between two feature vector sets (obsolete: integer costs; use ManhattanCost)",
              "DigitalCircularityToolkit", "Distance")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Demand", "FVdemand", "Distance from", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Supply", "FVsupply", "Distance to", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("DistanceMatrix", "DM", "Distance matrix data where row[i,j] is cost from demand[i] to supply[j]", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_Number> demands)) return;
            if (!DA.GetDataTree(1, out GH_Structure<GH_Number> supply)) return;

            string dims_error = Metrics.CheckDimensions(CostKernels.ToRows(demands), CostKernels.ToRows(supply));
            if (dims_error != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, dims_error);
                return;
            }

            // Sum of |differences|, rounded up like the other metrics (B11: it used to truncate).
            double[][] d = CostKernels.ToRows(demands);
            double[][] s = CostKernels.ToRows(supply);
            GH_Structure<GH_Integer> distances = CostKernels.BuildTree(d.Length, s.Length,
                (i, j) => Metrics.SaturatingInt(Math.Ceiling(Metrics.Manhattan(d[i], s[j]))),
                i => new GH_Path(i));   // {0}, {1}, ... like the Euclidean metrics (it used to copy the Demand paths)

            DA.SetDataTree(0, distances);
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
                return IconLoader.ManhattanIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3890D147-152C-425A-B8D0-6A1CC74EB403"); }
        }
        // Replaced in v0.6 (COMPONENT_SPEC §2): kept loadable for saved files.
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override bool Obsolete => true;
    }
}