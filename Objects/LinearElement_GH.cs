using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class LinearElement_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LinearElement class.
        /// </summary>
        public LinearElement_GH()
          : base("LinearElement", "Stick",
              "A linear element",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Closed polyline curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of curve samples for PCA analysis", GH_ParamAccess.item, 15);
            pManager.AddVectorParameter("PCAOverride", "PCA1", "Override vector for primary axis.", GH_ParamAccess.item, new Vector3d(0, 0, 0));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "LinearObject class", GH_ParamAccess.item) ;
            pManager.AddBoxParameter("BoundingBox", "BB", "Bounding box of object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            Curve curve = null;
            int n = 0;
            Vector3d pca_override = new Vector3d(0, 0, 0);

            // populate
            if (!DA.GetData(0, ref curve)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref pca_override);

            if (n < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "n must be greater than 1");
            }

            if (n < 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "n < 10 may result in poor results");
            }

            // Instantiate
            Object obj = new Object();

            // Check of local X axis is overriden by user
            if (pca_override.Length > 0)
            {
                obj = new Object(curve, n, pca_override);
            }
            else
            {
                obj = new Object(curve, n);
            }

            // return
            DA.SetData(0, obj);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("750C489F-8D4E-41CE-AC2B-2C4AD80916BE"); }
        }
    }
}