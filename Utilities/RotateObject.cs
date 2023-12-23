using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using DigitalCircularityToolkit.Objects;

namespace DigitalCircularityToolkit.Utilities
{
    public class RotateObject : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RotateObject class.
        /// </summary>
        public RotateObject()
          : base("RotateObject", "Rotate",
              "Rotate an object about one of its PCAs",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to rotate", GH_ParamAccess.item);
            pManager.AddIntegerParameter("RotationAxis", "Axis", "PCA axis of rotation", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("RotationDegrees", "Degree", "Amount to rotate (degrees)", GH_ParamAccess.item, 0);

            Param_Integer param = pManager[1] as Param_Integer;
            param.AddNamedValue("PCA1", 1);
            param.AddNamedValue("PCA2", 2);
            param.AddNamedValue("PCA3", 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RotatedObject", "ObjRotated", "New rotated object", GH_ParamAccess.item);
            pManager.AddGeometryParameter("RotatedGeometry", "Geo", "Rotated geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DigitalCircularityToolkit.Objects.DesignObject obj = new DigitalCircularityToolkit.Objects.DesignObject();
            int axis = 1;
            double deg = 0;

            if (!DA.GetData(0, ref obj)) return;
            DA.GetData(1, ref axis);
            DA.GetData(2, ref deg);

            DigitalCircularityToolkit.Objects.DesignObject rotated_object = obj.Rotate(axis, deg);

            DA.SetData(0, rotated_object);
            if (obj.Geometry is PointCloud)
            {
                DA.SetDataList(1, ((PointCloud)rotated_object.Geometry).GetPoints());
            }
            else
            {
                DA.SetDataList(1, new List<GeometryBase> { rotated_object.Geometry });
            }


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
            get { return new Guid("14BC0572-A73D-4F5F-BF8A-AEF8BE3BC520"); }
        }
    }
}