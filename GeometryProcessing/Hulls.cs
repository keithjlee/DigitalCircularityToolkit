using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Distances;
using MIConvexHull;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.GeometryProcessing
{
    public static class Hulls
    {
        public static Mesh MakeHull(IEnumerable<Point3d> pts)
        {
            //List<double[]> point_data = new List<double[]>();

            //foreach(Point3d pt in pts)
            //{
            //    point_data.Add(new double[] {pt.X, pt.Y, pt.Z});
            //}

            List<double[]> point_data = pts.Select(pt => new double[] { pt.X, pt.Y, pt.Z }).ToList();

            var hull = MIConvexHull.ConvexHull.Create(point_data);

            Mesh hullmesh = new Mesh();

            var faces = hull.Faces;
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
    }
}
