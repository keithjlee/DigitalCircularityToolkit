using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Matching
{
    public class MatchLines_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MatchLines class.
        /// </summary>
        public MatchLines_GH()
          : base("MatchLines (DCT)", "MatchLines",
              "Get the lines that connect supply-demand assignments",
              "DigitalCircularityToolkit", "Matching")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("DemandPoints", "pDemand", "Points that represent the demand elements", GH_ParamAccess.list);
            pManager.AddPointParameter("SupplyPoints", "pSupply", "Points that represent the inventory elements", GH_ParamAccess.list);
            pManager.AddIntegerParameter("AssignmentIndices", "A", "Assignment indices of supply elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("MatchLines", "Lines", "Matching lines", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Initialize
            List<Point3d> pdemand = new List<Point3d>();
            List<Point3d> psupply = new List<Point3d>();
            List<int> ids = new List<int>();

            if (!DA.GetDataList(0, pdemand)) return;
            if (!DA.GetDataList(1, psupply)) return;
            if (!DA.GetDataList(2, ids)) return;

            if (pdemand.Count != ids.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                    "Length of demand points ({0}) must equal length of assignment indices ({1})", pdemand.Count, ids.Count));
                return;
            }

            // One entry per demand point, so the output stays parallel to the demand list.
            // Unassigned demand (index -1, demand > supply) gives a null entry.
            var lines = new List<GH_Line>(pdemand.Count);
            int unassigned = 0;

            for (int i = 0; i < pdemand.Count; i++)
            {
                int j = ids[i];
                if (j == -1)
                {
                    lines.Add(null);
                    unassigned++;
                    continue;
                }
                if (j < 0 || j >= psupply.Count)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                        "Assignment index {0} at position {1} is out of range: there are {2} supply points", j, i, psupply.Count));
                    return;
                }
                lines.Add(new GH_Line(new Line(pdemand[i], psupply[j])));
            }

            if (unassigned > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, string.Format(
                    "{0} demand point(s) are unassigned (index -1); their lines are null", unassigned));
            }

            DA.SetDataList(0, lines);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.MatchLinesIcon; //.MATCHLINES;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7B8F25E9-BA41-4A9A-87CA-811146DC3E46"); }
        }
    }
}