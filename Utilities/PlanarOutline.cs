using System;
using System.Collections.Generic;
using System.Linq;
using Accord;
using DigitalCircularityToolkit.GeometryProcessing;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
    public class PlanarOutline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlanarOutline class.
        /// </summary>
        public PlanarOutline()
          : base("PlanarOutline", "PlanarOutline",
              "Get the planar shadow outline of an object",
              "DigitalCircularityToolkit", "Utilities")
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
            pManager.AddCurveParameter("ProjectedCurve", "Curve", "Projected outline of object geometry", GH_ParamAccess.item);
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

            Curve shadow = null;

            var curve = obj.Geometry as Curve;
            if (curve != null) shadow = GetShadow(curve, plane);

            var brep = obj.Geometry as Brep;
            if (brep != null) shadow = GetShadow(brep, plane);

            var mesh = obj.Geometry as Mesh;
            if (mesh != null) shadow = GetShadow(mesh, plane);

            var pc = obj.Geometry as PointCloud;
            if (pc != null) shadow = GetShadow(pc, plane);

            DA.SetData(0, shadow);


        }

        private Curve GetShadow(PointCloud geo, Plane plane)
        {
            Polyline hull = Hulls.MakeHull2d(geo.GetPoints(), plane);

            return hull.ToPolylineCurve();
        }

        private Curve GetShadow(Curve geo, Plane plane)
        {
            Transform projection = Transform.PlanarProjection(plane);
            var proj_geo = (Curve)geo.Duplicate();
            proj_geo.Transform(projection);

            return proj_geo;
        }

        private Curve GetShadow(Brep geo, Plane plane)
        {

            Mesh[] meshes = Mesh.CreateFromBrep(geo, new MeshingParameters());

            Mesh brep_mesh = new Mesh();
            foreach (var mesh in meshes)
            {
                brep_mesh.Append(mesh);
            }

            return GetShadow(brep_mesh, plane);
        }

        private Curve GetShadow(Mesh geo, Plane plane)
        {
            Polyline[] outlines = geo.GetOutlines(plane);

            return JoinOutlines(outlines);
        }

        private Curve JoinOutlines(Polyline[] outlines)
        {
            // convert to polylinecurve
            List<PolylineCurve> outline_curves = new List<PolylineCurve>();
            foreach (Polyline outline in outlines) outline_curves.Add(outline.ToPolylineCurve());

            Curve[] joined_curves = Curve.JoinCurves(outline_curves);

            return joined_curves[0];
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
            get { return new Guid("24F26087-6C18-4763-B8B8-F63B12377D9F"); }
        }
    }
}