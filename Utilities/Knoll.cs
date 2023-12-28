using System;
using System.Collections.Generic;
using DigitalCircularityToolkit.Objects;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Utilities
{
  public class Knoll : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public Knoll()
      : base("Knoll", "Knoll",
        "Knoll a collection of objects",
        "DigitalCircularityToolkit", "Utilities")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddGenericParameter("Objects", "Objs", "Collection of objects to knoll", GH_ParamAccess.list);
            pManager.AddPlaneParameter("OriginPlane", "Plane", "Plane that defines the starting origin of grid", GH_ParamAccess.item, new Plane(Point3d.Origin, Vector3d.YAxis, Vector3d.XAxis));
            pManager.AddIntegerParameter("NumRows", "nRows", "Number of rows in knoll", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("RowSpacing", "dRow", "Spacing of rows", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("ColSpacing", "dCol", "Spacing of columns", GH_ParamAccess.item, 1);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Objects", "Objs", "Knolled objects", GH_ParamAccess.list);
      pManager.AddGeometryParameter("Geo", "Geo", "Geometry of knolled objects", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      List<DesignObject> objs = new List<DesignObject>();
      Plane plane = new Plane(Point3d.Origin, Vector3d.YAxis, Vector3d.XAxis);
      int nCols = 1;
      double dCol = 1;
      double dRow = 1;

      if (!DA.GetDataList(0, objs)) return;
      DA.GetData(1, ref plane);
      DA.GetData(2, ref nCols);
      DA.GetData(3, ref dCol);
      DA.GetData(4, ref dRow);

      if (nCols > objs.Count){
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number of columns specified > number of objects; nCols reduce to total number of objects");
                nCols = objs.Count;
      }

      List<DesignObject> new_objs = KnollObjects(objs, plane, nCols, dCol, dRow);

      List<GeometryBase> geos = new List<GeometryBase>();

      foreach (DesignObject obj in new_objs){
        geos.Add(obj.Geometry);
      }

      DA.SetDataList(0, new_objs);
      DA.SetDataList(1, geos);
    }

    public List<DesignObject> KnollObjects(List<DesignObject> objs, Plane plane, int nCols, double dCol, double dRow){

      int count = -1;

      List<DesignObject> new_objects = new List<DesignObject>();

      double y_offset = 0;

      while (count < objs.Count){

        double x_offset = 0;
        double y_max = 0;

        for (int j = 0; j < nCols; j++)
        {
            count++;
            if (count >= objs.Count) { break; }
            Plane target_plane = plane.Clone();
            var obj = objs[count];

            var x = x_offset + obj.Length / 2 + dCol;
            var y = y_offset + obj.Width / 2 + dRow;

            target_plane.Origin = plane.Origin + plane.XAxis * x + plane.YAxis * y;

            Transform transformer = Transform.PlaneToPlane(obj.LocalPlane, target_plane);

            var new_obj = obj.TransformObject(transformer);

            new_objects.Add(new_obj);

            if (obj.Width > y_max) y_max = obj.Width;
            x_offset += obj.Length + dCol;
        }

        y_offset += y_max + dRow;

      }

      return new_objects;
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      { 
        // You can add image files to your project resources and access them like this:
        //return Resources.IconForThisComponent;
        return null;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("247f234b-8ae3-48ea-94fa-b5499bb2111e"); }
    }
  }
}
