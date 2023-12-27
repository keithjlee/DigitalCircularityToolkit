using Accord.Statistics.Kernels;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Distance
{
    internal static class HullDifference
    {
        //public static GH_Structure<GH_Integer> HullDiffCostTree(List<DesignObject> demand, List<DesignObject> supply)
        //{
        //    int n_demand = demand.Count;
        //    int n_supply = supply.Count;

        //    GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

        //    for (int i = 0; i < n_demand; i++)
        //    {
        //        GH_Path path = new GH_Path(i);
        //        for (int j = 0; j < n_supply; j++)
        //        {
        //            var dem = demand[i];
        //            var sup = supply[j];

        //            double dist = Math.Ceiling(HullDiff(dem.Hull, dem.LocalPlane, sup.Hull, sup.LocalPlane));
        //            cost_tree.Append(new GH_Integer((int)dist), path);
        //        }
        //    }
        //    return cost_tree;
        //}

        public static double HullDiff(Mesh hull_demand, Plane plane_demand, Mesh hull_supply, Plane plane_supply)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);
            
            var transformed_hull = hull_supply.Duplicate();
            transformed_hull.Transform(transformer);

            var intersect = Mesh.CreateBooleanIntersection(new List<Mesh> { hull_demand}, new List<Mesh> { hull_supply });

            double cost;
            try
            {
                Mesh mesh_intersect = intersect[0];

                double v_intersect = mesh_intersect.Volume();
                cost = Math.Abs(v_intersect - hull_demand.Volume());
            }
            catch
            {
                cost = hull_demand.Volume();
            }

            return cost;
        }

        public static double HullDiff(Mesh hull_demand, Plane plane_demand, Polyline hull_supply, Plane plane_supply)
        {
            return hull_demand.Volume();
        }

        public static double HullDiff(Polyline hull_demand, Plane plane_demand, Polyline hull_supply, Plane plane_supply)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);

            var transformed_hull = hull_supply.Duplicate();
            transformed_hull.Transform(transformer);

            Brep brep_demand = Brep.CreatePlanarBreps(hull_demand.ToNurbsCurve(), 1e-4)[0];
            Brep brep_supply = Brep.CreatePlanarBreps(transformed_hull.ToNurbsCurve(), 1e-4)[0];

            var brep_intersect = Brep.CreatePlanarIntersection(brep_demand, brep_supply, plane_demand, 1e-4);

            double cost;
            try
            {
                Brep brep = brep_intersect[0];

                double a_intersect = brep.GetArea();
                cost = Math.Abs(a_intersect - brep_demand.GetArea()); 
            }
            catch
            {
                cost = brep_demand.GetArea();
            }

            return cost;
        }

        public static double HullDiff(Polyline hull_demand, Plane plane_demand, Mesh hull_supply, Plane plane_supply)
        {
            return Brep.CreatePlanarBreps(hull_demand.ToNurbsCurve(), 1e-4)[0].GetArea();
        }
    }
}
