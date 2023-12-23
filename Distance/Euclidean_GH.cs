using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public class Euclidean_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CostMatrix_GH class.
        /// </summary>
        public Euclidean_GH()
          : base("EuclideanDistance", "DMEuclidean",
              "Generate a cost matrix of Euclidean distances between two feature vector sets",
              "DigitalCircularityToolkit", "Distance")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Demand", "D", "Distance from", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Supply", "S", "Distance to", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DistanceMatrix", "DM", "Distance matrix (with padded row/cols to form square)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Data", "Data", "Distance matrix data where row[i,j] is cost from demand[i] to supply[j]", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_Number> demands)) return;
            if (!DA.GetDataTree(1, out GH_Structure<GH_Number> supply)) return;

            List<double[]> demand_data = Utilities.Tree2List(demands);
            List<double[]> supply_data = Utilities.Tree2List(supply);

            int[,] cost_matrix = Euclidean.EuclideanCostMatrix(demand_data, supply_data);
            GH_Structure<GH_Integer> cost_data = Utilities.CostMatrix2CostTree(cost_matrix, demand_data.Count, supply_data.Count);

            DA.SetData(0, cost_matrix);
            DA.SetDataTree(1, cost_data);
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
            get { return new Guid("1D4C7587-5D12-44AF-B21B-F4168018BB7B"); }
        }
    }
}