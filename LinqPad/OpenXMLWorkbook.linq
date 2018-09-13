<Query Kind="Program">
  <Reference>&lt;ProgramFilesX86&gt;\Open XML SDK\V2.5\lib\DocumentFormat.OpenXml.dll</Reference>
  <GACReference>WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</GACReference>
  <Namespace>DocumentFormat.OpenXml.Packaging</Namespace>
  <Namespace>DocumentFormat.OpenXml.Spreadsheet</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

void Main()
{
	var xlsxFile = @"C:\Source\seymour-matt\TrackerCSharp\TrackerASTMTests\Resources\Output\test.xlsx";
	
	if (!File.Exists(xlsxFile))
    {
        throw new ApplicationException($"Excel workbook '{xlsxFile}' not found.");
    }
	
	var _document = new Workbook(xlsxFile);
	var refSheet = _document.GetSheet(WorkbookConstants.ReferenceSheetIdx);
	var cells = refSheet.Get(WorkbookConstants.TestPoints);
	cells.Dump();
}

internal class WorkbookConstants
{
	public static readonly int TestsSheetIdx = 1;
	public static readonly int StatsSheetIdx = 2;
	public static readonly int ReferenceSheetIdx = 3;

	public static readonly string ColActualPositionX = @"ActualBallTipCol_X";
	public static readonly string ColActualPositionY = @"ActualBallTipCol_Y";
	public static readonly string ColActualPositionZ = @"ActualBallTipCol_Z";
	public static readonly string ColActualTestPointX = @"ActualTestPointCol_X";
	public static readonly string ColActualTestPointY = @"ActualTestPointCol_Y";
	public static readonly string ColActualTestPointZ = @"ActualTestPointCol_Z";
	public static readonly string PointDistanceTestResultCol = @"PointDistanceTestResultCol";
	public static readonly string PointErrorCol = @"PointErrorCol";
	public static readonly string PointPairDistanceCol = @"PointPairDistanceCol";
	public static readonly string PointPairErrorCol = @"PointPairErrorCol";
	public static readonly string PointTestResultCol = @"PointTestResultCol";
	public static readonly string TestPointPairs = @"TestPointPairs";
	public static readonly string TestPoints = @"TestPoints";
	public static readonly string TestRows = @"TestRows";

	public static readonly string[] AllNames = { ColActualPositionX, ColActualPositionY, ColActualPositionZ, ColActualTestPointX, ColActualTestPointY, ColActualTestPointZ, PointDistanceTestResultCol, PointErrorCol, PointPairDistanceCol, PointPairErrorCol, PointTestResultCol, TestPointPairs, TestPoints, TestRows };
}

/// <summary>
/// Used for opening and adding data to an existing excel file.
/// </summary>
internal class Workbook : IDisposable
{
	private SpreadsheetDocument _document;

	/// <summary>
	/// Attempts to open an existing workbook.
	/// </summary>
	/// <param name="path">Path to excel workbook file.</param>
	public Workbook(string path)
	{
		// open the workbook
		_document = SpreadsheetDocument.Open(path, true);
		_document.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
	}

	/// <summary>
	/// Dispose any outstanding resources.
	/// </summary>
	public void Dispose()
	{
		if (_document != null)
		{
			_document.Close();
			_document.Dispose();
		}
	}

	/// <summary>
	/// Save the workbook
	/// </summary>
	public void Save()
	{
		if (_document != null)
		{
			_document.WorkbookPart.Workbook.Save();
		}
	}

	/// <summary>
	/// Get a specific sheet in the workbook.
	/// </summary>
	/// <param name="index">1 indexed sheet position.</param>
	/// <returns>Workbook Sheet.</returns>
	public SpreadSheet GetSheet(int index)
	{
		// Access the main Workbook part, which contains all references.
		var workbookPart = _document.WorkbookPart;
		var sheet = workbookPart.Workbook.Descendants<Sheet>().ElementAt(index - 1);

		// Get the first worksheet. 
		var worksheetPart = (WorksheetPart)(_document.WorkbookPart.GetPartById(sheet.Id));
		var worksheet = worksheetPart.Worksheet;
		var sheetData = worksheet.GetFirstChild<SheetData>();

		return new SpreadSheet(sheet, sheetData);
	}

	public Dictionary<string, string> GetDefinedNames()
	{
		var returnValue = new Dictionary<string, string>();

		DefinedNames definedNames = _document.WorkbookPart.Workbook.DefinedNames;
		if (definedNames != null)
		{
			foreach (DefinedName dn in definedNames)
			{
				returnValue.Add(dn.Name.Value, dn.Text);
			}
		}
		return returnValue;
	}
}


/// <summary>
/// Excel workbook spreadsheet.
/// </summary>
internal class SpreadSheet
{
	private Sheet _sheet;
	private SheetData _sheetData;

	/// <summary>
	/// Spreadsheet name.
	/// </summary>
	public string Name
	{
		set
		{
			_sheet.Name = value;
		}
		get
		{
			return _sheet.Name;
		}
	}

	/// <summary>
	/// Get or set a value at a specific position in the spreadsheet.
	/// </summary>
	/// <param name="row">1 indexed row.</param>
	/// <param name="col">1 indexed column.</param>
	/// <returns>Value at the given position</returns>
	public dynamic this[int row, int col]
	{
		get
		{
			return Get(row, col);
		}

		set
		{
			Set(row, col, value);
		}
	}

	/// <summary>
	/// Creates a new spreadsheet.
	/// </summary>
	/// <param name="sheet">OpenXML Sheet class.</param>
	/// <param name="sheetData">OpenXML SheetData class.</param>
	public SpreadSheet(Sheet sheet, SheetData sheetData)
	{
		_sheet = sheet;
		_sheetData = sheetData;
	}

	/// <summary>
	/// Set a value at a specific position in the spreadsheet.
	/// </summary>
	/// <param name="row">1 indexed row.</param>
	/// <param name="col">1 indexed column.</param>
	/// <param name="value">Value to be set.</param>
	public void Set(int row, int col, dynamic value)
	{
		var cell = GetCell(row, col);
		cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture));
		cell.DataType = GetCellType(value);
	}

	/// <summary>
	/// Get a value at a specific position in the spreadsheet.
	/// </summary>
	/// <param name="row">1 indexed row.</param>
	/// <param name="col">1 indexed column.</param>
	/// <returns>Value at the given position</returns>
	public dynamic Get(int row, int col)
	{
		dynamic ret;
		var cell = GetCell(row, col);
		string rawText = cell.CellValue?.Text ?? string.Empty;

		switch (cell.DataType?.Value)
		{
			case CellValues.Number:
				ret = double.Parse(rawText);
				break;
			case CellValues.Boolean:
				ret = bool.Parse(rawText);
				break;
			case CellValues.Date:
				ret = DateTime.Parse(rawText);
				break;
			default:
				ret = rawText;
				break;
		}

		return ret;
	}

	public IEnumerable<Cell> Get(string addressName)
	{
		return _sheetData
			.Descendants<Cell>()
			.Where(c => c.CellReference == addressName)
			.ToList();
	}

	/// <summary>
	/// Gets a cell at a specified position.
	/// </summary>
	/// <param name="rowIndex">1 indexed row.</param>
	/// <param name="colIndex">1 indexed column.</param>
	/// <returns>OpenXML Cell class.</returns>
	private Cell GetCell(int rowIndex, int colIndex)
	{
		Cell cell = null;
		string cellReference = GetCellReference(rowIndex, colIndex);
		var row = GetRow(rowIndex);
		var cellQuery = row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference);

		// If there is not a cell with the specified column name, insert one.  
		if (cellQuery.Count() > 0)
		{
			cell = cellQuery.First();
		}
		else
		{
			// Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
			Cell refCell = null;

			foreach (var rowCell in row.Elements<Cell>())
			{
				if (string.Compare(rowCell.CellReference.Value, cellReference, true) > 0)
				{
					refCell = rowCell;
					break;
				}
			}

			cell = new Cell() { CellReference = cellReference };
			row.InsertBefore(cell, refCell);
		}

		return cell;
	}

	/// <summary>
	/// Gets a row at the desired position.
	/// </summary>
	/// <param name="rowIndex">1 indexed row.</param>
	/// <returns>OpenXML Cell class.</returns>
	private Row GetRow(int rowIndex)
	{
		Row row = null;
		var rowQuery = _sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex);

		if (rowQuery.Count() > 0)
		{
			row = rowQuery.First();
		}
		else
		{
			row = new Row() { RowIndex = (uint)rowIndex };
			_sheetData.Append(row);
		}

		return row;
	}

	/// <summary>
	/// Produces cell referebce using on base 26 math; 1-26 => A-Z, 27-52 => AA-AZ
	/// </summary>
	/// <param name="row">Sheet row.</param>
	/// <param name="col">Sheet column.</param>
	/// <returns>CellReference name.</returns>
	private string GetCellReference(int row, int col)
	{
		var dividend = col;
		string columnName = String.Empty;
		int modulo;

		while (dividend > 0)
		{
			modulo = (dividend - 1) % 26;
			columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
			dividend = ((dividend - modulo) / 26);
		}

		return columnName + row;
	}

	/// <summary>
	/// Get a OpenXML cell value type from the given dynamic object.
	/// </summary>
	/// <param name="value">Dynamic object.</param>
	/// <returns>OpenXML cell value type.</returns>
	private CellValues GetCellType(dynamic value)
	{
		CellValues ret = CellValues.String;
		TypeCode code = Type.GetTypeCode(value.GetType());

		switch (code)
		{
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				ret = CellValues.Number;
				break;
			case TypeCode.Boolean:
				ret = CellValues.Boolean;
				break;
			case TypeCode.DateTime:
				ret = CellValues.Date;
				break;
			default:
				ret = CellValues.String;
				break;
		}

		return ret;
	}
}
