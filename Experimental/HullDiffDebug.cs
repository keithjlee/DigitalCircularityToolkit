using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using DigitalCircularityToolkit.Distance;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Experimental
{
    public class HullDiffDebug : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HullDiffDebug class.
        /// </summary>
        public HullDiffDebug()
          : base("HullDiffDebug", "HullDiffDebug",
              "Debugging hulldiff3d",
              "DigitalCircularityToolkit", "Experimental")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Demand", "D", "Demand objects", GH_ParamAccess.item);
            pManager.AddGenericParameter("Supply", "S", "Supply objects", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("HullDemand", "HullDemand", "Convex hull of demand object", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ademand", "Ademand", "Area of demand hull", GH_ParamAccess.item);
            pManager.AddBrepParameter("HullSupply", "HullSupply", "Convex hull of supply object", GH_ParamAccess.item);
            pManager.AddNumberParameter("Asupply", "Asupply", "Area of supply hull", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "d(D,S)", "Distance between demand and supply", GH_ParamAccess.item);
            pManager.AddBrepParameter("Intersection", "Intersect", "Intersection mesh", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject demand = new DesignObject();
            DesignObject supply = new DesignObject();

            if (!DA.GetData(0, ref demand)) return;
            if (!DA.GetData(1, ref supply)) return;

            Polyline hull_demand = demand.Hull2D;
            Polyline hull_supply = supply.Hull2D;

            Transform transformer = Transform.PlaneToPlane(supply.LocalPlane, demand.LocalPlane);


            hull_supply = hull_supply.Duplicate();
            hull_supply.Transform(transformer);

            Brep brep_demand = Brep.CreatePlanarBreps(hull_demand.ToNurbsCurve(), 1e-5)[0];
            double a_demand = brep_demand.GetArea();

            Brep brep_supply = Brep.CreatePlanarBreps(hull_supply.ToNurbsCurve(), 1e-5)[0];
            double a_supply = brep_supply.GetArea();

            Brep[] intersections = Brep.CreatePlanarIntersection(brep_demand, brep_supply, demand.LocalPlane, 1e-5);

            double dist;
            if (intersections == null || intersections.Length == 0)
            {
                double ratio = a_demand > a_supply ? a_supply / a_demand : a_demand / a_supply;

                dist = 1 - ratio;
            }
            else
            {
                double a_intersection = 0;
                foreach (Brep intersection in intersections) a_intersection += intersection.GetArea();

                double alpha_demand = a_demand / (a_demand + a_supply);
                double alpha_supply = 1 - alpha_demand;

                dist = alpha_demand * (1 - a_intersection / a_demand) + alpha_supply * (1 - a_intersection / a_supply);

                DA.SetDataList(5, intersections);
            }

            DA.SetData(0, hull_demand);
            DA.SetData(1, a_demand);
            DA.SetData(2, hull_supply);
            DA.SetData(3, a_supply);
            DA.SetData(4, dist);
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
            get { return new Guid("B0E07F77-599E-47DA-A751-9AA67B70F3B3"); }
        }
    }
}