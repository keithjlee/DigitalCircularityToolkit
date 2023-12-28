using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Matching
{
    public class MatchLines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MatchLines class.
        /// </summary>
        public MatchLines()
          : base("MatchLines", "MatchLines",
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Length of demand points must equal length of assignment indices");
            }

            List<Line> lines = new List<Line>();

            for (int i = 0; i < pdemand.Count; i++)
            {
                Line line = new Line(pdemand[i], psupply[ids[i]]);
                lines.Add(line);
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
            get { return new Guid("7B8F25E9-BA41-4A9A-87CA-811146DC3E46"); }
        }
    }
}