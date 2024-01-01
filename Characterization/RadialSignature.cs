using DigitalCircularityToolkit.Objects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Characterization
{
    internal static class RadialSignature
    {


        public static double[] HullSignature2D(DesignObject obj, int n)
        {
            // extract planar hull
            PolylineCurve hull = obj.Hull2D.ToPolylineCurve();

            // orient the start of curve to intersect with the local plane positive X axis
            Line local_x = new Line(obj.LocalPlane.Origin, obj.LocalPlane.XAxis);

            // find intersection
            CurveIntersections intersections = Intersection.CurveLine(hull, local_x, 1e-6, 1e-6);
            Point3d start_point = intersections[0].PointA;

            // set curve start to start_point
            hull.SetStartPoint(start_point);

            // sample points
            _ = hull.DivideByCount(n, true, out Point3d[] sample_points);

            double[] radial_distances = new double[n];
            for (int i = 0; i < n; i++)
            {
                radial_distances[i] = sample_points[i].DistanceTo(obj.LocalPlane.Origin);
            }

            return radial_distances;
        }

    }
}
