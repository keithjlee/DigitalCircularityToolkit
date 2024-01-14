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

            // 4 Identity (ID) parameter
            pManager.AddTextParameter("Identity (ID)", "ID", "Identity for the object (eg. balsa_sticks", GH_ParamAccess.item);

            // 5 Quantity (Qty) parameter
            pManager.AddNumberParameter("Quantity (Qty)", "Qty", "List of quantities for each row", GH_ParamAccess.list);

            // 6 Dim1
            pManager.AddNumberParameter("Dimension 1", "D1", "List of first dimensions eg. Lenght", GH_ParamAccess.list);

            // 7 Dim2
            pManager.AddNumberParameter("Dimension 2", "D2", "List of second dimensions eg. Width", GH_ParamAccess.list);

            // 8 Dim3
            pManager.AddNumberParameter("Dimension 3", "D3", "List of third dimensions eg. height", GH_ParamAccess.list);
            pManager[6].Optional = true;

            // 9 Add a button parameter
            pManager.AddBooleanParameter("Write to Sheet", "W", "Press button to write data to the sheet", GH_ParamAccess.item, false);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) { }

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

            // Identity (ID) - new input at index 4
            string idText = null;
            DA.GetData(4, ref idText);

            // Quantity (Qty) - new input at index 5
            List<double> qtyList = new List<double>();
            DA.GetDataList(5, qtyList);

            // Lists for dimensions
            List<double> dim1 = new List<double>();
            List<double> dim2 = new List<double>();
            List<double> dim3 = new List<double>();

            // Get the data as List<double> for all dimensions
            DA.GetDataList(6, dim1);
            DA.GetDataList(7, dim2);
            DA.GetDataList(8, dim3);

            // Check if all lists are of the same length
            if (dim1.Count != dim2.Count || dim2.Count != dim3.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Dimension lists are not of the same length.");
                return;
            }

            //============================================================
            //BODY
            //============================================================

            // Button state
            bool writeToSheet = false;
            DA.GetData(9, ref writeToSheet);

            // Only proceed if the button is pressed
            if (!writeToSheet) return;

            // Live link
            var googleSheetsConnect = new GoogleSheetsConnect(filePathClientSecret);
            string spreadsheetId = "1SKWICixI2Zce94PyAZpngRVtqgGM2VslZ27H35ihaSs"; // You'll get this from the component's input

            // Construct data for Google Sheets
            IList<IList<Object>> values = new List<IList<Object>>();
            for (int i = 0; i < dim1.Count; i++)
            {
                string idWithIndex = idText + "_" + (i + 1).ToString(); // Concatenating ID with index
                values.Add(new List<Object> { idWithIndex, qtyList[i], dim1[i], dim2[i], dim3[i] });
            }

            // Define range
            string endColumnLetter = ((char)(startColumnLetter[0] + 4)).ToString(); // Adjusted for 5 columns (ID, Qty, Dim1, Dim2, Dim3)
            string range = $"{sheetName}!{startColumnLetter}{startRow}:{endColumnLetter}{startRow + dim1.Count - 1}";

            // Write to Google Sheets
            googleSheetsConnect.WriteSheetData(spreadsheetId, range, values);

        }

        // ============================================================
        // HELPER FUNCTIONS
        // ============================================================


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("B2436033-3F76-4006-B89A-7062B3450055");
    }
}