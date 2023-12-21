using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class PointBasedObject : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointBasedObject class.
        /// </summary>
        public PointBasedObject()
          : base("PointBasedObject", "ObjPts",
              "An object created from a collection of points",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Points that define an object", GH_ParamAccess.list);
            pManager.AddVectorParameter("PCAOverride", "ForcePCA1", "Override the calculated PCA1 vector", GH_ParamAccess.item, new Vector3d(0, 0, 0));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Design object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Initialized
            List<Point3d> points = new List<Point3d> ();
            Vector3d pca_user = new Vector3d(0, 0, 0);

            //Populate
            if (!DA.GetDataList(0, points)) return;
            DA.GetData(1, ref pca_user);

            //Make object
            Object obj = new Object(points, 1, pca_user);

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
            get { return new Guid("3CE29286-6289-405F-8389-3754ADCF7A0B"); }
        }
    }
}