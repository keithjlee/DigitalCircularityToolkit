﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class RadialSignatureComplex_GH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RadialSignatureComplex_GH class.
        /// </summary>
        public RadialSignatureComplex_GH()
          : base("RadialSignatureComplex", "RadialSigComplex",
              "Get the radial signature of a curve in local plane coordinates where [u,v] -> [x, iy]",
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
            pManager.AddComplexNumberParameter("Signature", "Sig", "Curve signature", GH_ParamAccess.list);
            pManager.AddNumberParameter("RayDistances", "RayDist", "Distances from sample points to curve centroid", GH_ParamAccess.list);
            pManager.AddCurveParameter("ProjectedCurve", "ProjCurve", "Planar projection of curve", GH_ParamAccess.item);
            pManager.AddLineParameter("Rays", "Rays", "Rays used in analysis", GH_ParamAccess.list);
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

            Plane plane = new Plane();

            // divide curve and extract points
            curve.DivideByCount(n, true, out Point3d[] points);

            if (!DA.GetData(2, ref plane))
            {
                // auto fit plane
                Plane.FitPlaneToPoints(points, out plane);
            }

            var centroid = LengthMassProperties.Compute(curve).Centroid;
            plane.Origin = centroid;

            System.Numerics.Complex[] signature = HarmonicAnalysis.HullSignature2DComplex(points, centroid, plane, out Line[] rays, out double[] lengths);

            // signature
            DA.SetDataList(0, signature);
            DA.SetDataList(1, lengths);

            // projected curve
            Transform projection = Transform.PlanarProjection(plane);
            Curve projcurve = (Curve)curve.Duplicate();
            projcurve.Transform(projection);
            DA.SetData(2, projcurve);

            DA.SetDataList(3, rays);
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
            get { return new Guid("2AE7FB99-C011-461F-870C-2C7A86055F2A"); }
        }
    }
}