using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.GeometryProcessing;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class PlanarHull : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlanarShadow class.
        /// </summary>
        public PlanarHull()
          : base("PlanarHull (DCT)", "Hull2D",
              "Get the planar convex hull of an object",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to analyze", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "PCA Plane to project to", GH_ParamAccess.item);

            //make plane optional
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Hull", "Hull", "Planar hull", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //initialize
            DesignObject obj = new DesignObject();

            //get data
            if (!DA.GetData(0, ref obj)) return;
            
            //if plane is not provided, default to PCA XY
            Plane plane = new Plane(obj.Localbox.Center, obj.PCA1, obj.PCA2);
            DA.GetData(1, ref plane);

            //center to origin
            plane.Origin = obj.Localbox.Center;

            Polyline hull = Hulls.MakeHull2d(obj.SampledPoints, plane);

            DA.SetData(0, Brep.CreatePlanarBreps(hull.ToNurbsCurve(), 1e-6)[0]);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.Hull2dIcon; //.HULL;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B2D34CC4-9BEC-405F-9E2A-F969681B0D84"); }
        }
    }
}