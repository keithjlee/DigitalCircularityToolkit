using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Google.OrTools.Graph;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace DigitalCircularityToolkit.Experimental
{
    public class Hungarian2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Hungarian2 class.
        /// </summary>
        public Hungarian2()
          : base("Hungarian2", "Hungarian2",
              "Assignment algorithm using OrTools LinearSumAsssignment",
              "DigitalCircularityToolkit", "Experimental")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("DistanceMatrix", "DM", "Distance matrix as tree: number of branches = number of demand; count in each branch = number of supply. If assignment = -1, no real assigment occurred.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Assignment", "A", "Assignment indices. A[i] = j: assign inventory element j to demand element i", GH_ParamAccess.list);
            pManager.AddNumberParameter("Cost", "C", "Total cost of assingment", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_Integer> dm)) return;
            int n_demand = dm.PathCount;
            int n_supply = dm.get_Branch(0).Count;

            if (n_demand > n_supply)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "|demand| > |supply|, not all demands will be assigned a real supply item");
            }

            int[,] cost_matrix = Distance.Utilities.CostTree2CostMatrix(dm);
            int[,] cost_matrix_clone = (int[,])cost_matrix.Clone();

            // CODE IS EXACTLY THE SAME AS HUNGARIAN_GH.CS UNTIL ABOVE


            // FOLLOWING: https://developers.google.com/optimization/assignment/linear_assignment#c

            // CREATE SOLVER
            LinearSumAssignment assignment = new LinearSumAssignment();

            // ADD ARC
            //int n = cost_matrix.GetLength(0); // symmetric matrix, so this suffices
            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < n; j++)
            //    {
            //        assignment.AddArcWithCost(i, j, cost_matrix[i, j]);
            //    }
            //}

            // SOLVE
            //LinearSumAssignment.Status status = assignment.Solve();

            int[,] costs = {
            { 90, 76, 75, 70 },
            { 35, 85, 55, 65 },
            { 125, 95, 90, 105 },
            { 45, 110, 95, 115 },
        };
            int numWorkers = 4;
            int[] allWorkers = Enumerable.Range(0, numWorkers).ToArray();
            int numTasks = 4;
            int[] allTasks = Enumerable.Range(0, numTasks).ToArray();

            // Add each arc.
            foreach (int w in allWorkers)
            {
                foreach (int t in allTasks)
                {
                    if (costs[w, t] != 0)
                    {
                        assignment.AddArcWithCost(w, t, costs[w, t]);
                    }
                }
            }

            LinearSumAssignment.Status status = assignment.Solve();

            // GET ASSIGNMENTS
            int[] assigned_inventory = new int[n_demand];
            for (int i = 0; i < n_demand; i++)
            {
                assigned_inventory[i] = assignment.RightMate(i);
            }

            DA.SetDataList(0, assigned_inventory);
            DA.SetData(1, assignment.OptimalCost());

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
            get { return new Guid("F780DB0D-2AD1-42BB-9B41-557208D0DFC3"); }
        }
    }
}