using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Orientation
{
    public class PrincipalAxesMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PrincipalAxesMesh class.
        /// </summary>
        public PrincipalAxesMesh()
          : base("PrincipalAxesMesh", "PCAMesh",
              "Determine the principal axes for a Brep",
              "DigitalCircularityToolkit", "Orientation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Geometry", "Geo", "Mesh to analyze", GH_ParamAccess.item);
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
            pManager.AddPointParameter("Vertices", "V", "Mesh vertices used for analysis", GH_ParamAccess.list);
            pManager.AddMeshParameter("AlignedGeometry", "AlignedGeo", "Input geometry with PCA1 aligned with global X", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            Mesh mesh = new Mesh();
            bool align = true;

            // Populate
            if (!DA.GetData(0, ref mesh)) return;
            DA.GetData(1, ref align);

            Mesh transformed_mesh = mesh.DuplicateMesh();

            PCA.SolvePCA(mesh, align, out Vector3d[] pca_vectors, out Point3d[] points, transformed_mesh);

            // return
            DA.SetData(0, pca_vectors[0]);
            DA.SetData(1, pca_vectors[1]);
            DA.SetData(2, pca_vectors[2]);
            DA.SetDataList(3, points);
            DA.SetData(4, transformed_mesh);
        }

    

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.PCAMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E4220251-7A2F-4347-94B5-EAA9474A14CE"); }
        }
    }
}