using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Assignment
{
    public class Hungarian : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Hungarian class.
        /// </summary>
        public Hungarian()
          : base("Hungarian", "Hungarian",
              "Hungarian matching algorithm",
              "DigitalCircularityToolkit", "Assignment")
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
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_Integer> dm)) return;

            int[,] cost_matrix = Distance.Utilities.CostTree2CostMatrix(dm);
            int[] full_assignments = HungarianAlgorithm.HungarianAlgorithm.FindAssignments(cost_matrix);

            int n_demand = dm.PathCount;
            int n_supply = dm.get_Branch(0).Count;

            int[] assignments = Distance.Utilities.AssignmentIndices(full_assignments, n_demand, n_supply);

            DA.SetDataList(0, assignments);
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
            get { return new Guid("D93D70FA-F26F-4ED8-A35D-9FFE58E460D4"); }
        }
    }
}