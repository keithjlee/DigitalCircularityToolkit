using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using LapjvCSharp;

namespace DigitalCircularityToolkit.Matching
{
    public class ShortestPath : GH_Component
    {
        int[] assignments; //container for assignment indices
        double total_cost; //container for total cost

        /// <summary>
        /// Initializes a new instance of the ShortestPath class.
        /// </summary>
        public ShortestPath()
          : base("ShortestPath (DCT)", "ShortestPath",
              "Shortest Path matching algorithm",
              "DigitalCircularityToolkit", "Matching")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("DistanceMatrix", "DM", "Distance matrix as tree: number of branches = number of demand; count in each branch = number of supply. If assignment = -1, no real assignment occurred.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("AutoRun", "Auto", "Automatically run the algorithm. Set to false when working with large parametric problems so that you can manually perform the assignment between parameter changes. I.e., add a button here.", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Assignment", "A", "Assignment indices. A[i] = j: assign inventory element j to demand element i", GH_ParamAccess.list);
            pManager.AddNumberParameter("Cost", "C", "Total cost of assignment", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool auto = true;
            if (!DA.GetDataTree(0, out GH_Structure<GH_Integer> dm)) return;
            DA.GetData(1, ref auto);

            int n_demand = dm.PathCount;
            int n_supply = dm.get_Branch(0).Count;

            if (n_demand > n_supply)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "|demand| > |supply|, not all demands will be assigned a real supply item");
            }

            LapjvCSharp.Lapjv lap = new LapjvCSharp.Lapjv();
            if (auto){

                int[,] cost_matrix_int = Distance.Utilities.CostTree2CostMatrix(dm);
                double[,] cost_matrix = new double[n_demand, n_supply];
                for (int i = 0; i < n_demand; i++)
                {
                    for (int j = 0; j < n_supply; j++)
                    {
                        cost_matrix[i, j] = (double)cost_matrix_int[i, j];
                    }
                }

                double[,] cost_matrix_clone = (double[,])cost_matrix.Clone();
                
                (int[] full_assignments, int[] col_assignments) = lap.lapjvCsharp(cost_matrix);
                assignments = Distance.Utilities.AssignmentIndices(full_assignments, n_demand, n_supply);

                total_cost = 0;

                for (int i = 0; i < n_demand; i++)
                {
                    total_cost += cost_matrix_clone[i, assignments[i]];
                }
            }

            

            DA.SetDataList(0, assignments);
            DA.SetData(1, total_cost);
            
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
            get { return new Guid("00A166A1-0044-4E16-AB8E-415FE55E2E1F"); }
        }
    }
}