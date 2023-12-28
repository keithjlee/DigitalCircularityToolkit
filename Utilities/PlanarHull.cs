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
          : base("PlanarHull", "Hull2D",
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
            pManager.AddIntegerParameter("Plane", "Plane", "PCA Plane to project to", GH_ParamAccess.item, 1);

            Param_Integer param = pManager[1] as Param_Integer;

            param.AddNamedValue("XY", 1);
            param.AddNamedValue("XZ", 2);
            param.AddNamedValue("YZ", 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Hull", "Hull", "Planar hull", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Projection plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            int iplane = 1;

            if (!DA.GetData(0, ref obj)) return;
            DA.GetData(1, ref iplane);

            Plane plane = GetReferencePlane(obj, iplane);

            Polyline hull = Hulls.MakeHull2d(obj.SampledPoints, plane);

            DA.SetData(0, Brep.CreatePlanarBreps(hull.ToNurbsCurve(), 1e-6)[0]);
            DA.SetData(1, plane);
        }

        private Plane GetReferencePlane(DesignObject obj, int iplane)
        {
            Plane plane;

            if (iplane == 1)
            {
                plane = new Plane(obj.Localbox.Center, obj.PCA1, obj.PCA2);
            }
            else if (iplane == 2)
            {
                plane = new Plane(obj.Localbox.Center, obj.PCA1, obj.PCA3);
            }
            else if (iplane == 3)
            {
                plane = new Plane(obj.Localbox.Center, obj.PCA2, obj.PCA3);
            }
            else
            {
                plane = new Plane(obj.Localbox.Center, obj.PCA1, obj.PCA2);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Plane enum not recognized, defaulted to PCA XY");
            }

            return plane;
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
            get { return new Guid("B2D34CC4-9BEC-405F-9E2A-F969681B0D84"); }
        }
    }
}