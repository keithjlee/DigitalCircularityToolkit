﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Orientation
{
    public static class Discretizer
    {
        /// <summary>
        /// Discretize a curve into n points
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Point3d[] DiscretizeCurve(Curve curve, int n)
        {
            // sample points
            Point3d[] discretized_points = new Point3d[n];

            // populated discretized_points
            double[] discretized_params = curve.DivideByCount(n, true, out discretized_points);

            return discretized_points;
        }

        public static Point3d[] DiscretizeBrep(Brep brep, List<int> n_uvs)
        {
            // total number of sample points
            int n_total = 0;
            foreach (int n in n_uvs)
            {
                n_total += (int)Math.Pow(n, 2);
            }

            Point3d[] surface_points = new Point3d[n_total];

            // get the actual local width (u) and height (v) distances
            //double increment_u;
            //double increment_v;

            int counter = 0;
            for (int i = 0; i < brep.Faces.Count; i++)
            {
                BrepFace face = brep.Faces[i];
                int n_uv = n_uvs[i];

                // get the actual increment distances in u, v
                //face.GetSurfaceSize(out increment_u, out increment_v);

                face.SetDomain(0, new Interval(0.0, 1.0));
                face.SetDomain(1, new Interval(0.0, 1.0));

                double increment = 1.0 / (n_uv + 1);

                // sample
                for (int j = 1; j <= n_uv; j++)
                {
                    for (int k = 1; k <= n_uv; k++)
                    {
                        surface_points[counter] = face.PointAt(j * increment, k * increment);
                        counter += 1;
                    }
                }
            }

            return surface_points;
        }

        /// <summary>
        /// Convert an array of Point3ds to a jagged array positions[][] = [[X,Y,Z], [X,Y,Z],...]
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double[][] PositionMatrix(Point3d[] points)
        {
            // data of x, y, z points
            double[][] positions = new double[points.Length][];

            for (int i = 0; i < points.Length; i++)
            {
                // current point
                var point = points[i];

                // populate data array
                positions[i] = new double[] { point.X, point.Y, point.Z };
            }

            return positions;
        }

        public static double[][] PositionMatrix(List<Point3d> points)
        {
            // data of x, y, z points
            double[][] positions = new double[points.Count][];

            for (int i = 0; i < points.Count; i++)
            {
                // current point
                var point = points[i];

                // populate data array
                positions[i] = new double[] { point.X, point.Y, point.Z };
            }

            return positions;
        }
    }
}