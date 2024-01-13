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
    public class ReadSheet : GH_Component
    {

        public ReadSheet()
          : base("ReadSheet", "ReadS",
            "Read inventory data from Googlde sheet. The columns should be arranged in the following order: ID, Qty, Dim1, Dim2, Dim3...,",
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

            // 4 Set number of dimensions
            pManager.AddIntegerParameter("Number of dimensions", "D", "Number of " +
                "feature dimensions of your object type", GH_ParamAccess.item, 3);

            // 5 Refresh
            pManager.AddBooleanParameter("Refresh", "R", "Refresh sheet data", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            //0
            pManager.AddBrepParameter("geo", "geo", "geo", GH_ParamAccess.tree);
            //1
            pManager.AddNumberParameter("Dimensions", "dims", "dims", GH_ParamAccess.tree);
            //2
            pManager.AddTextParameter("Identifier", "id", "Identifier", GH_ParamAccess.tree);
            //3
            pManager.AddTextParameter("Test", "Test", "test", GH_ParamAccess.tree);


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
            string sheetName = null;
            DA.GetData(1, ref sheetName);

            // Start column
            string startColumnLetter = "";
            DA.GetData(2, ref startColumnLetter);
            int startColumn = ColumnLetterToNumber(startColumnLetter);

            // Start row index
            int startRow = 0;
            DA.GetData(3, ref startRow);
            startRow -= 1; // Because of 0-indexing

            // Number of dimensions
            int numDimensions = 3;
            DA.GetData(4, ref numDimensions);


            // Live link
            var googleSheetsReader = new GoogleSheetsReader(filePathClientSecret);
            string spreadsheetId = "1SKWICixI2Zce94PyAZpngRVtqgGM2VslZ27H35ihaSs"; // You'll get this from the component's input
            string range = null;
            range = ConstructRange(sheetName, startColumnLetter, startRow + 1, numDimensions);


            IList<IList<Object>> sheetData = googleSheetsReader.ReadSheetData(spreadsheetId, range);

            // Initialize the tree for geometries
            GH_Structure<GH_Brep> geoTree = new GH_Structure<GH_Brep>();

            foreach (var row in sheetData)
            {
                // Assuming the last three values in each row are width, height, and length
                // and the second value is quantity
                if (row.Count >= numDimensions + 2)
                {
                    // Parse dimensions and quantity
                    double width = Convert.ToDouble(row[row.Count - 3]);
                    double height = Convert.ToDouble(row[row.Count - 2]);
                    double length = Convert.ToDouble(row[row.Count - 1]);
                    int quantity = Convert.ToInt32(row[1]);
                    // Construct the brep
                    GH_Brep brep = ConstructBrep(width, height, length, quantity);

                    // Add the brep to the tree
                    geoTree.Append(brep, new GH_Path(geoTree.PathCount));
                }
            }

            // Set the geometry tree to the output parameter
            DA.SetDataTree(0, geoTree);

            // Process sheetData and convert it to a suitable Grasshopper format
            GH_Structure <GH_String> ghSheetData = new GH_Structure<GH_String>();
            for (int i = 0; i < sheetData.Count; i++)
            {
                var row = sheetData[i];
                List<GH_String> ghRow = new List<GH_String>();
                foreach (var cell in row)
                {
                    // Assuming all data in sheetData are stringss.
                    // Modify this part if you have different types of data
                    ghRow.Add(new GH_String(cell.ToString()));
                }
                ghSheetData.AppendRange(ghRow, new GH_Path(i));
            }

            // Process sheetData as needed for your component


            // Set the converted data to an output parameter, for example to parameter index 3
            DA.SetDataTree(3, ghSheetData);

        }

        // ============================================================
        // HELPER FUNCTIONS
        // ============================================================

        private int ColumnLetterToNumber(string columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter)) return 0;
            columnLetter = columnLetter.ToUpper(); // Convert to uppercase to standardize
            int columnNumber = 0;
            for (int i = 0; i < columnLetter.Length; i++)
            {
                columnNumber *= 26;
                columnNumber += (columnLetter[i] - 'A' + 1);
            }
            return columnNumber - 1; // Subtract 1 to make it zero-based
        }

        private string ColumnNumberToLetter(int columnNumber)
        {
            string columnLetter = String.Empty;
            int modulo;

            while (columnNumber > 0)
            {
                modulo = (columnNumber) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnLetter;
        }

        private string ConstructRange(string sheet, string startingColumn, int startingRow, int dimCount)
        {
            int startColumnNumber = ColumnLetterToNumber(startingColumn);
            int endColumnNumber = startColumnNumber + dimCount + 1;
            string endColumn = ColumnNumberToLetter(endColumnNumber);

            string range = sheet + "!" + startingColumn + startingRow + ":" + endColumn;

            return range;
        }

        private GH_Brep ConstructBrep(double width, double height, double length, int quantity)
        {
            // Create a list to hold the duplicated Breps
            List<GH_Brep> breps = new List<GH_Brep>();

            // Define the base point for the box (can be changed based on requirements)
            Point3d basePoint = new Point3d(0, 0, 0);

            // Create a box with the given dimensions
            Box box = new Box(Plane.WorldXY, new Interval(0, width), new Interval(0, height), new Interval(0, length));
            Brep brep = box.ToBrep();

            // Duplicate the brep according to the quantity
            for (int i = 0; i < quantity; i++)
            {
                breps.Add(new GH_Brep(brep));
            }

            // If there's only one brep, return it directly
            if (breps.Count == 1)
            {
                return breps[0];
            }

            // If multiple breps, return a new GH_Brep containing all of them
            return new GH_Brep(Brep.JoinBreps(breps.Select(b => b.Value).ToList(), 0.01)[0]);
        }


 /*       private GH_Number ConstructDim()
        {

        }

        private GH_Text ConstructID()
        {

        }*/


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("BB2F194E-671B-4FDB-82E3-1355A563260D");
    }
}