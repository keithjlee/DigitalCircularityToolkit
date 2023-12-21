using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class BoxObject : Object
    {
        public int Quantity;
        public double LengthBuffer;
        public double WidthBuffer;
        public double HeightBuffer;
        public Box EffectiveBox
        {
            get
            {
                return GetEffectiveBox();
            }
        }
        public double Volume
        {
            get { return LengthBuffer * Length * WidthBuffer * Width * HeightBuffer * Height; }
        }

        public Box GetEffectiveBox()
        {
            var scaler = Transform.Scale(LocalPlane, LengthBuffer, WidthBuffer, HeightBuffer);

            Box box = new Box(Localbox);
            box.Transform(scaler);
            return box;
        }
    }
}
