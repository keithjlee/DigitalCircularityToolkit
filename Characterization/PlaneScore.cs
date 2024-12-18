﻿using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
    public class PlaneScore : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlanarScore class.
        /// </summary>
        public PlaneScore()
          : base("PlaneScore (DCT)", "PlaneScore",
              "Measure how well a plane abstracts an input object",
              "DigitalCircularityToolkit", "Characterization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to measure", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "F", "Scale factor for distance, must be >= 100", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("PlaneScore", "Score", "Plane score. Optimal value is 0.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            if (!DA.GetData(0, ref obj)) return;

            double factor = 100;
            DA.GetData(1, ref factor);

            double ratio1 = obj.Height / obj.Length;
            double ratio2 = obj.Height / obj.Width;

            if (ratio1 > ratio2)
            {
                DA.SetData(0, ratio1 * factor);
            }
            else
            {
                DA.SetData(0, ratio2 * factor);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.PlaneScoreIcon; //.PLANESCORE;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("24877A3A-6077-4ED3-9DA4-9D0A2070D768"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}