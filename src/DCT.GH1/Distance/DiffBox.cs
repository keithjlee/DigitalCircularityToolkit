using System;
using System.Collections.Generic;
using Rhino.Geometry;
using DigitalCircularityToolkit.Objects;
using DigitalCircularityToolkit.Utilities;
using Grasshopper;
using Grasshopper.Kernel;

namespace DigitalCircularityToolkit.Distance
{
	public static class DiffBox
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

			int[,] cost_matrix = new int[n_demand, n_supply];

			for (int i = 0; i < n_demand; i++)
			{
				for (int j = 0; j < n_supply; j++)
				{
					cost_matrix[i, j] = (int)(Math.Ceiling(DiffBoxPair(demands[i], supply[j])));
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

        public static double DiffBoxPair(DesignObject obj1, DesignObject obj2)
        {
            var dL = Math.Pow(obj1.Length - obj2.Length, 2);
            var dW = Math.Pow(obj1.Width - obj2.Width, 2);
            var dH = Math.Pow(obj1.Height - obj2.Height, 2);

            return Math.Sqrt(dL + dW + dH);
        }

        
    }
}

