using DigitalCircularityToolkit.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Distance
{
    public static class Euclidean
    {
        public static int[,] EuclideanCostMatrix(List<double[]> demands, List<double[]> supply)
        {
            int n_demand = demands.Count;
            int n_supply = supply.Count;

            int[,] cost_matrix = new int[n_demand, n_supply];

            for (int i = 0; i < n_demand; i++)
            {
                for (int j = 0; j < n_supply; j++)
                {
                    cost_matrix[i, j] = (int)(Math.Ceiling(EuclideanDist(demands[i], supply[j])));
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

        public static double PNormDist(IEnumerable<double> x1, IEnumerable<double> x2, int p)
        {
            int n_dims = x1.Count();

            double dist = 0;

            for (int i = 0; i < n_dims; i++)
            {
                dist += Math.Pow(Math.Abs(x1.ElementAt(i) - x2.ElementAt(i)), p);
            }

            return Math.Pow(dist, 1/p);
        }
    }
}
