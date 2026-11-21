using System.Drawing;
using Grasshopper.Kernel;

namespace DigitalCircularityToolkit.Framework
{
    /// <summary>
    /// Base class for DCT components (see docs/specs/COMPONENT_SPEC.md).
    /// Supplies the "(DCT)" name suffix, the tab name, and the icon by convention:
    /// Icons/&lt;ClassName&gt;.svg|.pdf is rendered at build time to Resources/Icons/&lt;ClassName&gt;.png and embedded.
    /// Existing components predate this class and still use GH_Component + IconLoader properties.
    /// </summary>
    public abstract class DctComponent : GH_Component
    {
        public const string Tab = "DigitalCircularityToolkit";

        protected DctComponent(string name, string nickname, string description, string subCategory)
            : base(name + " (DCT)", nickname, description, Tab, subCategory)
        {
        }

        /// <summary>Icon resource name; defaults to the class name. Override only to share an icon.</summary>
        protected virtual string IconName => GetType().Name;

        protected override Bitmap Icon => IconLoader.Get(IconName);
    }
}
