using System;
using System.Collections.Generic;

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
              "Generate a distance matrix of manhattan distances between two feature vector sets",
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

            if (Utilities.CheckDims(demands, supply))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Demand and supply feature vectors must be of same dimension");
            }

            GH_Structure<GH_Integer> distances = new GH_Structure<GH_Integer>();

            for (int i = 0; i < demands.PathCount; i++)
            {
                GH_Path path = demands.Paths[i];
                List<GH_Integer> row = new List<GH_Integer>();

                for (int j = 0; j < supply.PathCount; j++)
                {
                    GH_Path path2 = supply.Paths[j];
                    double dist = 0;

                    for (int k = 0; k < demands[path].Count; k++)
                    {
                        dist += Math.Abs(demands[path][k].Value - supply[path2][k].Value);
                    }

                    row.Add(new GH_Integer((int)dist));
                }

                distances.AppendRange(row, path);
            }

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
    }
}