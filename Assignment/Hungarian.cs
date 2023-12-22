using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using HungarianAlgorithm;
using DigitalCircularityToolkit.Utilities;

namespace DigitalCircularityToolkit.Assignment
{
  public class Hungarian : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public Hungarian()
      : base("Hungarian", "Hungarian",
        "Hungarian description",
        "DigitalCircularityToolkit", "Assignment")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddGenericParameter("Demand", "D", "Demand objects", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supply", "S", "Supply objects", GH_ParamAccess.list);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
            pManager.AddIntegerParameter("Assignment", "A", "Assignment indices", GH_ParamAccess.list);
            pManager.AddIntegerParameter("CostMatrix", "CM", "Cost Matrix", GH_ParamAccess.tree);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
            // Initialize
            List<DesignObject> demands = new List<DesignObject>();
            List<DesignObject> supply = new List<DesignObject>();

            // Assign
            if (!DA.GetDataList(0, demands)) return;
            if (!DA.GetDataList(1, supply)) return;

            if (demands.Count > supply.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "|Demand| must be <= |Supply|");
            }

            int[,] cost_matrix = Cost.DiffBoxCostMatrix(demands, supply);

            // convert to gh tree to output
            GH_Structure<GH_Integer> cost_matrix_tree = new GH_Structure<GH_Integer>();

            for (int col = 0; col < demands.Count; col++)
            {
                GH_Path path = new GH_Path(col);
                for (int row = 0; row < supply.Count; row++)
                {
                    cost_matrix_tree.Append(new GH_Integer(cost_matrix[row, col]), path);
                }
            }

            int[] supply_to_demand_expanded = HungarianAlgorithm.HungarianAlgorithm.FindAssignments(cost_matrix);
            int[] assignment_indices = demand_to_supply(supply_to_demand_expanded, demands.Count);

            DA.SetDataList(0, assignment_indices);
            DA.SetDataTree(1, cost_matrix_tree);
    }

    private int[] demand_to_supply(int[] supply_to_demand, int n_demand)
        {
            int[] demand_to_supply = new int[n_demand];

            for (int i = 0; i < supply_to_demand.Length; i++)
            {
                var val = supply_to_demand[i];

                if (val >= n_demand)
                {
                    continue;
                }
                else
                {
                    demand_to_supply[val] = i;
                }
            }

            return demand_to_supply;
        }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      { 
        // You can add image files to your project resources and access them like this:
        //return Resources.IconForThisComponent;
        return null;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("513d2334-94df-40ad-8041-27ce7ad7ce6f"); }
    }
  }
}
