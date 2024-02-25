using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

namespace DigitalCircularityToolkit.Characterization
{
  public class FeatureVector : GH_Component, IGH_VariableParameterComponent
    {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public FeatureVector()
      : base("FeatureVector", "FeatureVec",
        "Create a feature vector for an object",
        "DigitalCircularityToolkit", "Characterization")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddNumberParameter("X1", "X1", "Object property", GH_ParamAccess.list);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
            pManager.AddNumberParameter("FeatureVector", "FV", "Feature vector", GH_ParamAccess.tree);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
            //Initialize
            List<double> x1 = new List<double>();

            //Populate
            if (!DA.GetDataList(0, x1)) return;

            //information
            int n = x1.Count;
            int n_params = Params.Input.Count;

            //Collector
            GH_Structure<GH_Number> feature_vectors = new GH_Structure<GH_Number>();
            List<List<double>> data_collector = new List<List<double>>();

            for (int i_input = 0; i_input < n_params; i_input++)
            {
                data_collector.Add(new List<double>());
                DA.GetDataList(i_input, data_collector[i_input]);
                if (data_collector[i_input].Count != n)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Data inputs must have the same length");
                }
            }

            //Populate tree
            for (int i = 0; i < n; i++)
            {
                GH_Path path = new GH_Path(i);

                for (int j = 0; j < n_params; j++)
                {
                    feature_vectors.Append(new GH_Number(data_collector[j][i]), path);
                }

            }

            //output
            DA.SetDataTree(0, feature_vectors);

    }

    #region VARIABLE COMPONENT INTERFACE IMPLEMENTATION
    public bool CanInsertParameter(GH_ParameterSide side, int index)
    {

        // Only insert parameters on input side. This can be changed if you like/need
        // side== GH_ParameterSide.Output
        if (side == GH_ParameterSide.Input)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        // Only allowed to remove parameters if there are more than 2
        // from the input side
        if (side == GH_ParameterSide.Input && Params.Input.Count > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {

            // Has to return a parameter object!
            Param_GenericObject param = new Param_GenericObject();

            int count = 0;
            for (int i = 0; i < Params.Input.Count; i++)
            {
                count += i;
            }

            param.Name = "X" + (count + 2).ToString();
            param.NickName = param.Name;
            param.Description = "Object property";
            param.Optional = true;
            param.Access = GH_ParamAccess.list;

            return param;
    }


    public bool DestroyParameter(GH_ParameterSide side, int index)
    {
        //This function will be called when a parameter is about to be removed. 
        //You do not need to do anything, but this would be a good time to remove 
        //any event handlers that might be attached to the parameter in question.


        return true;
    }

    public void VariableParameterMaintenance()
    {
        //This method will be called when a closely related set of variable parameter operations completes. 
        //This would be a good time to ensure all Nicknames and parameter properties are correct. This method will also be 
        //called upon IO operations such as Open, Paste, Undo and Redo.


        //throw new NotImplementedException();


    }


    #endregion

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
    {
      get
      {
                return Properties.Resources.FEATUREVEC;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("b006d02d-2e04-4875-a77d-a3d3b80e0700"); }
    }

    public override GH_Exposure Exposure => GH_Exposure.primary;


    }
}
