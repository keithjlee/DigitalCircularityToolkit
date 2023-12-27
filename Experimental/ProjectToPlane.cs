using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Experimental
{
    public class ProjectToPlane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ProjectToPlane class.
        /// </summary>
        public ProjectToPlane()
          : base("ProjectToPlane", "ProjToPlane",
              "Project the image of an object to a plane",
              "DigitalCircularityToolkit", "Experimental")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to project", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane to project to. Plane origin is at centroid of object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geo", "Projected geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            Plane plane = new Plane();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref plane)) return;

            // assert that plane is centered on object
            plane.Origin = obj.Localbox.Center;

            Transform projection = Transform.PlanarProjection(plane);

            GeometryBase planar_geo = obj.Geometry.Duplicate();
            planar_geo.Transform(projection);

            DA.SetData(0, planar_geo);
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
            get { return new Guid("DCF8EC97-FC96-4C4D-A14E-D064B2D1FD3F"); }
        }
    }
}