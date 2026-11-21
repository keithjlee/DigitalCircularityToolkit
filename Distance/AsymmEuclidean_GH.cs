using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public class AsymmEuclidean_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AsymmEuclidean_GH class.
        /// </summary>
        public AsymmEuclidean_GH()
          : base("AsymmEuclideanDistance (DCT)", "DMAsymm",
              "Generate a distance matrix of asymmetric Euclidean distances between two feature vector sets",
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
            pManager.AddIntegerParameter("Power1", "p", "Power of the distance when the supply is at least as large as the demand in every feature", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Power2", "q", "Power of the distance when the supply is smaller than the demand in any feature (penalty: use Power2 > Power1 with distances > 1)", GH_ParamAccess.item, 2);
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

            string dims_error = CostKernels.CheckDimensions(CostKernels.ToRows(demands), CostKernels.ToRows(supply));
            if (dims_error != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, dims_error);
                return;
            }

            int p = 1;
            int q = 2;
            DA.GetData(2, ref p);
            DA.GetData(3, ref q);

            if (p <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Are you sure you want a power <= 0?");
            }
            if (q <= p)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Power2 <= Power1: undersized supply is not penalized more than oversized supply");
            }


            GH_Structure<GH_Integer> cost_data = Euclidean.AsymmEuclideanCostTree(demands, supply, p, q);

            DA.SetDataTree(0, cost_data);
        }


        
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.DistanceAsymmIcon; //.DISTANCEASYMM;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("669170C3-521E-47A6-8CAC-82E5D9CAE3A0"); }
        }
    }
}