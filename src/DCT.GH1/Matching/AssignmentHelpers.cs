using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace DigitalCircularityToolkit.Matching
{
    /// <summary>
    /// Shared checks for the assignment components (COST_MATRIX_SPEC §5).
    /// An assignment entry of -1 means "demand row not assigned" (demand &gt; supply).
    /// </summary>
    internal static class AssignmentHelpers
    {
        /// <summary>
        /// Validates a GH cost tree: at least one branch, at least one item, all branches the same length.
        /// Returns null when valid, otherwise a message for an Error.
        /// </summary>
        public static string Validate(GH_Structure<GH_Integer> tree, out int nDemand, out int nSupply)
        {
            nDemand = tree.PathCount;
            nSupply = nDemand > 0 ? tree.get_Branch(0).Count : 0;
            if (nDemand == 0 || nSupply == 0)
                return "DistanceMatrix is empty";
            for (int i = 0; i < nDemand; i++)
            {
                int count = tree.get_Branch(i).Count;
                if (count != nSupply)
                    return string.Format("DistanceMatrix branch {0} has {1} items; branch 0 has {2}. Every demand branch must list one cost per supply item.",
                        tree.get_Path(i), count, nSupply);
                foreach (var item in tree.get_Branch(i))
                {
                    if (item == null)
                        return string.Format("DistanceMatrix branch {0} contains a null cost", tree.get_Path(i));
                }
            }
            return null;
        }
    }
}
