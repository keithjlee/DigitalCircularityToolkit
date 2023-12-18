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
    public class PrincipalAxes : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PrincipalAxes()
          : base("PrincipalAxes", "PCA",
            "Determine the principal axes of a discretized curve",
            "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Closed polyline curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("No. samples", "n", "Number of curve samples for PCA analysis", GH_ParamAccess.item, 25);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("PCA1", "PCA1", "Principal Component 1", GH_ParamAccess.item) ;
            pManager.AddVectorParameter("PCA2", "PCA2", "Principal Component 2", GH_ParamAccess.item) ;
            pManager.AddVectorParameter("PCA3", "PCA3", "Principal Component 3", GH_ParamAccess.item) ;
            pManager.AddPointParameter("Points", "Points", "Discretized points", GH_ParamAccess.list) ;
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

            // populate
            if (!DA.GetData(0, ref curve)) return;
            DA.GetData(1, ref n);

            if (n < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "n must be greater than 1");
            }

            if (n < 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "n < 10 may result in poor results");
            }

            // sample points
            Point3d[] discretized_points = new Point3d[n];

            // populated discretized_points
            double[] discretized_params = curve.DivideByCount(n, true, out discretized_points);

            // data of x, y, z points
            double[][] positions = new double[n][];

            for (int i = 0; i < discretized_points.Length; i++)
            {
                // current point
                var point = discretized_points[i];

                // populate data array
                //positions[i] = new double[] { point.X, point.Y, point.Z };
                positions[i] = new double[]{point.X, point.Y, point.Z};
            }

            // create a PCA class
            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center
            };

            // analyze data
            MultivariateLinearRegression transform = pca.Learn(positions);

            // get PCAs
            Vector3d[] pca_vectors = new Vector3d[]
            {
                new Vector3d(),
                new Vector3d(),
                new Vector3d()
            };

            // populate vectors
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    pca_vectors[j][i] = transform.Weights[i][j];
                }
            }

            // return
            DA.SetData(0, pca_vectors[0]);
            DA.SetData(1, pca_vectors[1]);
            DA.SetData(2, pca_vectors[2]);
            DA.SetDataList(3, discretized_points);
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