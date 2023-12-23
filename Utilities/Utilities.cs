using System;
using System.Linq;

namespace DigitalCircularityToolkit.Utilities
{
	public static class Utilities
	{

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

