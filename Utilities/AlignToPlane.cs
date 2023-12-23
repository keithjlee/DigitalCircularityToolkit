using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class AlignToPlane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AlignToPlane class.
        /// </summary>
        public AlignToPlane()
          : base("AlignToPlane", "AlignPlane",
              "Align an object to a plane",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Object", "Object to align", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane to align to", GH_ParamAccess.item, Plane.WorldXY);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Aligned object", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geo", "Geometry of aligned object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            Plane pl = Plane.WorldXY;

            if (!DA.GetData(0, ref obj)) return;
            DA.GetData(1, ref pl);

            Transform transformer = Transform.PlaneToPlane(obj.LocalPlane, pl);

            DesignObject newobject = obj.TransformObject(transformer);

            DA.SetData(0, newobject);
            DA.SetData(1, newobject.Geometry);
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
            get { return new Guid("FE70341C-0A3C-4224-B8E6-4C0994E8E4A2"); }
        }
    }
}