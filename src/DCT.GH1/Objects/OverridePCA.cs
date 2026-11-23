using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Orientation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class OverridePCA : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OverridePCA class.
        /// </summary>
        public OverridePCA()
          : base("OverridePCA (DCT)", "OverridePCA",
              "Override the calculated principal components with a plane whose X,Y components = PCA1, PCA2",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_DesignObject(), "Object", "Obj", "Object to modify", GH_ParamAccess.item);
            pManager.AddPlaneParameter("PrincipalPlane", "XYPCA", "A plane that whose X,Y components represent the overriding PCA1, PCA2", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_DesignObject(), "Object", "Obj", "Object with overriden principal component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = null;
            Plane plane = new Plane();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref plane)) return;

            // A new object with the plane's axes; the input (shared with the upstream output) is not touched (B4).
            obj = obj.WithFrame(plane);

            DA.SetData(0, obj);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.OverridePCAIcon; //.OVERRIDEPCA;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FC6588C8-8DDF-4B65-B001-C3DD861CEED6"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}