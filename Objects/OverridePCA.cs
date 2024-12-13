using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Orientation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Objects
{
    public class OverridePCA : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OverridePCA class.
        /// </summary>
        public OverridePCA()
          : base("OverridePCA", "OverridePCA",
              "Override the calculated principal components with a plane whose X,Y components = PCA1, PCA2",
              "DigitalCircularityToolkit", "Objects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object to modify", GH_ParamAccess.item);
            pManager.AddPlaneParameter("PrincipalPlane", "XYPCA", "A plane that whose X,Y components represent the overriding PCA1, PCA2", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Obj", "Object with overriden principal component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DesignObject obj = new DesignObject();
            Plane plane = new Plane();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref plane)) return;

            //if (obj is LinearObject) obj = new LinearObject(obj);
            //if (obj is PlanarObject) obj = new PlanarObject(obj);
            //if (obj is BoxObject) obj = new BoxObject(obj);
            //if (obj is SphericalObject) obj = new SphericalObject(obj);

            if (obj is LinearObject) obj = (LinearObject)obj;
            if (obj is PlanarObject) obj = (PlanarObject)obj;
            if (obj is BoxObject) obj = (BoxObject)obj;
            if (obj is SphericalObject) obj = (SphericalObject)obj;

            // assert that plane origin is centered
            plane.Origin = obj.Localbox.Center;

            obj.Repopulate(plane);

            DA.SetData(0, obj);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return IconLoader.OverridePCAIcon; //.OVERRIDEPCA;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FC6588C8-8DDF-4B65-B001-C3DD861CEED6"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}