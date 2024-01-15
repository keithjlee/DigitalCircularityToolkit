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
            var googleSheetsConnect = new GoogleSheetsConnect(filePathClientSecret);
            string spreadsheetId = "1SKWICixI2Zce94PyAZpngRVtqgGM2VslZ27H35ihaSs"; // You'll get this from the component's input
            string range = null;
            range = ConstructRange(sheetName, startColumnLetter, startRow + 1, numDimensions);


            IList<IList<Object>> sheetData = googleSheetsConnect.ReadSheetData(spreadsheetId, range);

            // Initialize the tree for geometries
            GH_Structure<GH_Brep> geoTree = new GH_Structure<GH_Brep>();

            foreach (var row in sheetData)
            {
                if (row.Count >= numDimensions + 2)
                {
                    double dim1 = Convert.ToDouble(row[row.Count - numDimensions]);
                    double dim2 = Convert.ToDouble(row[row.Count - numDimensions + 1]);
                    double? dim3 = numDimensions == 3 ? (double?)Convert.ToDouble(row[row.Count - numDimensions + 2]) : null;
                    int quantity = Convert.ToInt32(row[1]);

                    List<GH_Brep> breps = ConstructBrep(dim1, dim2, dim3, quantity, numDimensions);

                    GH_Path path = new GH_Path(geoTree.PathCount);
                    foreach (GH_Brep brep in breps)
                    {
                        geoTree.Append(brep, path);
                    }
                }
            }

            DA.SetDataTree(0, geoTree);

            // Initialize the tree for dimensions
            GH_Structure<GH_Number> dimTree = new GH_Structure<GH_Number>();

            foreach (var row in sheetData)
            {
                if (row.Count >= numDimensions + 2)
                {
                    List<GH_Number> dimensions = ConstructDim(row, numDimensions);

                    GH_Path path = new GH_Path(dimTree.PathCount);
                    foreach (GH_Number dim in dimensions)
                    {
                        dimTree.Append(dim, path);
                    }
                }
            }

            DA.SetDataTree(1, dimTree);

            // Initialize the tree for IDs
            GH_Structure<GH_String> idTree = new GH_Structure<GH_String>();

            foreach (var row in sheetData)
            {
                GH_String id = ConstructID(row);
                GH_Path path = new GH_Path(idTree.PathCount);
                idTree.Append(id, path);
            }

            DA.SetDataTree(2, idTree);


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

        private List<GH_Brep> ConstructBrep(double dim1, double dim2, double? dim3, int quantity, int numDimensions)
        {
            List<GH_Brep> breps = new List<GH_Brep>();

            if (numDimensions == 3 && dim3.HasValue)
            {
                // Create a box for 3 dimensions
                Box box = new Box(Plane.WorldXY, new Interval(0, dim1), new Interval(0, dim2), new Interval(0, dim3.Value));
                Brep brep = box.ToBrep();
                for (int i = 0; i < quantity; i++)
                {
                    breps.Add(new GH_Brep(brep.DuplicateBrep()));
                }
            }
            else if (numDimensions == 2)
            {
                // Create a rectangle for 2 dimensions
                Rectangle3d rectangle = new Rectangle3d(Plane.WorldXY, dim1, dim2);
                Brep[] breps2D = Brep.CreatePlanarBreps(rectangle.ToNurbsCurve());
                if (breps2D != null && breps2D.Length > 0)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        breps.Add(new GH_Brep(breps2D[0].DuplicateBrep()));
                    }
                }
            }

            return breps;
        }


        private List<GH_Number> ConstructDim(IList<Object> row, int numDimensions)
        {
            List<GH_Number> dimensions = new List<GH_Number>();

            // Extract the last numDimensions values from the row
            for (int i = row.Count - numDimensions; i < row.Count; i++)
            {
                if (double.TryParse(row[i].ToString(), out double value))
                {
                    dimensions.Add(new GH_Number(value));
                }
                else
                {
                    // Handle the case where the value cannot be parsed as a double
                    dimensions.Add(new GH_Number(0));
                }
            }

            return dimensions;
        }


        private GH_String ConstructID(IList<Object> row)
        {
            if (row.Count > 0)
            {
                return new GH_String(row[0].ToString());
            }
            else
            {
                return new GH_String(string.Empty);
            }
        }



        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("BB2F194E-671B-4FDB-82E3-1355A563260D");
    }
}