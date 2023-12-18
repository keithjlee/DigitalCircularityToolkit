using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace DigitalCircularityToolkit
{
    public class DigitalCircularityToolkitInfo : GH_AssemblyInfo
    {
        public override string Name => "DigitalCircularityToolkit";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("2f5dd184-98ac-4f0a-a6e9-a4d65abbd24c");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}