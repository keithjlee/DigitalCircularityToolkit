using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Matching
{
    public class Hungarian_GH_OBSOLETE : GH_Component
    {
        int[] assignments; //container for assignment indices
        double total_cost; //container for total cost
        
        /// <summary>
        /// Initializes a new instance of the Hungarian class.
        /// </summary>
        public Hungarian_GH_OBSOLETE()
          : base("Hungarian (DCT)", "Hungarian",
              "Hungarian matching algorithm",
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

            string invalid = AssignmentHelpers.Validate(dm, out int n_demand, out int n_supply);
            if (invalid != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, invalid);
                return;
            }

            if (n_demand > n_supply)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "|demand| > |supply|, not all demands will be assigned a real supply item");
            }

            if (auto)
            {
                // In-house solver since v0.6 (was the HungarianAlgorithm package): same optimal total cost.
                Numerics.AssignmentResult result = Numerics.LinearAssignment.Solve(AssignmentHelpers.ToCostMatrix(dm, n_demand, n_supply), true, Numerics.AssignmentMethod.Dense);
                assignments = result.Assignment;
                total_cost = result.Cost;
                int unassigned = result.Unassigned;

                if (unassigned > 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Format(
                        "{0} demand item(s) have no supply item: their Assignment is -1, and Cost excludes them", unassigned));
                }
            }
            else if (assignments == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "AutoRun is false and no assignment has been computed yet. Set AutoRun to true (e.g. press the button) to run.");
                return;
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
                return IconLoader.HungarianIcon; //.HUNGARIAN;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D93D70FA-F26F-4ED8-A35D-9FFE58E460D4"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
    }
}