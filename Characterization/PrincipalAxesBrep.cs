using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class PrincipalAxesBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PrincipalAxesBrep class.
        /// </summary>
        public PrincipalAxesBrep()
          : base("PrincipalAxesBrep", "PCABrep",
              "Determine the principal axes for a Brep",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "Geo", "Brep to analyze", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ApproxNumSamples", "n", "Number of samples take for PCA analysis", GH_ParamAccess.item, 100);
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
            pManager.AddPointParameter("AnalysisPoints", "Points", "Discretized points used for analysis", GH_ParamAccess.list);
            pManager.AddBrepParameter("AlignedGeometry", "AlignedGeo", "Input geometry with PCA1 aligned with global X", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            Brep brep = new Brep();
            int n = 100;
            bool align = true;

            // Populate
            if (!DA.GetData(0, ref brep)) return;
            DA.GetData(1, ref n);
            DA.GetData(2, ref align);

            // number of samples per face
            List<int> sample_densities = PCA.AssignSampleDensity(brep, n);

            // density of UV sampling per face
            List<int> n_uv = PCA.AssignUV(sample_densities);

            // get approx evenly distributed points on surfaces
            Point3d[] discretized_points = PCA.DiscretizeBrep(brep, n_uv);

            // get PCA vectors
            double[][] positions = PCA.PositionMatrix(discretized_points);

            // get PCA vecvtors
            Vector3d[] pca_vectors = PCA.PCAvectors(positions, align);

            // transform point set
            // get the centroid
            VolumeMassProperties props = VolumeMassProperties.Compute(brep);


            // transformation plane-to-plane
            Transform plane_transform = PCA.Aligner(pca_vectors, props.Centroid);

            // apply
            brep.Transform(plane_transform);

            // return
            DA.SetData(0, pca_vectors[0]);
            DA.SetData(1, pca_vectors[1]);
            DA.SetData(2, pca_vectors[2]);
            DA.SetDataList(3, discretized_points);
            DA.SetData(4, brep);
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
            get { return new Guid("646BBC6E-C38E-47BC-87D7-C3B8327CA283"); }
        }
    }
}