using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.GeometryProcessing;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class VolumeHull : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VolumeHull class.
        /// </summary>
        public VolumeHull()
          : base("VolumeHull", "Hull3D",
              "Get the volumetric hull of an object",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to analyze", GH_ParamAccess.item);
            pManager.AddNumberParameter("PlanarTolerance", "Tol", "Tolerance for checking planarity of object", GH_ParamAccess.item, 1e-4);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Hull", "Hull", "Volumetric hull", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            double tol = 1e-4;
            DA.GetData(1, ref tol);

            Brep hull;

            bool planar = false;
            if (obj.Height <= tol) planar = true;
            
            if (planar)
            {
                Polyline planar_hull = Hulls.MakeHull2d(obj.SampledPoints, obj.LocalPlane);

                hull = Brep.CreatePlanarBreps(planar_hull.ToNurbsCurve(), tol)[0];
            }
            else
            {
                Mesh volume_hull = Hulls.MakeHull(obj.SampledPoints);

                hull = Brep.CreateFromMesh(volume_hull, true);
            }

            DA.SetData(0, hull);
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
            get { return new Guid("0A28B56D-49A3-44E5-B321-CD8972E7D6A6"); }
        }
    }
}