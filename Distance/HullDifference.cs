using Accord.Statistics.Kernels;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Distance
{
    internal static class HullDifference
    {
        public static GH_Structure<GH_Integer> HullDiff3DCostTree(List<DesignObject> demand, List<DesignObject> supply, double factor)
        {
            int n_demand = demand.Count;
            int n_supply = supply.Count;

            GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

            for (int i = 0; i < n_demand; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < n_supply; j++)
                {
                    var dem = demand[i];
                    var sup = supply[j];

                    double dist = HullDiff3D(dem.Hull3D, dem.LocalPlane, sup.Hull3D, sup.LocalPlane, factor);
                    cost_tree.Append(new GH_Integer((int)dist), path);
                }
            }
            return cost_tree;
        }

        public static GH_Structure<GH_Integer> HullDiff2DCostTree(List<DesignObject> demand, List<DesignObject> supply, double factor)
        {
            int n_demand = demand.Count;
            int n_supply = supply.Count;

            GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

            for (int i = 0; i < n_demand; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < n_supply; j++)
                {
                    var dem = demand[i];
                    var sup = supply[j];

                    double dist = HullDiff2D(dem.Hull2D, dem.LocalPlane, sup.Hull2D, sup.LocalPlane, factor);
                    cost_tree.Append(new GH_Integer((int)dist), path);
                }
            }
            return cost_tree;
        }

        /// <summary>
        /// Get the distance in aligned 3D hulls between two objects. 0 = perfect match, 1 = no match at all
        /// </summary>
        /// <param name="hull_demand"></param>
        /// <param name="plane_demand"></param>
        /// <param name="hull_supply"></param>
        /// <param name="plane_supply"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static double HullDiff3D(Mesh hull_demand, Plane plane_demand, Mesh hull_supply, Plane plane_supply, double factor)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);

            hull_supply = (Mesh)hull_supply.Duplicate();
            hull_supply.Transform(transformer);

            double v_demand = hull_demand.Volume();
            double v_supply = hull_supply.Volume();

            // intersection
            Mesh[] intersections = Mesh.CreateBooleanIntersection(new List<Mesh> { hull_demand }, new List<Mesh> { hull_supply });

            double dist;

            if (intersections == null || intersections.Length == 0)
            {
                double ratio = v_demand > v_supply ? v_supply / v_demand : v_demand / v_supply;

                dist = 1 - ratio;
            }
            else
            {
                double v_intersection = 0;
                foreach (Mesh intersect in intersections) v_intersection += intersect.Volume();

                double alpha_demand = v_demand / (v_demand + v_supply);
                double alpha_supply = 1 - alpha_demand;

                dist = alpha_demand * (1 - v_intersection / v_demand) + alpha_supply * (1 - v_intersection / v_supply);

            }
            return Math.Ceiling(dist * factor);
        }

        /// <summary>
        /// Get the distance in aligned planar 2D hulls. 0 = perfect match; 1 = zero match
        /// </summary>
        /// <param name="hull_demand"></param>
        /// <param name="plane_demand"></param>
        /// <param name="hull_supply"></param>
        /// <param name="plane_supply"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static double HullDiff2D(Polyline hull_demand, Plane plane_demand, Polyline hull_supply, Plane plane_supply, double factor)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);

            hull_supply = hull_supply.Duplicate();
            hull_supply.Transform(transformer);

            Brep brep_demand = Brep.CreatePlanarBreps(hull_demand.ToNurbsCurve(), 1e-5)[0];
            double a_demand = brep_demand.GetArea();

            Brep brep_supply = Brep.CreatePlanarBreps(hull_supply.ToNurbsCurve(), 1e-5)[0];
            double a_supply = brep_supply.GetArea();

            Brep[] intersections = Brep.CreatePlanarIntersection(brep_demand, brep_supply, plane_demand, 1e-5);

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
            }

            return Math.Ceiling(dist * factor);
        }

    }
}
