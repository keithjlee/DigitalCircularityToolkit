using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Distances;
using MIConvexHull;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Rhino.Collections;

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

        public static Polyline MakeHull2d(IEnumerable<Point3d> pts)
        {
            var reduced_points = new Grasshopper.Kernel.Geometry.Node2List(pts);
            Polyline hull = Grasshopper.Kernel.Geometry.ConvexHull.Solver.ComputeHull(reduced_points);

            return hull;
        }

        public static Polyline MakeHull2d(IEnumerable<Point3d> pts, Plane plane)
        {
            Point3dList ptlist = new Point3dList(pts);
            Transform world_coords = Transform.PlaneToPlane(plane, Plane.WorldXY);

            //project to global XY plane
            ptlist.Transform(world_coords);


            Grasshopper.Kernel.Geometry.Node2List planar_points = new Grasshopper.Kernel.Geometry.Node2List(ptlist);

            Polyline hull_global = Grasshopper.Kernel.Geometry.ConvexHull.Solver.ComputeHull(planar_points);

            //project 
            Transform local_coords = Transform.PlaneToPlane(Plane.WorldXY, plane);

            hull_global.Transform(local_coords);

            return hull_global;
        }


    }
}
