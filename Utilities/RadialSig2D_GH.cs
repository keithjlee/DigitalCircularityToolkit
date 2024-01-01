using System;
using System.Collections.Generic;
using Accord.Math;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace DigitalCircularityToolkit.Utilities
{
    public class RadialSig2D_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Radial class.
        /// </summary>
        public RadialSig2D_GH()
          : base("RadialSig2D", "Radial2D",
              "Get the radial signature of the 2D convex hull",
              "DigitalCircularityToolkit", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignObject", "Obj", "Object to analyze", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of radial samples to take", GH_ParamAccess.item, 20);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Signature", "Sig", "Radial distance signature", GH_ParamAccess.list);
            pManager.AddPointParameter("SampledPoints", "Points", "Sampled points for analysis", GH_ParamAccess.list);
            pManager.AddCurveParameter("Hull", "Hull", "Hull", GH_ParamAccess.item);
            pManager.AddPointParameter("StartPoint", "StartPoint", "start point", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            int n = 20;
            DA.GetData(1, ref n);

            // extract planar hull
            PolylineCurve hull = obj.Hull2D.ToPolylineCurve();

            // orient the start of curve to intersect with the local plane positive X axis
            Line local_x = new Line(obj.LocalPlane.Origin, obj.LocalPlane.XAxis);

            // find intersection
            CurveIntersections intersections = Intersection.CurveLine(hull, local_x, 1e-6, 1e-6);
            Point3d start_point = intersections[0].PointA;

            // set curve start to start_point
            hull.SetStartPoint(start_point);

            // Iterate over hull
            List<Point3d> sample_points = new List<Point3d>();
            double[] radial_distances = new double[n];
            double increment = 1 / (double)n;

            for (int i = 0; i < n; i++)
            {
                var point = hull.PointAtNormalizedLength(increment * i);
                radial_distances[i] = point.DistanceTo(obj.LocalPlane.Origin);
                sample_points.Add(point);
            }

            DA.SetDataList(0, radial_distances);
            DA.SetDataList(1, sample_points);
            DA.SetData(2, hull);
            DA.SetData(3, start_point);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("80025530-E9D2-4333-9B30-3D183BCCCDA8"); }
        }
    }
}