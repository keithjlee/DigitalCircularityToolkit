using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Documentation;
using System.Linq;

namespace DigitalCircularityToolkit.Input
{
    public class WriteSheet : GH_Component
    {

        public WriteSheet()
          : base("WriteSheet", "WriteS",
            "Write inventory data to Googlde sheet. The columns will be arranged in the following order: ID, Qty, Dim1, Dim2, Dim3...,",
            "DigitalCircularityToolkit", "Input")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            // 0 Set path
            pManager.AddTextParameter("Client secret file path", "CS", "Path to the client " +
                "secret file (Supplied by instructors)", GH_ParamAccess.item);

            // 1 Sheet name
            pManager.AddTextParameter("Sheet name", "S", "Sheet name typed out " +
                "excactly as in the bottom of your sheet (eg. Sheet5)", GH_ParamAccess.item);

            // 2 Set starting column
            pManager.AddTextParameter("Starting column", "C", "Starting column of your sheet. " +
                "Should also contain your id's. Input should be " +
                "the column Letter! (A for first column etc.)", GH_ParamAccess.item, "A");

            // 3 Set starting row
            pManager.AddIntegerParameter("Starting row", "R", "The row number where your actual data starts. " +
                "1 For the first row etc.", GH_ParamAccess.item, 1);

            // 4 Dim1
            pManager.AddNumberParameter("Dimension 1", "D1", "List of first dimensions eg. Lenght", GH_ParamAccess.list);

            // 5 Dim2
            pManager.AddNumberParameter("Dimension 2", "D2", "List of second dimensions eg. Width", GH_ParamAccess.list);

            // 6 Dim3
            pManager.AddIntegerParameter("Dimension 3", "D3", "List of third dimensions eg. height", GH_ParamAccess.list);

            pManager[6].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
  
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ===========================================================
            // INPUTS
            // ===========================================================

            // Client Secret
            string filePathClientSecret = null;
            DA.GetData(0, ref filePathClientSecret);

            // Sheet name
            string sheetName = "Sheet1";
            DA.GetData(1, ref sheetName);

            // Start column
            string startColumnLetter = "A";
            DA.GetData(2, ref startColumnLetter);

            // Start row index
            int startRow = 0;
            DA.GetData(3, ref startRow);

            // dim1
            GH_List dim1 = null;
            DA.GetData(4, ref dim1);

            // dim2
            GH_List dim2 = null;
            DA.GetData(4, ref dim2);

            // dim3
            GH_List dim3 = null;
            DA.GetData(4, ref dim3);

            //===========================================================
            //BODY
            //===========================================================


            // Live link
            var googleSheetsConnect = new GoogleSheetsConnect(filePathClientSecret);
            string spreadsheetId = "1SKWICixI2Zce94PyAZpngRVtqgGM2VslZ27H35ihaSs"; // You'll get this from the component's input

        }

        // ============================================================
        // HELPER FUNCTIONS
        // ============================================================


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("B2436033-3F76-4006-B89A-7062B3450055");
    }
}