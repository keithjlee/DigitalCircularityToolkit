using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class RadialSignature_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RadialSignatureComplex_GH class.
        /// </summary>
        public RadialSignature_GH()
          : base("RadialSignature (DCT)", "RadialSig",
              "Perform a radial signature analysis on a curve.",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Curve to analyze", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumSamples", "n", "Number of radial samples to take", GH_ParamAccess.item, 20);
            pManager.AddPlaneParameter("Plane", "Plane", "Analysis plane. If not supplied, auto-fit will occur", GH_ParamAccess.item);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ComplexSignature", "ComplexSig", "Curve signature where planar coordinates [u, v] is converted to complex number u + iv", GH_ParamAccess.list);
            pManager.AddNumberParameter("DistanceSignature", "DistSig", "Distance from the curve centroid to each sample point, measured in the plane (= |ComplexSignature|)", GH_ParamAccess.list);
            pManager.AddCurveParameter("ProjectedCurve", "Curve", "Planar projection of curve", GH_ParamAccess.item);
            pManager.AddLineParameter("Rays", "Rays", "Rays from the centroid to each projected sample point", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            if (!DA.GetData(0, ref curve)) return;

            int n = 20;
            DA.GetData(1, ref n);
            if (n < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "NumSamples must be at least 2");
                return;
            }

            // Samples equally spaced by arc length: n points on a closed curve, n + 1 (both ends) on an open one.
            curve.DivideByCount(n, true, out Point3d[] points);
            if (points == null || points.Length < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not divide the curve");
                return;
            }
            if (!curve.IsClosed)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Curve is open: the signature is treated as periodic, so the jump from end back to start adds high harmonics");

            Plane plane = Plane.Unset;
            if (!DA.GetData(2, ref plane))
            {
                // auto fit plane
                if (Plane.FitPlaneToPoints(points, out plane) != PlaneFitResult.Success)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not fit a plane to the curve (is it a straight line?). Supply a Plane.");
                    return;
                }
            }

            // Centre: the area centroid for a closed planar outline (as for design objects, PCA.GetCentroid), otherwise
            // the length centroid of the curve. It used to be the length centroid always.
            Point3d? centre = null;
            if (curve.IsClosed && curve.IsPlanar()) centre = AreaMassProperties.Compute(curve)?.Centroid;
            if (centre == null) centre = LengthMassProperties.Compute(curve)?.Centroid;
            if (centre == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not compute the curve centroid");
                return;
            }
            plane.Origin = centre.Value;

            System.Numerics.Complex[] signature = HarmonicAnalysis.RadialSignature2DComplex(points, plane, out Line[] rays, out double[] lengths);

            // signature
            DA.SetDataList(0, signature);
            DA.SetDataList(1, lengths);

            // projected curve
            Transform projection = Transform.PlanarProjection(plane);
            Curve projcurve = curve.DuplicateCurve();
            projcurve.Transform(projection);
            DA.SetData(2, projcurve);

            // analysis rays
            DA.SetDataList(3, rays);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.RadialSignatureIcon; //.RADIALSIG;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2AE7FB99-C011-461F-870C-2C7A86055F2A"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
    }
}