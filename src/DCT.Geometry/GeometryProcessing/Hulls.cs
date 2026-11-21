using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIConvexHull;
using Rhino.Geometry;
using Rhino.Collections;

namespace DigitalCircularityToolkit.GeometryProcessing
{
    public static class Hulls
    {
        public static Mesh MakeHull(IEnumerable<Point3d> pts)
        {
            List<double[]> point_data = pts.Select(pt => new double[] { pt.X, pt.Y, pt.Z }).ToList();

            var result = ConvexHull.Create(point_data);

            Mesh hullmesh = new Mesh();

            // MIConvexHull >= 1.1 reports degenerate (collinear / coplanar) input instead of
            // returning a zero-volume hull; return an empty mesh in that case.
            if (result.Outcome != ConvexHullCreationResultOutcome.Success || result.Result == null)
                return hullmesh;

            var faces = result.Result.Faces;
            int count = 0;

            foreach ( var face in faces)
            {
                foreach (var vertex in face.Vertices)
                {
                    var pos = vertex.Position;
                    Point3d point = new Point3d(pos[0], pos[1], pos[2]);
                    hullmesh.Vertices.Add(point);
                }

                hullmesh.Faces.AddFace(count, count + 1, count + 2);
                count += 3;
            }

            hullmesh.Weld(Math.PI);
            hullmesh.Normals.ComputeNormals();

            return hullmesh;
        }

        /// <summary>
        /// Closed counter-clockwise convex hull of the points' XY coordinates, at Z = 0 (same result as Grasshopper's
        /// ConvexHull.Solver, whose start vertex may differ; P2.5: no Grasshopper reference needed).
        /// </summary>
        public static Polyline MakeHull2d(IEnumerable<Point3d> pts)
        {
            var list = pts as IList<Point3d> ?? new List<Point3d>(pts);
            var x = new double[list.Count];
            var y = new double[list.Count];
            for (int i = 0; i < x.Length; i++) { x[i] = list[i].X; y[i] = list[i].Y; }

            int[] idx = Numerics.ConvexHull2D.Indices(x, y);
            var hull = new Polyline(idx.Length + 1);
            foreach (int i in idx) hull.Add(x[i], y[i], 0);
            if (idx.Length > 0) hull.Add(hull[0]);
            return hull;
        }

        /// <summary>Convex hull of the points projected onto <paramref name="plane"/>, as a closed polyline in that plane.</summary>
        public static Polyline MakeHull2d(IEnumerable<Point3d> pts, Plane plane)
        {
            Point3dList ptlist = new Point3dList(pts);
            ptlist.Transform(Transform.PlaneToPlane(plane, Plane.WorldXY));   // plane coordinates (Z dropped by the hull)

            Polyline hull = MakeHull2d(ptlist);
            hull.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
            return hull;
        }

    }
}
