namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>Helpers for assignment results. An entry of -1 means "demand row not assigned" (demand &gt; supply).</summary>
    public static class Assignment
    {
        /// <summary>Total cost over assigned rows only; counts rows with no (valid) assignment.</summary>
        public static double AssignedCost(int[,] cost, int[] assignment, out int unassigned)
        {
            unassigned = 0;
            double total = 0;
            int nSupply = cost.GetLength(1);
            for (int i = 0; i < assignment.Length; i++)
            {
                int j = assignment[i];
                if (j < 0 || j >= nSupply)
                {
                    unassigned++;
                    continue;
                }
                total += cost[i, j];
            }
            return total;
        }
    }
}
