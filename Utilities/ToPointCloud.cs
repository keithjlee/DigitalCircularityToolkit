using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
  public class ToPointCloud : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public ToPointCloud()
      : base("ToPointCloud", "PointCloud",
        "Convert a collection of points into a Point Cloud",
        "DigitalCircularityToolkit", "Utilities")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddPointParameter("Points", "Points", "Collection of points", GH_ParamAccess.list);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
            pManager.AddGenericParameter("PointCloud", "PointCloud", "PointCloud object", GH_ParamAccess.item);
    }
        
    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
            List<Point3d> points = new List<Point3d>();

            if (!DA.GetDataList(0, points)) return;

            PointCloud pointcloud = new PointCloud(points);

            DA.SetData(0, pointcloud);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
                return Properties.Resources.TOPOINTLCOUD;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("77f2887b-6e5f-4da3-ae76-7877a3ac469f"); }
    }
  }
}
