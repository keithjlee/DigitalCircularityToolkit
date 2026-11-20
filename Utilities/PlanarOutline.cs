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
          : base("PlanarOutline (DCT)", "PlanarOutline",
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

            if (obj.Geometry is Curve curve) shadow = GetShadow(curve, plane);

            if (obj.Geometry is Brep brep) shadow = GetShadow(brep, plane);

            if (obj.Geometry is Mesh mesh) shadow = GetShadow(mesh, plane);

            if (obj.Geometry is PointCloud pc) shadow = GetShadow(pc, plane);

            if (shadow == null && obj.Geometry != null && !(obj.Geometry is Curve) && !(obj.Geometry is PointCloud))
            {
                // Outline failed (B13): fall back to the convex hull of the vertices, and say so.
                Point3d[] pts = OutlineFallbackPoints(obj.Geometry);
                if (pts.Length >= 3)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not compute the exact outline; returning the planar convex hull instead");
                    shadow = Hulls.MakeHull2d(pts, plane).ToPolylineCurve();
                }
            }

            if (shadow == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not compute an outline for geometry of type " +
                    (obj.Geometry == null ? "null" : obj.Geometry.GetType().Name));
                return;
            }

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

            return GetShadow(brep_mesh, plane, geo.IsSolid);
        }

        private Curve GetShadow(Mesh geo, Plane plane, bool closed = false)
        {
            Polyline[] outlines = geo.GetOutlines(plane);

            if (outlines == null || outlines.Length == 0)
            {
                // GetOutlines fails when faces are edge-on to the plane or project onto each other (e.g. a box viewed
                // along its own axes, B13). For a closed mesh the front-facing faces alone cover the whole shadow, so
                // retry with those (edge-on and back faces dropped).
                Mesh facing = FrontFacing(geo, plane.Normal, closed || geo.IsClosed);
                if (facing != null) outlines = facing.GetOutlines(plane);
            }

            return JoinOutlines(outlines);
        }

        private static Mesh FrontFacing(Mesh geo, Vector3d normal, bool closed)
        {
            if (!closed) return null;
            Mesh m = geo.DuplicateMesh();
            m.FaceNormals.ComputeFaceNormals();
            var drop = new List<int>();
            for (int i = 0; i < m.Faces.Count; i++)
            {
                if (m.FaceNormals[i] * normal <= 1e-6) drop.Add(i);
            }
            if (drop.Count == 0 || drop.Count == m.Faces.Count) return null;
            m.Faces.DeleteFaces(drop);
            m.Compact();
            return m;
        }

        private static Point3d[] OutlineFallbackPoints(GeometryBase geo)
        {
            if (geo is Mesh mesh) return mesh.Vertices.ToPoint3dArray();
            if (geo is Brep brep)
            {
                var pts = new List<Point3d>();
                foreach (var v in brep.Vertices) pts.Add(v.Location);
                foreach (var m in Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh) ?? new Mesh[0])
                    pts.AddRange(m.Vertices.ToPoint3dArray());
                return pts.ToArray();
            }
            return new Point3d[0];
        }

        private Curve JoinOutlines(Polyline[] outlines)
        {
            if (outlines == null || outlines.Length == 0) return null;

            // convert to polylinecurve
            List<PolylineCurve> outline_curves = new List<PolylineCurve>();
            foreach (Polyline outline in outlines) outline_curves.Add(outline.ToPolylineCurve());

            Curve[] joined_curves = Curve.JoinCurves(outline_curves);

            return joined_curves.Length > 0 ? joined_curves[0] : null;
        }



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.PlanarOutlineIcon; //.OUTLINE;
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