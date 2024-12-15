using DigitalCircularityToolkit.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace DigitalCircularityToolkit.Distance
{
    public static class Euclidean
    {
        /// <summary>
        /// Get the |ndemand| x |nsupply| cost matrix DM where DM[i,j] is the distance between the demand and supply points raised to a power
        /// </summary>
        /// <param name="demands"></param>
        /// <param name="supply"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int[,] EuclideanCostMatrix(List<double[]> demands, List<double[]> supply, int power)
        {
            int n_demand = demands.Count;
            int n_supply = supply.Count;

            int[,] cost_matrix = new int[n_demand, n_supply];

            for (int i = 0; i < n_demand; i++)
            {
                for (int j = 0; j < n_supply; j++)
                {
                    double dist = EuclideanDist(demands[i], supply[j]);

                    cost_matrix[i, j] = (int)(Math.Ceiling(Math.Pow(dist, power)));
                }
            }

            if (n_demand != n_supply)
            {
                if (n_demand > n_supply)
                {
                    return Utilities.PadCols(cost_matrix);
                }
                else
                {
                    return Utilities.PadRows(cost_matrix);
                }
            }

            return cost_matrix;
        }

        /// <summary>
        /// Get the |ndemand| x |nsupply| cost matrix DM in data tree format where DM.Branches[i].Values[j] is the distance between the demand and supply points raised to a power
        /// </summary>
        /// <param name="demands"></param>
        /// <param name="supply"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static GH_Structure<GH_Integer> EuclideanCostTree(List<double[]> demands, List<double[]> supply, int power)
        {
            int n_demand = demands.Count;
            int n_supply = supply.Count;

            GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

            for (int i = 0; i < n_demand; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < n_supply; j++)
                {
                    double dist = Math.Ceiling(Math.Pow(EuclideanDist(demands[i], supply[j]), power));
                    cost_tree.Append(new GH_Integer((int)dist), path);
                }
            }
            return cost_tree;
        }

        

        /// <summary>
        /// Get the |ndemand| x |nsupply| cost matrix DM in data tree format where DM.Branches[i].Values[j] is the distance between the demand and supply points raised to a power
        /// </summary>
        /// <param name="demands"></param>
        /// <param name="supply"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static GH_Structure<GH_Integer> EuclideanCostTree(GH_Structure<GH_Number> demands, GH_Structure<GH_Number> supply, int power)
        {
            int n_demand = demands.PathCount;
            int n_supply = supply.PathCount;

            GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

            for (int i = 0; i < n_demand; i++)
            {
                var d = demands.Branches[i].Cast<GH_Number>();

                GH_Path path = new GH_Path(i);

                for (int j = 0; j < n_supply; j++)
                {
                    var s = supply.Branches[j].Cast<GH_Number>();
                    int dist = (int)Math.Ceiling(Math.Pow(EuclideanDist(d, s), power));

                    dist = dist < Int32.MaxValue ? dist : Int32.MaxValue;

                    cost_tree.Append(new GH_Integer(dist), path);
                }
            }
            return cost_tree;
        }

        /// <summary>
        /// Get the |ndemand| x |nsupply| cost matrix DM in data tree format where DM.Branches[i].Values[j] is the distance from demand to supply raised to a power p if all components are greater than 0 and q otherwise, with q > p
        /// </summary>
        /// <param name="demands"></param>
        /// <param name="supply"></param>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GH_Structure<GH_Integer> AsymmEuclideanCostTree(GH_Structure<GH_Number> demands, GH_Structure<GH_Number> supply, int p, int q)
        {
            int n_demand = demands.PathCount;
            int n_supply = supply.PathCount;

            GH_Structure<GH_Integer> cost_tree = new GH_Structure<GH_Integer>();

            for (int i = 0; i < n_demand; i++)
            {
                var d = demands.Branches[i].Cast<GH_Number>();

                GH_Path path = new GH_Path(i);

                for (int j = 0; j < n_supply; j++)
                {
                    var s = supply.Branches[j].Cast<GH_Number>();
                    int dist = (int)Math.Ceiling(AsymmEuclideanDist(d, s, p, q));

                    dist = dist < Int32.MaxValue ? dist : Int32.MaxValue;

                    cost_tree.Append(new GH_Integer(dist), path);
                }
            }
            return cost_tree;
        }

        public static double EuclideanDist(IEnumerable<double> x1, IEnumerable<double> x2)
        {
            int n_dims = x1.Count();

            double dist = 0;

            for (int i = 0; i < n_dims; i++)
            {
                dist += Math.Pow(x1.ElementAt(i) - x2.ElementAt(i), 2);
            }

            return Math.Sqrt(dist);
        }

        public static double AsymmEuclideanDist(IEnumerable<double> x_source, IEnumerable<double> x_target, int p, int q)
        {
            int n_dims = x_source.Count();

            double dist = 0;

            bool valid = true;
            for (int i = 0; i < n_dims; i++)
            {
                double diff = x_target.ElementAt(i) - x_source.ElementAt(i);
                if (diff < 0) valid = false;

                dist += Math.Pow(diff, 2);
            }

            if (valid)
            {
                return Math.Pow(Math.Sqrt(dist), p);
            }
            else
            {
                return Math.Pow(Math.Sqrt(dist), q);
            }
        }

        public static double EuclideanDist(IEnumerable<GH_Number> x1, IEnumerable<GH_Number> x2)
        {
            int n_dims = x1.Count();

            double dist = 0;

            for (int i = 0; i < n_dims; i++)
            {
                dist += Math.Pow(x1.ElementAt(i).Value - x2.ElementAt(i).Value, 2);
            }

            return Math.Sqrt(dist);
        }

        public static double AsymmEuclideanDist(IEnumerable<GH_Number> x_source, IEnumerable<GH_Number> x_target, int p, int q)
        {
            int n_dims = x_source.Count();

            double dist = 0;

            bool valid = true;

            for (int i = 0; i < n_dims; i++)
            {
                double diff = x_target.ElementAt(i).Value - x_source.ElementAt(i).Value;
                if (diff < 0) valid = false;

                dist += Math.Pow(diff, 2);
            }

            if (valid)
            {
                return Math.Pow(Math.Sqrt(dist), p);
            }
            else
            {
                return Math.Pow(Math.Sqrt(dist), q);
            }
        }


    }
}
