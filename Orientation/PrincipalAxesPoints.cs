using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Orientation
{
    public class PrincipalAxesPoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PrincipalAxesPoints class.
        /// </summary>
        public PrincipalAxesPoints()
          : base("PrincipalAxesPoints", "PCAPoints",
              "Determine the principal axes for a collection of points",
              "DigitalCircularityToolkit", "Orientation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Collection of points", GH_ParamAccess.list);
            pManager.AddBooleanParameter("AlignY", "AlignY", "Orient PCA vectors such that local Y axis is aligned to global Y", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("PCA1", "PCA1", "Principal Component 1", GH_ParamAccess.item);
            pManager.AddVectorParameter("PCA2", "PCA2", "Principal Component 2", GH_ParamAccess.item);
            pManager.AddVectorParameter("PCA3", "PCA3", "Principal Component 3", GH_ParamAccess.item);
            pManager.AddPointParameter("AlignedPoints", "AlignedPts", "Input points with PCA1 aligned with global X", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            List<Point3d> points = new List<Point3d> { };
            bool align = true;

            // Populate
            if (!DA.GetDataList(0, points)) return;
            DA.GetData(1, ref align);

            // Initialize
            Vector3d[] pca_vectors;
            Point3d[] new_points;

            // Solve
            PCA.SolvePCA(points, align, out pca_vectors, out new_points);

            // return
            DA.SetData(0, pca_vectors[0]);
            DA.SetData(1, pca_vectors[1]);
            DA.SetData(2, pca_vectors[2]);
            DA.SetDataList(3, new_points);

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
            get { return new Guid("D87FAED7-8805-4572-A7A5-124A43459570"); }
        }
    }
}