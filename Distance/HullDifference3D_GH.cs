﻿using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public class HullDifference3D_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HullDifference3D_GH class.
        /// </summary>
        public HullDifference3D_GH()
          : base("HullDifference3D_GH", "DMHull3D",
              "Get the difference between two aligned volumetric hulls",
              "DigitalCircularityToolkit", "Distance")
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
            pManager.AddIntegerParameter("DistanceMatrix", "DM", "Distance matrix data where row [i,j] is distance between demand[i] and supply[j]", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<DesignObject> demand = new List<DesignObject>();
            List<DesignObject> supply = new List<DesignObject>();

            if (!DA.GetDataList(0, demand)) return;
            if (!DA.GetDataList(1, supply)) return;

            GH_Structure<GH_Integer> dm = HullDifference.HullDiff3DCostTree(demand, supply);

            DA.SetDataTree(0, dm);
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
            get { return new Guid("D13BF45C-B440-494F-B82F-A5F212C4C3A3"); }
        }
    }
}