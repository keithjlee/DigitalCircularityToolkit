using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Distance
{
    public class HullDifference_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HullDifference class.
        /// </summary>
        public HullDifference_GH()
          : base("HullDifference", "DMHull",
              "Get the difference between two aligned hulls",
              "DigitalCircularityToolkit", "Distance")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Demand", "D", "Demand objects", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supply", "S", "Inventory objects", GH_ParamAccess.list);
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
            get { return new Guid("725A9E44-A95A-4347-8B65-D721137EB7ED"); }
        }
    }
}