using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

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
            pManager.AddTextParameter("CSV Path", "P", "Path to the CSV file", GH_ParamAccess.item);

            // 1 Set starting column
            pManager.AddTextParameter("Starting column", "C", "Starting column of your sheet. " +
                "Should also contain your id's. Input should be " +
                "the column Letter! (A for first column etc.)", GH_ParamAccess.item, "A");

            // 2 Set starting row
            pManager.AddIntegerParameter("Starting row", "R", "The row number where your actual data starts. " +
                "1 For the first row etc.", GH_ParamAccess.item, 1);

            // 3 Set number of dimensions
            pManager.AddIntegerParameter("Number of dimensions", "D", "Number of " +
                "feature dimensions of your object type", GH_ParamAccess.item, 3);

            // 4 Refresh
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
            pManager.AddTextParameter("Identifier", "id", "Identifier", GH_ParamAccess.list);
            //3
            pManager.AddNumberParameter("Test", "Test", "test", GH_ParamAccess.tree);


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

            // CSV
            string filePath = null;
            if (!DA.GetData(0, ref filePath)) return; // If no data or wrong data type, exit
            if (!File.Exists(filePath)) return; // If file doesn't exist, exit

            // Start column
            string startColumnLetter = "";
            DA.GetData(1, ref startColumnLetter);
            int startColumn = ColumnLetterToNumber(startColumnLetter);

            // Start row index
            int startRow = 0;
            if (!DA.GetData(2, ref startRow)) return;
            startRow -= 1; // Because of 0-indexing

            // Number of dimensions
            int numDimensions = 3;
            if (!DA.GetData(3, ref numDimensions)) return;


            // Live link
            var googleSheetsReader = new GoogleSheetsReader("C:/Users/soere/OneDrive/Desktop/sheets/client_secret_2.json");
            string spreadsheetId = "1SKWICixI2Zce94PyAZpngRVtqgGM2VslZ27H35ihaSs"; // You'll get this from the component's input
            string range = "Sheet4!A3:E13"; // Example range, also from input
            range = ConstructRange("Sheet4", startColumnLetter, startRow + 1, numDimensions);


            IList<IList<Object>> sheetData = googleSheetsReader.ReadSheetData(spreadsheetId, range);

            // Process sheetData and convert it to a suitable Grasshopper format
            GH_Structure<GH_String> ghSheetData = new GH_Structure<GH_String>();
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

            // Set the converted data to an output parameter, for example to parameter index 3
            DA.SetDataTree(3, ghSheetData);

            // Process sheetData as needed for your component


            // Initialize dim list for each row
            List<List<double>> dims = new List<List<double>>();

            // List of lists for geometry
            List<List<GH_Brep>> boxes = new List<List<GH_Brep>>();

            // List for IDs
            List<GH_String> ids = new List<GH_String>();

            int rowCounter = 0;

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                for (int i = startRow; i < lines.Length; i++)
                {
                    string line = lines[i];
                    
                    // Current row list
                    List<double> currentRow = new List<double>();

                    // List for for breps
                    List<GH_Brep> boxSublist = new List<GH_Brep>();

                    // Split the line into parts (columns)
                    string[] parts = line.Split(',');

                    // Iterate over the columns, starting from the startColumn index
                    // and considering up to numDimensions columns
                    for (int j = startColumn + 2; j < Math.Min(parts.Length, startColumn + 2 + numDimensions); j++)
                    {
                        if (double.TryParse(parts[j], out double value))
                        {
                            currentRow.Add(value);
                        }
                    }

                    // Parse the quantity value from the second column (parts[1])
                    int quantity = 1; // Default to 1 
                    if (parts.Length > 1 && int.TryParse(parts[1], out int parsedQuantity))
                    {
                        quantity = parsedQuantity;
                    }

                    // Assuming each row has enough dimensions to create a box
                    if (currentRow.Count == numDimensions)
                    {
                        if (numDimensions == 3)
                        {
                            // Create a box
                            Point3d basePoint = new Point3d(0, 0, 0); // Example base point
                            Box box = new Box(Plane.WorldXY, new Interval(0, currentRow[0]), new Interval(0, currentRow[1]), new Interval(0, currentRow[2]));

                            // Convert to Brep

                            Brep brep = box.ToBrep();
                            // Add the box to the boxSublist list according to quantity
                            for (int q = 0; q < quantity; q++)
                            {
                                boxSublist.Add(new GH_Brep(brep));
                            }

                            boxes.Add(boxSublist);
                        }
                        else if (numDimensions == 2)
                        {
                            // Create a 2D rectangle
                            Rectangle3d rectangle = new Rectangle3d(Plane.WorldXY, currentRow[0], currentRow[1]);

                            // Convert to NurbsCurve
                            NurbsCurve curve = rectangle.ToNurbsCurve();

                            // Create a Brep from the curve
                            Brep[] breps = Brep.CreatePlanarBreps(curve);
                            if (breps != null && breps.Length > 0)
                            {
                                Brep brep = breps[0];

                                // Add the brep to the boxSublist list according to the quantity
                                for (int q = 0; q < quantity; q++)
                                {
                                    boxSublist.Add(new GH_Brep(brep));
                                }

                                boxes.Add(boxSublist);
                            }
                        }

                    }


                    // Add the current row's list to the dims list
                    dims.Add(currentRow);

                    // Add the id to idslist
                    ids.Add(new GH_String(parts[0]));

                    // Increment the row counter
                    rowCounter++;
                }
            }
            catch (Exception ex)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error reading the CSV file: " + ex.Message);
                return;
            }

            // Create a tree structure for geo
            GH_Structure<GH_Brep> boxTree = new GH_Structure<GH_Brep>();
            for (int i = 0; i < boxes.Count; i++)
            {
                boxTree.AppendRange(boxes[i], new GH_Path(i));
            }

            // Create a tree structure for dims
            GH_Structure<GH_Number> dimTree = new GH_Structure<GH_Number>();
            for (int i = 0; i < dims.Count; i++)
            {
                // Convert each value in the current row to GH_Number and add it to a new branch in the tree
                dimTree.AppendRange(dims[i].ConvertAll(x => new GH_Number(x)), new GH_Path(i));
            }

            // Create a tree structure for ids
            GH_Structure<GH_String> id_tree = new GH_Structure<GH_String>();
            for (int i = 0; i < ids.Count; i++)
            {
                // Convert int to GH_Integer and attach
                id_tree.Append(ids[i], new GH_Path(i));
            }

            // Set the trees to the output parameter
            DA.SetDataTree(0, boxTree);
            DA.SetDataTree(1, dimTree);
            DA.SetDataTree(2, id_tree);
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


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("BB2F194E-671B-4FDB-82E3-1355A563260D");
    }
}