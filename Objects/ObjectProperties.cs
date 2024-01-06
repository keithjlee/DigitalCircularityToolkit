using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class ObjectProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ObjectInformation class.
        /// </summary>
        public ObjectProperties()
          : base("ObjectProperties", "ObjProps",
              "Properties of a design object",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to analyze", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centroid", "Centroid", "Centroid of object", GH_ParamAccess.item);
            pManager.AddVectorParameter("PCA1", "PCA1", "Principal component vector 1", GH_ParamAccess.item);
            pManager.AddVectorParameter("PCA2", "PCA2", "Principal component vector 2", GH_ParamAccess.item);
            pManager.AddVectorParameter("PCA3", "PCA3", "Principal component vector 3", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Length (PCA1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width (PCA2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height (PCA3)", GH_ParamAccess.item);
            pManager.AddBoxParameter("BoundingBox", "BB", "Bounding box", GH_ParamAccess.item);
            pManager.AddPlaneParameter("LocalPlane", "XY", "PCA XY plane", GH_ParamAccess.item);
            pManager.AddCurveParameter("PlanarHull", "Hull2D", "Convex hull on local XY plane", GH_ParamAccess.item);
            pManager.AddMeshParameter("VolumeHull", "Hull3D", "Volumetric convex hull", GH_ParamAccess.item);
            pManager.AddPointParameter("SampledPoints", "Pts", "Points used for PCA analysis", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Geometry", "Geo", "Geometry of object", GH_ParamAccess.list);

            pManager.HideParameter(0);
            pManager.HideParameter(7);
            pManager.HideParameter(8);
            pManager.HideParameter(9);
            pManager.HideParameter(10);
            pManager.HideParameter(11);
            pManager.HideParameter(12);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize
            DesignObject obj = new DesignObject();

            // Read
            if (!DA.GetData(0, ref obj)) return;

            DA.SetData(0, obj.Centroid);
            DA.SetData(1, obj.PCA1);
            DA.SetData(2, obj.PCA2);
            DA.SetData(3, obj.PCA3);
            DA.SetData(4, obj.Length);
            DA.SetData(5, obj.Width);
            DA.SetData(6, obj.Height);
            DA.SetData(7, obj.Localbox);
            DA.SetData(8, obj.LocalPlane);
            DA.SetData(9, obj.Hull2D);
            DA.SetData(10, obj.Hull3D);
            DA.SetDataList(11, obj.SampledPoints);
            DA.SetData(12, obj.Geometry);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ObjectProperties;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("34017B92-0083-4D54-97BB-33D7C3D692A8"); }
        }
    }
}