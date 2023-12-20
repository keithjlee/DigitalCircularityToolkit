using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Orientation
{
    internal class InventoryItem
    {
        public Vector3d PCA1;
        public Vector3d PCA2;
        public Vector3d PCA3;
        public BoundingBox Boundingbox;
        public GeometryBase Geometry;

        public InventoryItem() { }


    }
}
