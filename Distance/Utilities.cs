using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Distance
{
    public static class Utilities
    {
        /// <summary>
        /// Convert a square cost matrix to a [n_demand][n_supply] data tree that does not include any padded rows/cols
        /// </summary>
        /// <param name="cost_matrix"></param>
        /// <returns></returns>
        public static GH_Structure<GH_Integer> CostMatrix2CostTree(int[,] cost_matrix, int n_rows, int n_cols)
        {
            GH_Structure<GH_Integer> tree = new GH_Structure<GH_Integer>();

            for (int row = 0; row < n_rows; row++)
            {
                GH_Path path = new GH_Path(row);

                for (int col = 0; col < n_cols; col++)
                {
                    tree.Append(new GH_Integer(cost_matrix[row, col]), path);
                }
            }

            return tree;
        }

        public static GH_Structure<GH_Integer> Matrix2Tree(int[,] cost_matrix)
        {
            GH_Structure<GH_Integer> tree = new GH_Structure<GH_Integer>();

            int n_rows = cost_matrix.GetLength(0);
            int n_cols = cost_matrix.GetLength(1);

            for (int row = 0; row < n_rows; row++)
            {
                GH_Path path = new GH_Path(row);

                for (int col = 0; col < n_cols; col++)
                {
                    tree.Append(new GH_Integer(cost_matrix[row, col]), path);
                }
            }

            return tree;
        }

        public static int[,] CostTree2CostMatrix(GH_Structure<GH_Integer> tree, bool pad = true)
        {
            int n_demand = tree.PathCount;
            int n_supply = tree.get_Branch(0).Count;

            int[,] cost_matrix = new int[n_demand, n_supply];

            for (int i = 0; i < n_demand; i++)
            {
                GH_Path path = tree.get_Path(i);
                for (int j = 0; j < n_supply; j++)
                {
                    cost_matrix[i, j] = tree.get_DataItem(path, j).Value;
                }
            }

            if (n_demand != n_supply && pad)
            {
                if (n_demand < n_supply)
                {
                    return PadRows(cost_matrix);
                }
                else
                {
                    return PadCols(cost_matrix);
                }
            }
            else
            {
                return cost_matrix;
            }
        }

        public static List<int[]> Tree2List(GH_Structure<GH_Integer> tree)
        {
            List<int[]> list_data = new List<int[]>();

            for (int i = 0; i < tree.PathCount; i++)
            {
                List<int> row = new List<int>();
                GH_Path path = tree.get_Path(i);

                for (int j = 0; j < tree.get_Branch(path).Count; j++)
                {
                    GH_Integer value = tree.get_DataItem(path, j);
                    row.Add(value.Value);
                }

                list_data.Add(row.ToArray());
            }

            return list_data;
        }

        public static List<double[]> Tree2List(GH_Structure<GH_Number> tree)
        {
            List<double[]> list_data = new List<double[]>();

            for (int i = 0; i < tree.PathCount; i++)
            {
                List<double> row = new List<double>();
                GH_Path path = tree.get_Path(i);

                for (int j = 0; j < tree.get_Branch(path).Count; j++)
                {
                    GH_Number value = tree.get_DataItem(path, j);
                    row.Add(value.Value);
                }

                list_data.Add(row.ToArray());
            }

            return list_data;
        }

        public static int[,] PadRows(int[,] cost_matrix)
        {
            int n = cost_matrix.GetLength(0);
            int m = cost_matrix.GetLength(1);

            int[,] sq_costs = new int[m, m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i >= n)
                    {
                        sq_costs[i, j] = Int32.MaxValue;
                    }
                    else
                    {
                        sq_costs[i, j] = cost_matrix[i, j];
                    }
                }
            }

            return sq_costs;
        }

        public static int[,] PadCols(int[,] cost_matrix)
        {
            int n = cost_matrix.GetLength(0);
            int m = cost_matrix.GetLength(1);

            int[,] sq_costs = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j >= m)
                    {
                        sq_costs[i, j] = Int32.MaxValue;
                    }
                    else
                    {
                        sq_costs[i, j] = cost_matrix[i, j];
                    }
                }
            }

            return sq_costs;
        }

        public static int[] AssignmentIndices(int[] init_assignments, int n_demand, int n_supply)
        {
            int[] assignments = new int[n_demand];

            for (int i = 0; i < n_demand; i++)
            {
                int assignment = init_assignments[i];

                if (assignment >= n_supply)
                {
                    assignments[i] = -1;
                }
                else
                {
                    assignments[i] = assignment;
                }
            }

            return assignments;
        }

        public static bool CheckDims(GH_Structure<GH_Number> dem, GH_Structure<GH_Number> sup)
        {
            int d1 = dem.Branches[0].Count;
            int d2 = sup.Branches[0].Count;

            if (d1 == d2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
