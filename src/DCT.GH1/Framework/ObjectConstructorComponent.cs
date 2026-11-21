using System;
using System.Threading.Tasks;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;

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
            SetOutputs(DA, result.Object);
        }
    }
}
