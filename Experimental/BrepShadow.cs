using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using DigitalCircularityToolkit.Objects;

namespace DigitalCircularityToolkit.Experimental
{
    public class BrepShadow : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BrepShadow class.
        /// </summary>
        public BrepShadow()
          : base("BrepShadow", "BrepShadow",
              "Brep shadow debugging",
              "DigitalCircularityToolkit", "Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to analyze", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane to project to. Origin will be shifted to object centroid.", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Project", "Proj", "Projected outline of object geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            Plane plane = new Plane();
            if (!DA.GetData(1, ref plane))
            {
                plane = obj.LocalPlane;
            }

            plane.Origin = obj.Localbox.Center;

            Brep geo = obj.Geometry as Brep;

            Transform projection = Transform.PlanarProjection(plane);
            Brep proj_geo = geo.Duplicate() as Brep;
            proj_geo.Transform(projection);

            Mesh[] meshes = Mesh.CreateFromBrep(geo, new MeshingParameters());

            Mesh brep_mesh = new Mesh();
            foreach (var mesh in meshes)
            {
                brep_mesh.Append(mesh);
            }

            Polyline[] outlines = brep_mesh.GetOutlines(plane);
            List<PolylineCurve> curves = new List<PolylineCurve>();
            foreach (Polyline outline in outlines)
            {
                curves.Add(outline.ToPolylineCurve());
            }

            Curve shadow = Curve.JoinCurves(curves)[0];

            DA.SetData(0, shadow);
            //Mesh.QuadRemeshBrep()
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
            get { return new Guid("6F6A02AC-10B1-4743-9A36-39B6E1046139"); }
        }
    }
}