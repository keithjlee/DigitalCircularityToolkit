using System;
using System.Collections.Generic;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace DigitalCircularityToolkit.Framework
{
    /// <summary>
    /// Lets a released component gain optional inputs at the END of its input list without breaking saved files.
    /// <para>
    /// GH_Component.Read expects one "param_input" chunk per current input. A file saved before an input was appended
    /// lacks that chunk, so Grasshopper logs "Input parameter chunk is missing. Archive is corrupt." and shows the
    /// "IO generated N messages" dialog on every open. This helper takes the appended inputs off before the base
    /// Read and puts them back afterwards, still at their defaults.
    /// </para>
    /// Usage: <c>public override bool Read(GH_IReader reader) => AppendedInputs.Read(this, reader, 2, base.Read);</c>
    /// where 2 is the input count of the last release without the appended inputs.
    /// </summary>
    internal static class AppendedInputs
    {
        public static bool Read(GH_Component component, GH_IReader reader, int legacyInputCount, Func<GH_IReader, bool> baseRead)
        {
            var appended = new List<IGH_Param>();
            if (!reader.ChunkExists("param_input", legacyInputCount))
            {
                while (component.Params.Input.Count > legacyInputCount)
                {
                    IGH_Param p = component.Params.Input[component.Params.Input.Count - 1];
                    component.Params.UnregisterInputParameter(p, false);
                    appended.Insert(0, p);
                }
            }

            bool ok = baseRead(reader);

            foreach (IGH_Param p in appended) component.Params.RegisterInputParam(p);
            if (appended.Count > 0) component.Params.OnParametersChanged();
            return ok;
        }
    }
}
