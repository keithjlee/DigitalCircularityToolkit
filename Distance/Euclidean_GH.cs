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
          : base("EuclideanDistance (DCT)", "DMEuclidean",
              "Generate a distance matrix of Euclidean distances between two feature vector sets",
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
            pManager.AddIntegerParameter("Power", "p", "Power factor for distance", GH_ParamAccess.item, 1);
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

            int pow = 1;
            DA.GetData(2, ref pow);

            if (pow <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Are you sure you want a power <= 0?");
            }

            GH_Structure<GH_Integer> cost_data = Euclidean.EuclideanCostTree(demands, supply, pow);

            DA.SetDataTree(0, cost_data);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => IconLoader.DistanceSymmIcon;//IconLoader.DistanceSymmIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1D4C7587-5D12-44AF-B21B-F4168018BB7B"); }
        }
    }
}