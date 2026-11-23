using System;
using System.Threading.Tasks;
using DigitalCircularityToolkit.Objects;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Framework
{
    /// <summary>Result of building one design object on a worker thread.</summary>
    public sealed class ConstructionResult
    {
        public DesignObject Object;
        public string Error;
    }

    /// <summary>
    /// Base for the object constructor components (COMPONENT_SPEC §1, §5): the per-item analysis runs as parallel
    /// tasks when Grasshopper's "Parallel Computing" is on for the component, and inline otherwise.
    /// Inputs are read and validated on the main thread (<see cref="Prepare"/>), the object is built in a task
    /// (the returned builder must be a pure function of the inputs), and messages and outputs are set back on the
    /// main thread. Exactly one task is added per iteration in the pre-solve pass, so results stay paired with
    /// their items.
    /// </summary>
    public abstract class ObjectConstructorComponent : GH_TaskCapableComponent<ConstructionResult>
    {
        protected ObjectConstructorComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory) { }

        /// <summary>
        /// Read and validate inputs. Return a builder for the object; or return null with <paramref name="error"/>
        /// set (shown as an Error), or null with no error for missing input (no output, no message).
        /// </summary>
        protected abstract Func<DesignObject> Prepare(IGH_DataAccess DA, out string error);

        /// <summary>Set the outputs for a successfully built object.</summary>
        protected abstract void SetOutputs(IGH_DataAccess DA, DesignObject obj);

        // ---- Orientation inputs (v0.7, ALIGNMENT_SPEC §5): appended after the released inputs.

        /// <summary>Number of inputs in the last release without the orientation inputs (they follow at this index).</summary>
        protected abstract int LegacyInputCount { get; }

        /// <summary>Append the Orientation and Plane inputs; call at the end of RegisterInputParams.</summary>
        protected static void RegisterOrientationInputs(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Orientation", "Orient",
                "How the axes are found. 0 Auto: principal axes of the shape (by area or length); square, round or symmetric " +
                "shapes are aligned with a side by the minimum-area rectangle, box Breps with their edges. 1 PCA: principal " +
                "axes only. 2 MinBox: minimum-area rectangle around the third axis. 3 Legacy: as v0.6 and earlier (sample " +
                "points, unweighted); files saved before v0.7 use Legacy. Ignored when Plane is given", GH_ParamAccess.item, 0);
            var mode = (Param_Integer)pManager[i];
            mode.AddNamedValue("Auto", 0);
            mode.AddNamedValue("PCA", 1);
            mode.AddNamedValue("MinBox", 2);
            mode.AddNamedValue("Legacy", 3);
            pManager[i].Optional = true;
            int j = pManager.AddPlaneParameter("Plane", "Pl",
                "Optional: use this plane's X and Y axes as the object's axes (overrides Orientation). Its origin is ignored",
                GH_ParamAccess.item);
            pManager[j].Optional = true;
        }

        /// <summary>The analysis options from the orientation inputs, or null with an error.</summary>
        protected AnalysisOptions ReadOrientation(IGH_DataAccess DA, out string error)
        {
            error = null;
            int mode = 0;
            DA.GetData(LegacyInputCount, ref mode);
            Plane plane = Plane.Unset;
            if (DA.GetData(LegacyInputCount + 1, ref plane) && plane.IsValid)
                return new AnalysisOptions(OrientationMode.Override, 0.05, plane, null);
            if (mode < 0 || mode > 3)
            {
                error = string.Format("Orientation must be 0 (Auto), 1 (PCA), 2 (MinBox) or 3 (Legacy); got {0}", mode);
                return null;
            }
            return new AnalysisOptions((OrientationMode)mode);
        }

        /// <summary>Files saved before v0.7 keep their frames: their Orientation input is set to Legacy.</summary>
        public override bool Read(GH_IReader reader) =>
            AppendedInputs.Read(this, reader, LegacyInputCount, base.Read, () =>
            {
                var mode = (Param_Integer)Params.Input[LegacyInputCount];
                mode.PersistentData.Clear();
                mode.PersistentData.Append(new GH_Integer((int)OrientationMode.Legacy));
            });

        private bool _remarkedSymmetric;

        protected override void BeforeSolveInstance()
        {
            _remarkedSymmetric = false;
            base.BeforeSolveInstance();
        }

        protected sealed override void SolveInstance(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                Func<DesignObject> build = Prepare(DA, out string error);
                TaskList.Add(build == null
                    ? Task.FromResult(new ConstructionResult { Error = error })
                    : Task.Run(() => new ConstructionResult { Object = build() }, CancelToken));
                return;
            }

            if (!GetSolveResults(DA, out ConstructionResult result))
            {
                Func<DesignObject> build = Prepare(DA, out string error);
                result = build == null ? new ConstructionResult { Error = error } : new ConstructionResult { Object = build() };
            }

            if (result == null) return;
            if (result.Error != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Error);
                return;
            }
            if (result.Object == null) return;
            if (result.Object.Frame.Resolved == OrientationResolution.Symmetric && !_remarkedSymmetric)
            {
                _remarkedSymmetric = true;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    "Some objects have no preferred direction (square, round or symmetric): their axes follow a fixed rule " +
                    "(the side closest to world X). Use the Plane input to set them.");
            }
            SetOutputs(DA, result.Object);
        }
    }
}
