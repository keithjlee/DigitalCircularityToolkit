using Grasshopper.Kernel;

namespace DigitalCircularityToolkit.Framework
{
    /// <summary>
    /// COMPONENT_SPEC §5: expensive outputs are computed only when something can see them, i.e. the output is
    /// wired or the component's preview is on. With the preview off and nothing connected, the output is empty
    /// (hovering it shows nothing); say so in the output's description.
    /// </summary>
    internal static class OutputDemand
    {
        public static bool IsNeeded(GH_Component component, int outputIndex)
        {
            return component.Params.Output[outputIndex].Recipients.Count > 0 || !component.Hidden;
        }
    }
}
