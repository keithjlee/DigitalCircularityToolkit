﻿using Accord.Statistics.Kernels;
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
        public static GH_Structure<GH_Integer> HullDiff3DCostTree(List<DesignObject> demand, List<DesignObject> supply)
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

                    double dist = HullDiff3D(dem.Hull3D, dem.LocalPlane, sup.Hull3D, sup.LocalPlane);
                    cost_tree.Append(new GH_Integer((int)dist), path);
                }
            }
            return cost_tree;
        }

        public static GH_Structure<GH_Integer> HullDiff2DCostTree(List<DesignObject> demand, List<DesignObject> supply)
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

                    double dist = Math.Ceiling(HullDiff2D(dem.Hull2D, dem.LocalPlane, sup.Hull2D, sup.LocalPlane));
                    cost_tree.Append(new GH_Integer((int)dist), path);
                }
            }
            return cost_tree;
        }

        public static double HullDiff3D(Mesh hull_demand, Plane plane_demand, Mesh hull_supply, Plane plane_supply)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);
            
            Mesh transformed_supply = (Mesh)hull_supply.Duplicate();
            transformed_supply.Transform(transformer);

            double v_demand = hull_demand.Volume();
            double v_supply = hull_supply.Volume();

            //CancellationToken ctoken = new CancellationToken();
            //bool success = Intersection.MeshMesh(new List<Mesh> { hull_demand, transformed_supply }, 1e-6, out _, false, out _, true, out Mesh intermesh, null, ctoken, null);

            //double cost;

            //if (success)
            //{
            //    try
            //    {
            //        double v_intersect = intermesh.Volume();

            //        var vdiff1 = Math.Abs(v_intersect - v_demand);
            //        var vdiff2 = Math.Abs(v_intersect - v_supply);

            //        cost = vdiff1 + vdiff2;
            //    }
            //    catch
            //    {
            //        cost = Math.Abs(hull_demand.Volume() - hull_supply.Volume());
            //    }
                
            //}
            //else
            //{
            //    cost = Math.Abs(hull_demand.Volume() - hull_supply.Volume());
            //}
            
            double cost = Math.Abs(v_demand - v_supply);
            //cost = cost < Int32.MaxValue ? cost : Int32.MaxValue;
            return Math.Ceiling(cost);
        }

        public static double HullDiff2D(Polyline hull_demand, Plane plane_demand, Polyline hull_supply, Plane plane_supply)
        {
            Transform transformer = Transform.PlaneToPlane(plane_supply, plane_demand);

            var transformed_supply = hull_supply.Duplicate();
            transformed_supply.Transform(transformer);

            Brep brep_demand = Brep.CreatePlanarBreps(hull_demand.ToNurbsCurve(), 1e-5)[0];
            Brep brep_supply = Brep.CreatePlanarBreps(transformed_supply.ToNurbsCurve(), 1e-5)[0];

            var intersect = Brep.CreatePlanarIntersection(brep_demand, brep_supply, plane_demand, 1e-5);

            double cost;
            if (intersect.Length != 0)
            {
                double v_intersect = 0;
                foreach (Brep intersection in intersect) v_intersect += intersection.GetArea();

                double v_excess = 0;
                var differences = Brep.CreateBooleanDifference(brep_supply, brep_demand, 1e-5);

                foreach (Brep diff in differences) v_excess += diff.GetArea();

                cost = Math.Abs(brep_demand.GetArea() - v_intersect + v_excess);
            }
            else
            {
                cost = Math.Abs(brep_demand.GetArea() - brep_supply.GetArea());
            }

            cost = cost < Int32.MaxValue ? cost : Int32.MaxValue;
            return cost;
        }

    }
}
