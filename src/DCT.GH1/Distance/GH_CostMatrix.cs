using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Numerics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace DigitalCircularityToolkit.Distance
{
    /// <summary>
    /// Grasshopper wrapper for a <see cref="CostMatrix"/> (COST_MATRIX_SPEC §3). One matrix travels as one item instead
    /// of a tree of n×m boxed numbers. Immutable, so duplicating shares it. Not serialized (cannot be internalized).
    /// </summary>
    public class GH_CostMatrix : GH_Goo<CostMatrix>
    {
        public GH_CostMatrix() { }
        public GH_CostMatrix(CostMatrix matrix) : base(matrix) { }

        public override bool IsValid => Value != null;
        public override string IsValidWhyNot => Value == null ? "No cost matrix" : string.Empty;
        public override string TypeName => "CostMatrix";
        public override string TypeDescription => "Costs between demand (rows) and supply (columns); ∞ = forbidden pair";
        public override IGH_Goo Duplicate() => new GH_CostMatrix(Value);
        public override string ToString() => Value?.ToString() ?? "Null cost matrix";
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case CostMatrix m:
                    Value = m;
                    return true;
                case IGH_Goo goo when goo.ScriptVariable() is CostMatrix wrapped:
                    Value = wrapped;
                    return true;
                default:
                    return false;
            }
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value is Q m)
            {
                target = m;
                return true;
            }
            return false;
        }
    }

    /// <summary>The parameter for cost matrices. Hidden from the toolbar; used by the cost and assignment components.</summary>
    public class Param_CostMatrix : GH_Param<GH_CostMatrix>
    {
        public Param_CostMatrix()
          : base("CostMatrix (DCT)", "CM", "Cost matrix: rows = demand, columns = supply", "DigitalCircularityToolkit", "Distance", GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("86ABD24A-74FD-49FB-BA90-D35607CE5D60");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override System.Drawing.Bitmap Icon => IconLoader.Get("EuclideanCost");
    }

    /// <summary>Shared input handling for the cost components.</summary>
    internal static class CostInputs
    {
        /// <summary>Feature vectors from the Demand/Supply trees (one branch per object), or null with an error message.</summary>
        public static bool Read(IGH_DataAccess DA, int demandIndex, int supplyIndex, out double[][] demand, out double[][] supply, out string error)
        {
            demand = supply = null;
            error = null;
            if (!DA.GetDataTree(demandIndex, out GH_Structure<GH_Number> d)) return false;
            if (!DA.GetDataTree(supplyIndex, out GH_Structure<GH_Number> s)) return false;
            if (HasNull(d) || HasNull(s))
            {
                error = "Feature vectors contain null values";
                return false;
            }
            demand = CostKernels.ToRows(d);
            supply = CostKernels.ToRows(s);
            error = CostFunctions.CheckFeatures(demand, supply);
            return error == null;
        }

        private static bool HasNull(GH_Structure<GH_Number> tree)
        {
            foreach (List<GH_Number> branch in tree.Branches)
                foreach (GH_Number x in branch)
                    if (x == null) return true;
            return false;
        }

        /// <summary>A power input: must be finite and &gt; 0.</summary>
        public static string CheckPower(string name, double p) =>
            double.IsFinite(p) && p > 0 ? null : string.Format("{0} must be a number > 0 (got {1})", name, p);
    }
}
