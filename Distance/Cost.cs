using System;
using System.Collections.Generic;
using Rhino.Geometry;
using DigitalCircularityToolkit.Objects;
using DigitalCircularityToolkit.Utilities;
using Grasshopper;
using Grasshopper.Kernel;

namespace DigitalCircularityToolkit.Distance
{
	public static class Cost
	{
		/// <summary>
		/// Rows: supply; Columns: demand
		/// </summary>
		/// <param name="demands"></param>
		/// <param name="supply"></param>
		/// <returns></returns>
		public static int[,] DiffBoxCostMatrix(List<DesignObject> demands, List<DesignObject> supply)
		{
			int n_demand = demands.Count;
			int n_supply = supply.Count;

			int[,] cost_matrix = new int[n_supply, n_demand];

			for (int i = 0; i < n_supply; i++)
			{
				for (int j = 0; j < n_demand; j++)
				{
					cost_matrix[i, j] = (int)(Math.Ceiling(DiffBox(supply[i], demands[j])));
				}
			}

			if (n_demand != n_supply)
			{
				return SquareCostMatrix(cost_matrix);
			}

			return cost_matrix;
		}

		public static double DiffBox(DesignObject obj1, DesignObject obj2)
		{
			var dL = Math.Pow(obj1.Length - obj2.Length, 2);
			var dW = Math.Pow(obj1.Width - obj2.Width, 2);
			var dH = Math.Pow(obj1.Height - obj2.Height, 2);

			return Math.Sqrt(dL + dW + dH);
		}

        public static int[,] SquareCostMatrix(int[,] cost_matrix)
        {

            // m >= n
            int m = cost_matrix.GetLength(0); // m is the number of supply (rows)
            int n = cost_matrix.GetLength(1); // n is the number of demand (cols) 

            // generate the cost matrix
            int[,] costs = new int[m, m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (j >= n)
                    {
                        // fill the matrix to be square
                        costs[i, j] = (int)Int32.MaxValue;
                    }
                    else
                    {
                        costs[i, j] = cost_matrix[i, j];
                    }
                }
            }

            return costs;
        }
    }
}

