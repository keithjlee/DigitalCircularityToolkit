using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircularityToolkit.Objects
{
    public static class ObjectAnalysis
    {
        /// <summary>
        /// Reorient the object with a local X axis pca
        /// </summary>
        /// <param name="PCA1_override"></param>
        /// <param name="obj"></param>
        public static void OverridePCA(Vector3d PCA1_override, DesignObject obj)
        {
            // local plane
            Plane plane = new Plane(Point3d.Origin, obj.PCA1, obj.PCA2);
            Plane vector_override = new Plane(Point3d.Origin, PCA1_override, obj.PCA2);

            // global plane
            Plane plane_override = new Plane(obj.LocalPlane.Origin, PCA1_override, obj.PCA2);

            // Map the PCA XY plane to the world XY plane
            Transform vector_transform = Transform.PlaneToPlane(plane, vector_override);
            Transform plane_transform = Transform.PlaneToPlane(obj.LocalPlane, plane_override);

            // update PCAs
            obj.PCA1.Transform(vector_transform);
            obj.PCA2.Transform(vector_transform);
            obj.PCA3.Transform(vector_transform);

            // update local plane
            obj.LocalPlane = plane_override;

            // update bounding box
            obj.Boundingbox = obj.Geometry.GetBoundingBox(obj.LocalPlane, out obj.Localbox);

            // update transformed geometry
            obj.TransformedGeometry.Transform(plane_transform);
        }

    }
}
