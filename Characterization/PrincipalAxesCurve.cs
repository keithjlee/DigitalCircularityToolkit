using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;

namespace DigitalCircularityToolkit.Characterization
{
    public class PrincipalAxesCurve : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PrincipalAxesCurve()
          : base("PrincipalAxesCurve", "PCACurve",
            "Determine the principal axes of a curve object",
            "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Closed polyline curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of curve samples for PCA analysis", GH_ParamAccess.item, 15);
            pManager.AddBooleanParameter("AlignY", "AlignY", "Orient PCA vectors such that local Y axis is aligned to global Y", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("PCA1", "PCA1", "Principal Component 1", GH_ParamAccess.item) ;
            pManager.AddVectorParameter("PCA2", "PCA2", "Principal Component 2", GH_ParamAccess.item) ;
            pManager.AddVectorParameter("PCA3", "PCA3", "Principal Component 3", GH_ParamAccess.item) ;
            pManager.AddPointParameter("AnalysisPoints", "Points", "Discretized points used for analysis", GH_ParamAccess.list);
            pManager.AddCurveParameter("AlignedCurve", "AlignedCrv", "Input curve with PCA1 aligned with global X", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            Curve curve = null;
            int n = 0;
            bool align = true;

            // populate
            if (!DA.GetData(0, ref curve)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref align);

            if (n < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "n must be greater than 1");
            }

            if (n < 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "n < 10 may result in poor results");
            }

            // sample points
            Point3d[] discretized_points = PCA.DiscretizeCurve(curve, n);

            // data of x, y, z points
            double[][] positions =PCA.PositionMatrix(discretized_points);

            // get PCA vecvtors
            Vector3d[] pca_vectors = PCA.PCAvectors(positions, align);

            // get the Transformation object to align curve to global XYZ
            Transform plane_transform = PCA.Aligner(pca_vectors, curve, discretized_points);

            // Transform input curve
            curve.Transform(plane_transform);

            // return
            DA.SetData(0, pca_vectors[0]);
            DA.SetData(1, pca_vectors[1]);
            DA.SetData(2, pca_vectors[2]);
            DA.SetDataList(3, discretized_points);
            DA.SetData(4, curve);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4a25f078-97cc-4f3c-a33a-107da8cdf100");
    }
}