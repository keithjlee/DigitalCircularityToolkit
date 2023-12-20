using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalCircularityToolkit.Orientation;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    internal class Object
    {
        public Point3d Centroid;
        public int NSamples;
        public Point3d[] SampledPoints;
        public Vector3d PCA1;
        public Vector3d PCA2;
        public Vector3d PCA3;
        public Plane LocalPlane;
        public BoundingBox Boundingbox;
        public double Length
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(false, true, true));
            }
        }
        public double Width
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, false, true));
            }
        }
        public double Height
        {
            get
            {
                return Boundingbox.Corner(true, true, true).DistanceTo(Boundingbox.Corner(true, true, false));
            }
        }
        public Box Localbox;
        public GeometryBase Geometry;
        public GeometryBase TransformedGeometry;

        public Object() { }

        /// <summary>
        /// A linear object
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="n"></param>
        public Object(Curve curve, int n)
        {
            Populate(curve, n);
        }

        private void Populate(Curve curve, int n)
        {
            // Initialize
            Vector3d[] pca_vectors;
            Point3d[] discretized_points;
            Curve transformed_curve;

            // Solve
            PCA.SolvePCA(curve, true, n, out pca_vectors, out discretized_points, out transformed_curve);

            // Populate
            NSamples = n;
            SampledPoints = discretized_points;
            PCA1 = pca_vectors[0];
            PCA2 = pca_vectors[1];
            PCA3 = pca_vectors[2];
            Geometry = curve;
            TransformedGeometry = transformed_curve;

            // Get Centroid
            Centroid = PCA.GetCentroid(curve, discretized_points);

            // Get Local plane
            LocalPlane = new Plane(Centroid, PCA1, PCA2);

            // Get Bounding Box
            Boundingbox = Geometry.GetBoundingBox(LocalPlane, out Localbox);

        }
    }


}
