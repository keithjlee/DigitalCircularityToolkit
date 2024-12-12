using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Matching
{
    public class AlignToObject_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Align_GH class.
        /// </summary>
        public AlignToObject_GH()
          : base("AlignToObject", "AlignObj",
              "Align an object to another",
              "DigitalCircularityToolkit", "Matching")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("SourceObject", "Source", "Source object to align", GH_ParamAccess.item);
            pManager.AddGenericParameter("TargetObject", "Target", "Object to align to", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Aligned object", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geo", "Geometry of aligned object", GH_ParamAccess.item);
            pManager.AddTransformParameter("Transform", "T", "Transformation used to align object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject source = new DesignObject();
            DesignObject target = new DesignObject();

            if (!DA.GetData(0, ref source)) return;
            if (!DA.GetData(1, ref target)) return;

            Transform transformer = Transform.PlaneToPlane(source.LocalPlane, target.LocalPlane);

            DesignObject newobject = source.TransformObject(transformer);

            DA.SetData(0, newobject);
            DA.SetData(1, newobject.Geometry);
            DA.SetData(2, transformer);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; //.ALIGNTOOBJECT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F4E526F8-F0F9-4EA0-988A-2044DE529A97"); }
        }
    }
}