using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

#region Syncfusion
using Syncfusion.Blazor.Data;
using Syncfusion.ExcelExport;
using Syncfusion.Blazor.Internal;
using System.Text.RegularExpressions;
#endregion

namespace Syncfusion.Blazor.Grids.Internal
{
#pragma warning disable BL0005
    /// <summary>
    /// Helper for grid excel export.
    /// </summary>
    /// <typeparam name="T">TValue of grid.</typeparam>
    internal class GridExcelExport<T> : IDisposable
    {
        public GridExcelExport()
        {
        }

        private CultureInfo? culture;
        private PropertyInfoHelper<T> PropertyHelper = new PropertyInfoHelper<T>();
        private string _fileName = "Export.xlsx";
        private int ColumnIndex { get; set; } = -1;
        private bool _isChildGridInclude;
        private Worksheet? _sheet;
        private IEnumerable<object>? _dataSource;
        private List<GridColumn>? GridColumns { get; set; }

        private Dictionary<object, Dictionary<object, IEnumerable<object>>> distinctForeignKeyValue = new Dictionary<object, Dictionary<object, IEnumerable<object>>>();

        private bool IsTemplateColumnInclude { get; set; }

        private Dictionary<string, object>? ExportAggregate { get; set; }

        private bool IsCsvExport { get; set; }

        private SfGrid<T>? gridProp;
        private List<SpannedRow> SpannedCellIndex = new List<SpannedRow>();
        private object? _document;
        public DocumentOption _documentOption { get; set; }
        private bool groupSummary;
        private int TotalVisibleColumnsCount;

        private ExcelExportProperties? ExportProps { get; set; }

        private bool _detailrow;
        private int rowIndex;
        private string _trueValue = "true";
        private string _falseValue = "false";
        private string _theme = "default-theme";
        public string _emptyText = "No Records to display";
        private int exportColumnCount;
        private int colIndex;
        private AutoFormat? _autoFormat;
        internal bool isNewSheet;
        private bool _localSave;
        internal Workbook? _workbook;
        private Column? ExportColumn;
        private Row? Row;
        private Cell? ExcelGridCell;
        private int RecordIndex;
        private int GroupCurrentIndex;
        private int GroupRecordCount;
        private int colindexwidth = 35;
        private bool _isHideColumnInclude;
        private bool IsCustomCommandColumnInclude;
        private FontWeight _fontweight;

        public Type? SourceType { get; set; }

        private List<string> columnformat = new List<string>();
        private bool _isAutoFit;
        private bool _isAutoFitRows = true;

        private int ColumnDepth { get; set; } = 1;

        private bool isStackedHeaders { get; set; }

        private Regex rexExpression = new Regex(@"\D+", RegexOptions.Compiled);
        private Regex rexNumber = new Regex(@"^[cCdDnNpP]\d*$", RegexOptions.Compiled);
        private Regex rexStandard = new Regex(@"^[eEfFxX]\d*$", RegexOptions.Compiled);

        [Inject]
        [JsonIgnore]
        private ISyncfusionStringLocalizer? ExportLocalizer { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.  .
        /// </summary>
        /// <value>The name of the file.</value>
        /// <remarks></remarks>
        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the IsAutoFit for grid.    .
        /// </summary>
        /// <value>IsAutoFit in file.</value>
        /// <remarks></remarks>
        public bool IsAutoFit
        {
            get
            {
                return _isAutoFit;
            }

            set
            {
                _isAutoFit = value;
            }
        }

        /// <summary>
        /// Gets or sets the IsAutoFitRows for grid.    .
        /// </summary>
        /// <value>IsAutoFitRows in file.</value>
        /// <remarks></remarks>
        public bool IsAutoFitRows
        {
            get
            {
                return _isAutoFitRows;
            }

            set
            {
                _isAutoFitRows = value;
            }
        }

        /// <summary>
        /// Gets or sets the Theme of the file. .
        /// </summary>
        /// <value>The Theme of the file.</value>
        /// <remarks></remarks>
        public string Theme
        {
            get
            {
                return _theme;
            }

            set
            {
                _theme = value;
            }
        }

        // <summary>
        // Gets or sets the FontWeight.
        // </summary>
        // <value>FontWeight.</value>
        // <remarks></remarks>
        public FontWeight FontWeight
        {
            get
            {
                return _fontweight;
            }

            set
            {
                _fontweight = value;
            }
        }

        /// <summary>
        /// Gets or sets the File Path for saving file. .
        /// </summary>
        /// <value>The File Path for saving file in local.</value>
        /// <remarks></remarks>
        public string? FilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the is file locally saved. .
        /// </summary>
        /// <value>The LocalSave option for file.</value>
        /// <remarks></remarks>
        public bool LocalSave
        {
            get
            {
                return _localSave;
            }

            set
            {
                _localSave = value;
            }
        }

        // <summary>
        // Gets or sets the AutoFormat of the file.
        // </summary>
        // <value>The AutoFormat of the file.</value>
        // <remarks></remarks>
        public AutoFormat AutoFormat
        {
            get
            {
                return _autoFormat!;
            }

            set
            {
                _autoFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the Empty record text of the file. .
        /// </summary>
        /// <value>The name of the file.</value>
        /// <remarks></remarks>
        public string EmptyText
        {
            get
            {
                return _emptyText;
            }

            set
            {
                _emptyText = value;
            }
        }

        /// <summary>
        /// Gets or sets the datasource of the grid.    .
        /// </summary>
        /// <value>The Datasource of the grid.</value>
        /// <remarks></remarks>
        public IEnumerable<object> DataSource
        {
            get
            {
                return _dataSource!;
            }

            set
            {
                _dataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the IsHideColumnInclude for grid.  .
        /// </summary>
        /// <value>IsHideColumnInclude in file.</value>
        /// <remarks></remarks>
        public bool IsHideColumnInclude
        {
            get
            {
                return _isHideColumnInclude;
            }

            set
            {
                _isHideColumnInclude = value;
            }
        }

        /// <summary>
        /// Gets or sets the Header Text for multiple export.   .
        /// </summary>
        /// <value>HeaderText for multiple export.</value>
        /// <remarks></remarks>
        public string? HeaderText
        {
            get;
            set;
        }

        public bool IncludeDetailRow
        {
            get
            {
                return _detailrow;
            }

            set
            {
                _detailrow = value;
            }
        }

        /// <summary>
        /// Gets or sets the IncludeChildGrid to either included/exclude the child Grid.    .
        /// </summary>
        /// <value>true/false.</value>
        /// <remarks></remarks>
        public bool IncludeChildGrid
        {
            get
            {
                return _isChildGridInclude;
            }

            set
            {
                _isChildGridInclude = value;
            }
        }

        /// <summary>
        /// Gets or sets the Document.  .
        /// </summary>
        /// <value>The Document for exporting.</value>
        /// <remarks></remarks>
        public object Document
        {
            get
            {
                return _document!;
            }

            set
            {
                _document = value;
            }
        }

        // <summary>
        // Gets or sets the documentoption of the file.
        // </summary>
        // <value>The documentoption of the file.</value>
        // <remarks></remarks>
        public DocumentOption DocumentOption
        {
            get
            {
                return _documentOption;
            }

            set
            {
                _documentOption = value;
            }
        }

        /// <summary>
        /// Gets or sets the excel workbook.    .
        /// </summary>
        /// <value>The excel workbook.</value>
        /// <remarks></remarks>
        public Workbook WorkBook
        {
            get
            {
                return _workbook!;
            }

            set
            {
                _workbook = value;
            }
        }

        internal Worksheet Sheet
        {
            get
            {
                return _sheet!;
            }

            set
            {
                _sheet = value;
            }
        }

        private Cell GetCurrentCell
        {
            get
            {
                // Add bounds checking to prevent ArgumentOutOfRangeException
                if (rowIndex <= 0 || rowIndex > Sheet.Rows.Count)
                    return null!;
                
                var targetRow = Sheet.Rows[rowIndex - 1];
                if (colIndex <= 0 || colIndex > targetRow.Cells.Count)
                    return null!;
                
                return targetRow.Cells[colIndex - 1];
            }
        }

        internal async Task ExportHelper(SfGrid<T> gridModel, object dataSource)
        {
            _trueValue = ExportLocalizer!.GetText("Grid_True");
            _falseValue = ExportLocalizer.GetText("Grid_False");
            EmptyText = ExportLocalizer.GetText("Grid_EmptyRecord");
            await ExecuteResult(gridModel, dataSource).ConfigureAwait(true);
        }

        public async Task<MemoryStream> ExcelExport(SfGrid<T> GridModel, ExcelExportProperties? ExportProperties = null, bool isMemoryStreamExport = false)
        {
            if (GridModel.AllowExcelExport)
            {
                var eventStart = GridModel.GridEvents?.OnExcelExport;
                if (eventStart != null)
                {
                    eventStart("OnExcelExport");
                }
                await GridModel.EventAggregator.NotifyAsync("ExcelExport", "OnExcelExport").ConfigureAwait(true);

                GridColumns = GridUtils.GetColumns(GridModel).Clone()!;
                ExportLocalizer = GridModel.Localizer!;
                if (ExportProperties != null)
                {
                    ExportProps = ExportProperties;
                    FileName = ExportProperties.FileName ?? FileName;
                    IsHideColumnInclude = ExportProps.IncludeHiddenColumn;
                    IsTemplateColumnInclude = ExportProps.IncludeTemplateColumn;
                    IsCustomCommandColumnInclude = ExportProps.IncludeCommandColumn;
                    if (ExportProps.DataSource != null)
                    {
                        if (ExportProps.DataSource is SfDataManager && gridProp != null && gridProp.DataManager?.Json == null)
                        {
                            GridModel.DataManager = (SfDataManager)ExportProps.DataSource;
                        }
                        else
                        {
                            if(GridModel.DataManager != null)
                            {
                                GridModel.DataManager.Json = (IEnumerable<object>)ExportProps.DataSource;
                            }
                            
                            DataSource = (IEnumerable<object>)ExportProps.DataSource;
                        }
                    }
                }
                GridModel.EventAggregator.Trigger("ToolbarStateChanged", "ExcelExporting");
                if (GridModel.IsRenderedFromTreeGrid)
                {
                    await ExportHelper(GridModel, DataSource).ConfigureAwait(true);
                }
                else
                {
                    await Task.Run(() => ExportHelper(GridModel, DataSource)).ConfigureAwait(false);
                }
                using MemoryStream outputStream = new MemoryStream();
                if (IsCsvExport && ExportProperties?.Encoding != null)
                {
                    await Task.Run(() => _workbook?.Save(outputStream, true, ExportProperties.Encoding)).ConfigureAwait(false);
                }
                else
                {
                    await Task.Run(() => _workbook?.Save(outputStream, IsCsvExport)).ConfigureAwait(false);
                }
                if (isMemoryStreamExport)
                {
                    GridModel.EventAggregator.Trigger("ToolbarStateChanged", "ExcelExportCompleted");
                    return outputStream;
                }
                else
                {
                    await GridModel.InvokeMethod("sfBlazor.Grid.exportSave", new object[] { _fileName, Convert.ToBase64String(outputStream.ToArray()) }).ConfigureAwait(true);
                    var eventInfo = GridModel.GridEvents?.ExportComplete;
                    if (eventInfo != null)
                    {
                        eventInfo("Success");
                    }
                }
                GridModel.EventAggregator.Trigger("ToolbarStateChanged", "ExcelExportCompleted");
            }
            return null!;
        }

        public async Task<MemoryStream> CsvExport(SfGrid<T> GridModel, ExcelExportProperties? ExportProperties = null, bool isMemoryStreamExport = false)
        {
            IsCsvExport = true;
            FileName = "Export.csv";
            return await ExcelExport(GridModel, ExportProperties, isMemoryStreamExport: isMemoryStreamExport).ConfigureAwait(true);
        }

        public async Task ExecuteResult(SfGrid<T> GridModel, object dataSource)
        {
            culture = Intl.GetCulture();
            exportColumnCount = 0;
            gridProp = GridModel;
            if (gridProp.FilterSettings != null &&  gridProp.FilterSettings.Columns?.Count > 0)
            {
                for (int j = 0; j < gridProp.FilterSettings.Columns?.Count; j++)
                {
                    if (gridProp.FilterSettings.Columns[j].Value != null && GridColumns != null)
                    {
                        string fieldname = gridProp.FilterSettings.Columns[j].Field;
                        var column = GridUtils.GetColumnByField(fieldname, GridColumns);

                        if (column == null && GridColumns.Any(col => col.IsGridForeignColumn))
                        {
                            var fColsList = ForeignKey<T>.GetForeignKeyColumnsAsync(GridColumns);
                            column = fColsList.FirstOrDefault(col => col.ForeignKeyValue?.Equals(fieldname, StringComparison.Ordinal) == true);
                        }
                    }
                }
            }
            DataSource = (IEnumerable<object>)dataSource;
            if (ExportProps?.DataSource == null || gridProp.Aggregates?.Count > 0)
            {
                DataResult ExportData = await ExportHelper<T>.DataProcess(gridProp, ExportProps != null ? ExportProps.ExportType == Grids.ExportType.AllPages : true).ConfigureAwait(true);
                DataSource = (IEnumerable<object>)(ExportData.Result ?? Enumerable.Empty<object>());
                ExportAggregate = (Dictionary<string, object>)(ExportData.Aggregates ?? new Dictionary<string, object>());
            }
            InitializeExcel(_workbook!);
            List<GridColumn> gridcolumns = GridModel.IsFixedColumnPresent() ? GridModel.RearrangeColumns(GridColumns!)! : GridColumns!;
            GridColumns = ExportProps?.Columns?.Count > 0 ? GridUtils.GetColumns(GridModel, ExportProps.Columns).Clone()! : gridcolumns;
            if (gridProp.Columns != null && ExportProps?.Columns?.Count > 0)
            {
                var ColumnDepthCount = ExportHelper<T>.MeasureColumnDepth(gridProp.Columns);
                if (GridColumns?.Count != ExportProps.Columns.Count || ColumnDepthCount > 0)
                {
                    SetColumnIndex(ExportProps.Columns);
                }
                else
                {
                    SetColumnIndex(GridColumns);
                }
            }
            int templateColumncount = GridColumns?.Count(col => col.Template != null && col.Visible) ?? 0;
            int hideColumnCount = GridColumns?.Count(col => !col.Visible && col.Type != ColumnType.CheckBox) ?? 0;
            int count = GridColumns?.Count(col => col.Type != ColumnType.CheckBox) ?? 0;
            int columnCount = IsHideColumnInclude && IsTemplateColumnInclude ? count : IsHideColumnInclude ? count - templateColumncount : IsTemplateColumnInclude ? count - hideColumnCount : count - (templateColumncount + hideColumnCount);
            TotalVisibleColumnsCount = columnCount + (ExportHelper<T>.GetGroupColumnsCount(gridProp) > 0 ? ExportHelper<T>.GetGroupColumnsCount(gridProp) - 1 : 0);
            ExportHandler();
        }

        internal void SetColumnIndex(List<GridColumn> columns)
        {
            List<GridColumn> stackedcolumns = new List<GridColumn>();
            if (columns != null && columns.Count != 0)
            {
                foreach (var col in columns)
                {
                    col.Index = ++ColumnIndex;
                    if (col.Columns != null && col.Columns.Count > 0)
                    {
                        stackedcolumns.AddRange(col.Columns);
                    }
                }
            }
            if (stackedcolumns.Count > 0)
            {
                SetColumnIndex(stackedcolumns);
            }
        }

        private void ExportHandler()
        {
            IterateElements();
        }

        private void IterateElements()
        {
            if ((ExportProps == null || ExportProps.IncludeHeaderRow) && gridProp != null)
            {
                isStackedHeaders = false;
                colIndex = 0; //initcolindex and childlevel are always zero
                List<GridColumn> gridColumns = gridProp.IsFixedColumnPresent() ? gridProp.RearrangeColumns(gridProp.Columns!) : gridProp.Columns!;
                int ColCount = ExportProps?.Columns?.Count > 0 ? ExportProps.Columns.Count : gridColumns.Count;
                ColumnDepth = ExportHelper<T>.MeasureColumnDepth(gridProp.Columns!);
                if (GridColumns?.Count != ColCount || ColumnDepth > 0)
                {
                    List<GridColumn> stackedColumns = ExportProps?.Columns?.Count > 0 ? ExportProps.Columns : gridColumns;
                    var visibleColumns = stackedColumns.Where(column => (column.Visible || IsHideColumnInclude) && column.Type != ColumnType.CheckBox && (column.Template == null || IsTemplateColumnInclude) && (column.Commands == null || IsCustomCommandColumnInclude)).ToList();
                    ProcessStackedHeader(visibleColumns);
                }

                ProcessHeaderContent();
            }

            ProcessGridContents();

            if (ExportProps?.Footer != null)
            {
                int TotalHeaderRowsCount = (int)ExportProps.Footer.FooterRows;
                int value = TotalHeaderRowsCount - ExportProps.Footer.Rows!.Count;
                for (var i = 1; i <= TotalHeaderRowsCount; i++)
                {
                    rowIndex++;
                    Row = Sheet.Rows.Add();
                    Row.Index = rowIndex;
                    var cellCount = 0;
                    var indexValue = value > 0 ? value : 0;
                    if (i > indexValue)
                    {
                        cellCount = ExportProps.Footer.Rows[i - indexValue - 1].Cells?.Count ?? 0;
                    }

                    if (cellCount > 0)
                    {
                        for (var j = 0; j < cellCount; j++)
                        {
                            ExcelGridCell = Row.Cells.Add();
                            ExcelGridCell.Index = j + 1;
                            SetRowCellValues(Row.Cells[j], ExportProps.Footer.Rows[i - indexValue - 1].Cells![j]);
                        }
                    }
                }
            }
        }

        private void ProcessStackedHeader(List<GridColumn> Cols)
        {
            SpannedCellIndex.Clear();
            isStackedHeaders = true;
            int headerRows = (int)(ExportProps?.Header?.HeaderRows ?? 0);
            var ColDepth = ColumnDepth;
            if (ColDepth != 0)
            {
                for (var i = 0; i < ColDepth; i++)
                {
                    SpannedCellIndex.Add(new SpannedRow() { RowIndex = i, ColumnIndex = 0 });
                    rowIndex++;
                    Row = Sheet.Rows.Add();
                    Row.Index = rowIndex;
                    GetRowCells(Row);
                }

                foreach (GridColumn Column in Cols)
                {
                    if (headerRows == 0)
                    {
                        int stackRowIndex = SpannedCellIndex.Count - 1 + headerRows > 0 ? SpannedCellIndex.Count - 1 + headerRows : 1;
                        headerRows = ColDepth;
                        GenerateStackedRows(Column, headerRows > 0 ? rowIndex - (SpannedCellIndex.Count - 1) : stackRowIndex, ColDepth, SpannedCellIndex);
                    }
                    else
                    {
                        int stackRowIndex = SpannedCellIndex.Count - 1 + headerRows > 0 ? SpannedCellIndex.Count - 1 + headerRows : 1;
                        GenerateStackedRows(Column, headerRows > 0 ? rowIndex - (SpannedCellIndex.Count - 1) : stackRowIndex, ColDepth, SpannedCellIndex);
                    }
                }
            }
        }

        private void GenerateStackedRows(GridColumn Col, int RowIndex, int ColDepth, List<SpannedRow> SpannedCellIndex, bool isChildLevelStacked = false)
        {
            int CurrentCellIndex = 0;
            Cell? stackedHeaderCell = null;
            TextAlign sHAlign = Col.HeaderTextAlign != TextAlign.None ? Col.HeaderTextAlign : !GridUtils.IsNoneTextAlign(Col) ? Col.TextAlign : TextAlign.Left;
            int headerRows = (int)(ExportProps?.Header?.HeaderRows ?? 0);
            int groupCount = ExportHelper<T>.IsGroupingEnabled(gridProp!) ? (ExportHelper<T>.GetGroupColumnsCount(gridProp!) > 1 ? ExportHelper<T>.GetGroupColumnsCount(gridProp!) - 1 : 0) : 0;

            if (groupCount > 0)
            {
                CurrentCellIndex += groupCount;
            }

            if (Col.Columns == null)
            {
                CurrentCellIndex = ExportHelper<T>.GetCurrentCellIndex(SpannedCellIndex, CurrentCellIndex, headerRows > 0 ? RowIndex - headerRows - 1 : RowIndex - 1, colIndex);
                CurrentCellIndex = isChildLevelStacked ? CurrentCellIndex - 1 : CurrentCellIndex;
                colIndex++;
                Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].Value = Col.HeaderText;
                Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].CellStyle.HAlign = GetTextAlign(sHAlign);
                var range = Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex];
                CopyStyles(GridTableCellType.ColumnHeaderCell, range);
                for (var i = 0; i < SpannedCellIndex.Count; i++)
                {
                    SpannedCellIndex[i].SpannedCell = CurrentCellIndex;
                    SpannedCellIndex[i].ColumnIndex = ExportHelper<T>.IsGroupingEnabled(gridProp!) ? CurrentCellIndex + 1 - groupCount : CurrentCellIndex + 1;
                }

                Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].RowSpan = isChildLevelStacked ? SpannedCellIndex.Count : SpannedCellIndex.Count + 1;
                var eventInfo = gridProp!.GridEvents?.ExcelHeaderQueryCellInfoEvent;
                stackedHeaderCell = Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex];
                if (eventInfo != null || gridProp.IsRenderedFromTreeGrid)
                {
                    var EventArgs = new ExcelHeaderQueryCellInfoEventArgs()
                    {
                        Cell = stackedHeaderCell,
                        Column = Col,
                        Style = stackedHeaderCell.CellStyle,
                        Value = Col.HeaderText,
                        Colspan = stackedHeaderCell.ColumnSpan,
                        ColumnIndex = CurrentCellIndex + 1,
                        RowIndex = RowIndex
                    };
                    if (gridProp.IsRenderedFromTreeGrid)
                    {
                        gridProp.EventAggregator.NotifyAsync("TreeExcelHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                    }
                    else
                    {
                        if (eventInfo != null)
                        {
                            eventInfo(EventArgs);
                        }
                    }
                }

                Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].CellStyle.VAlign = VAlignType.Bottom;
                SpannedCellIndex[headerRows > 0 ? RowIndex - headerRows - 1 : RowIndex - 1].ColumnIndex = ExportHelper<T>.IsGroupingEnabled(gridProp) ? CurrentCellIndex + 1 - groupCount : CurrentCellIndex + 1;
            }
            else if (Col.Columns.Count > 0)
            {
                int ColSpan = ExportHelper<T>.GetColSpan(Col, 0, gridProp!);
                CurrentCellIndex = ExportHelper<T>.GetCurrentCellIndex(SpannedCellIndex, CurrentCellIndex, headerRows > 0 ? RowIndex - headerRows - 1 : RowIndex - 1, colIndex);
                colIndex++;
                if (ColSpan > 0)
                {
                    Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].Value = Col.HeaderText;
                    Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].CellStyle.HAlign = GetTextAlign(sHAlign);
                    Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex].ColumnSpan = ColSpan;
                    var range = Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex];
                    CopyStyles(GridTableCellType.ColumnHeaderCell, range);
                    int groupValue = ExportHelper<T>.GetGroupColumnsCount(gridProp!) == 0 || gridProp!.GroupSettings!.ShowGroupedColumn ? 1 : 0;
                    SpannedCellIndex[headerRows > 0 ? RowIndex - headerRows - 1 : RowIndex - 1].ColumnIndex = ExportHelper<T>.IsGroupingEnabled(gridProp!) ? CurrentCellIndex + (ColSpan - groupValue) - groupCount : CurrentCellIndex + ColSpan;
                    var eventInfo = gridProp!.GridEvents?.ExcelHeaderQueryCellInfoEvent;
                    stackedHeaderCell = Sheet.Rows[RowIndex - 1].Cells[CurrentCellIndex];
                    if (eventInfo != null || gridProp.IsRenderedFromTreeGrid)
                    {
                        var EventArgs = new ExcelHeaderQueryCellInfoEventArgs()
                        {
                            Cell = stackedHeaderCell,
                            Column = Col,
                            Style = stackedHeaderCell.CellStyle,
                            Value = Col.HeaderText,
                            Colspan = stackedHeaderCell.ColumnSpan,
                            ColumnIndex = CurrentCellIndex + 1,
                            RowIndex = RowIndex
                        };
                        if (gridProp.IsRenderedFromTreeGrid)
                        {
                            gridProp.EventAggregator.NotifyAsync("TreeExcelHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                        }
                        else
                        {
                            if (eventInfo != null)
                            {
                                eventInfo(EventArgs);
                            }
                        }
                    }
                }

                foreach (GridColumn InnerCol in Col.Columns)
                {
                    if (InnerCol.Columns != null)
                    {
                        GenerateStackedRows(InnerCol, RowIndex + 1, --ColDepth, SpannedCellIndex);
                    }
                    else
                    {
                        if (SpannedCellIndex.Count >= (RowIndex - headerRows + 1))
                        {
                            var stackIndex = SpannedCellIndex[RowIndex - headerRows].ColumnIndex;
                            SpannedCellIndex[RowIndex - headerRows].ColumnIndex = ++stackIndex;
                            GenerateStackedRows(InnerCol, RowIndex + 1, --ColDepth, SpannedCellIndex, true);
                        }
                    }
                }
            }
        }

        private void ProcessGridContents()
        {
            if (DataSource?.AsQueryable().Any() == true && ExportHelper<T>.GetGroupColumnsCount(gridProp!) > 0)
            {
                RenderGroupedData((IEnumerable<Group<T>>)DataSource);
                if (gridProp?.Aggregates != null)
                {
                    ProcessSummaryRow(DataSource.AsQueryable(), true);
                }
            }
            else
            {
                RenderRecord();
            }
        }

        private void RenderGroupedData(IEnumerable<Group<T>> groupedDatasource, int groupLevel = 0)
        {
            RecordIndex = ExportProps != null && !ExportProps.IncludeHeaderRow ? RecordIndex++ : RecordIndex;
            foreach (var grouRecord in groupedDatasource)
            {
                RenderGroupedRows(grouRecord, groupLevel);
            }
        }

        private void RenderRecord()
        {
            int count = DataSource?.AsQueryable().Count() ?? 0;
            if (count > 0)
            {
                foreach (T row in DataSource!)
                {
                    var rowIndex = ProcessRecordRow(row);

                    if ((ExportProps != null && (ExportProps.ExcelDetailRowMode == ExcelDetailRowMode.Expand || ExportProps.ExcelDetailRowMode == ExcelDetailRowMode.Collapse) || IsCsvExport) && ((IGrid)gridProp!).GridTemplates?.DetailTemplate != null)
                    {
                        var eventInfo = gridProp.GridEvents?.ExcelDetailTemplateExporting;
                        if (eventInfo != null)
                        {
                            var detailRow = ProcessDetailRow();
                            var eventArgs = new ExcelDetailTemplateEventArgs<T>()
                            {
                                ParentRow = new ParentRowInfo<T>() { Data = row, Index = rowIndex, Columns = GridColumns },
                                RowInfo = new ExcelDetailTemplateRowSettings() { }
                            };
                            eventInfo(eventArgs);
                            ProcessDetailTemplate(eventArgs, detailRow);
                        }
                    }
                }

                if (gridProp != null && gridProp.Aggregates != null)
                {
                    ProcessSummaryRow(DataSource.AsQueryable(), true);
                }
            }
            else
            {
                RenderEmptyTableBody();
            }
        }        

        private void ProcessDetailTemplate(ExcelDetailTemplateEventArgs<T> args, Row row, int outLineLevel = 1)
        {
            int detailIndent = 1 + outLineLevel;
            int detailCellIndex;
            if (args.RowInfo!.Headers != null || args.RowInfo.Rows != null)
            {
                void ProcessCell(ExcelDetailTemplateCell currentCell, Cell excelCell, int rowIndex, Row excelRow, bool isHeader)
                {
                    if (currentCell.Index != null)
                    {
                        currentCell.Index = detailCellIndex;
                        detailCellIndex++;
                    }
                    excelCell.Index = (int)currentCell.Index! + detailIndent;
                    if (currentCell.CellValue != null)
                    {
                        excelCell.Value = currentCell.CellValue;
                    }
                    if (currentCell.ColumnSpan != null)
                    {
                        excelCell.ColumnSpan = (int)currentCell.ColumnSpan;
                    }
                    if (currentCell.RowSpan != null)
                    {
                        excelCell.RowSpan = (int)currentCell.RowSpan;
                    }
                    if (isHeader)
                    {
                        HeaderCellTheme(excelCell);
                    }
                    else
                    {
                        RecordFieldCellTheme(excelCell);
                    }
                    if(currentCell.Style != null)
                    {
                        excelCell.CellStyle.FontColor = !string.IsNullOrEmpty(currentCell.Style.FontColor) ? ExportHelper<T>.GetHexValueFromColor(currentCell.Style.FontColor) : "#000000";
                        excelCell.CellStyle.FontSize = currentCell.Style.FontSize > 0 ? (int)currentCell.Style.FontSize : 10;
                        excelCell.CellStyle.FontName = !string.IsNullOrEmpty(currentCell.Style.FontName) ? currentCell.Style.FontName : AutoFormat.FontFamily;
                        if (currentCell.Style.HAlign != ExcelHorizontalAlign.Left)
                        {
                            excelCell.CellStyle.HAlign = Enum.Parse<HAlignType>(currentCell.Style.HAlign.ToString());
                        }

                        excelCell.CellStyle.VAlign = Enum.Parse<VAlignType>(currentCell.Style.VAlign.ToString());

                        if (!string.IsNullOrEmpty(currentCell.Style.BackColor))
                        {
                            excelCell.CellStyle.BackColor = ExportHelper<T>.GetHexValueFromColor(currentCell.Style.BackColor);
                        }

                        excelCell.CellStyle.Bold = currentCell.Style.Bold;
                        excelCell.CellStyle.Underline = currentCell.Style.Underline;
                        excelCell.CellStyle.Italic = currentCell.Style.Italic;
                        excelCell.CellStyle.WrapText = currentCell.Style.WrapText;
                    }
                    if (currentCell.Hyperlink != null) {
                        excelCell.Value = "<a href=" + currentCell.Hyperlink.Target + ">" + (!string.IsNullOrEmpty(currentCell.Hyperlink.DisplayText) ? currentCell.Hyperlink.DisplayText : currentCell.Hyperlink.Target) + " </a>";
                    }
                    if (currentCell.Image != null)
                    {
                        Sheet.Images.Add(currentCell.Image);
                    }
                }
                void ProcessRow(ExcelDetailTemplateRow detailTemplateRow, bool isHeader, int outerLineLevel = 1)
                {
                    var detailRow = row ?? ProcessDetailRow();
                    row = null!;
                    detailRow.Grouping.OutlineLevel = outerLineLevel;
                    if (ExportProps?.ExcelDetailRowMode == ExcelDetailRowMode.Collapse)
                    {
                        detailRow.Grouping.IsCollapsed = true;
                        detailRow.Grouping.IsHidden = true;
                    }
                    detailCellIndex = 0;
                    for (var j = 0; j < detailTemplateRow.Cells?.Count; j++)
                    {
                        detailRow.Cells.Add();
                        var currentCell = detailTemplateRow.Cells[j];
                        ProcessCell(currentCell, detailRow.Cells[j], detailRow.Index, detailRow, isHeader);
                    }
                    Sheet.Rows.Add(detailRow); 
                    if (detailTemplateRow.ChildRowInfo != null)
                    {
                        var childRow = ProcessDetailRow();
                        ProcessDetailTemplate(new ExcelDetailTemplateEventArgs<T>()
                        {
                            ParentRow = args.ParentRow,
                            RowInfo = detailTemplateRow.ChildRowInfo
                        }, childRow, ++outerLineLevel);
                    }
                }
                if (args.RowInfo.Headers != null)
                {
                    for (var i = 0; i < args.RowInfo.Headers.Count; i++)
                    {
                        ProcessRow(args.RowInfo.Headers[0], true);
                    }
                }
                if (args.RowInfo.Rows != null)
                {
                    for (var i = 0; i < args.RowInfo.Rows.Count; i++)
                    {
                        ProcessRow(args.RowInfo.Rows[i], false);
                    }
                }
            }
            else if (args.RowInfo.Image != null)
            {
                row.Grouping.OutlineLevel = outLineLevel;
                if (ExportProps?.ExcelDetailRowMode == ExcelDetailRowMode.Collapse)
                {
                    row.Grouping.IsCollapsed = true;
                    row.Grouping.IsHidden = true;
                }
                GetRowCells(Row!);
                Sheet.Images.Add(args.RowInfo.Image);
            }
            else if (args.RowInfo.Text != null)
            {
                row.Grouping.OutlineLevel = outLineLevel;
                if (ExportProps?.ExcelDetailRowMode == ExcelDetailRowMode.Collapse)
                {
                    row.Grouping.IsCollapsed = true;
                    row.Grouping.IsHidden = true;
                }
                GetRowCells(row);
                Sheet.Rows[row.Index - 1].Cells[1].Value = args.RowInfo.Text;
            }
            else if (args.RowInfo.Hyperlink != null)
            {
                row.Grouping.OutlineLevel = outLineLevel;
                if (ExportProps?.ExcelDetailRowMode == ExcelDetailRowMode.Collapse)
                {
                    row.Grouping.IsCollapsed = true;
                    row.Grouping.IsHidden = true;
                }
                GetRowCells(row);
                Sheet.Rows[row.Index - 1].Cells[1].Value = "<a href=" + args.RowInfo.Hyperlink.Target + ">" + (!string.IsNullOrEmpty(args.RowInfo.Hyperlink.DisplayText) ? args.RowInfo.Hyperlink.DisplayText : args.RowInfo.Hyperlink.Target) + " </a>";
            }
        }

        private Row ProcessDetailRow()
        {
            rowIndex++;
            Row = Sheet.Rows.Add();
            Row.Index = rowIndex;
            return Row;
        }
        private void RenderGroupedRows(Group<T> context, int groupLevel)
        {
            var captionSummaryIndex = 0;
            rowIndex++;
            Row = Sheet.Rows.Add();
            Row.Index = rowIndex;
            GetRowCells(Row);
            colIndex = 1;
            exportColumnCount = exportColumnCount > 0 ? exportColumnCount : TotalVisibleColumnsCount;
            var colspanLength = exportColumnCount + ExportHelper<T>.GetGroupColumnsCount(gridProp!);
            string groupColumnName = context.Field ?? "";
            var groupCol = GridUtils.GetColumnByField(groupColumnName, GridColumns!);
            bool flag = (groupCol?.Type == ColumnType.Date || groupCol?.Type == ColumnType.DateTime) ? true : false;
            var keyName = context.Key == null ? (flag == true ? context.Key : string.Empty) : context.Key;
            string? formatstring = groupCol?.Format;
            int count = 0;
            if (gridProp?.Aggregates != null && GridColumns != null)
            {
                foreach (GridAggregate sumRow in gridProp.Aggregates)
                {
                    foreach (GridAggregateColumn summaryCol in sumRow.Columns!)
                    {
                        GridColumn? SummaryGridColumn = GridColumns.FirstOrDefault(e => e.Field == summaryCol.Field);
                        groupSummary = !groupSummary ? summaryCol.GroupFooterTemplate != null : groupSummary;
                        //Render group summary column captiontemplate
                        if (summaryCol.GroupCaptionTemplate != null)
                        {
                            Tuple<int, int> tuple = RenderGroupSummaryCaption(summaryCol, SummaryGridColumn!, context, count, captionSummaryIndex, AggregateTemplateType.GroupCaption);
                            count = tuple.Item1;
                            captionSummaryIndex = tuple.Item2;
                        }
                    }
                }
            }

            colIndex = 1;
            for (int i = 0; i < groupLevel && i + 1 < Sheet.Rows[rowIndex - 1].Cells.Count; i++)
            {
                Sheet.Rows[rowIndex - 1].Cells[i + 1].Value = string.Empty;
                var range2 = GetCurrentCell;
                colIndex++;
            }

            int mergeCount = count != 0 ? count + ExportHelper<T>.GetGroupColumnsCount(gridProp!) - 1 : colspanLength;

            int captionCount = context.CountItems;
            string itemText = captionCount == 1 ? ExportLocalizer!.GetText("Grid_Item"): ExportLocalizer!.GetText("Grid_Items");
            string txt = context.HeaderText ?? (!string.IsNullOrEmpty(groupCol?.HeaderText) ? groupCol.HeaderText : context.Field ?? "");
            string headerText = txt ?? "";
            using (var PropertyHelper = new PropertyInfoHelper())
            {
                if (!string.IsNullOrEmpty(groupCol?.ForeignKeyValue) && groupCol?.GetForeignData() != null)
                {
                    object foreignColumnData = groupCol.GetForeignData();
                    var FData = GridUtils.GetForeignData(groupCol, context, foreignColumnData);
                    foreach (var val in (List<object>)FData)
                    {
                        keyName = PropertyHelper.GetObject(groupCol.ForeignKeyValue, val);
                    }
                }
            }
            string groupKey = headerText.Replace("{{:key}}", Convert.ToString((!string.IsNullOrEmpty(formatstring) ? string.Format(CultureInfo.CurrentCulture, formatstring, keyName) : keyName ?? ""), culture), StringComparison.Ordinal);
            if (ExportProps?.Theme?.Caption?.WrapText != true)
            {
                Sheet.Rows[RecordIndex].Height = 20;
            }
            for (var i = colIndex - 1; i < Sheet.Rows[rowIndex - 1].Cells.Count; i++)
            {
                CopyBorders(GridTableCellType.GroupCaptionCell, Row.Cells[i]);
                CopyStyles(GridTableCellType.GroupCaptionCell, Row.Cells[i]);
            }

             RenderCaption(headerText, context, groupCol!, itemText);
            if (mergeCount > 0)
            {
                mergeCount = mergeCount - (groupLevel + 1);
                Sheet.Rows[rowIndex - 1].Cells[colIndex - 1].ColumnSpan = captionSummaryIndex != 0 ? captionSummaryIndex - (colIndex - 1) : mergeCount;
            }

            RenderGroupSummaryDataRow(context, groupLevel);
        }
        private void RenderGroupSummaryDataRow(Group<T> context, int groupLevel)
        {
            if (!(context.Items is IEnumerable<Group<T>>) && context.Items != null)
            {
                GroupRecordCount = context.Items.AsQueryable().Count();
                GroupCurrentIndex = 0;
                foreach (T record in context.Items)
                {
                    GroupCurrentIndex++;
                    ProcessRecordRow(record, context.Level, context);
                }

                if (gridProp != null && gridProp.Aggregates != null && groupSummary)
                {
                    ProcessSummaryRow(null!, false, context);
                }
            }
            else
            {
                groupLevel++;
                RenderGroupedData((context.Items as IEnumerable<Group<T>>)!, groupLevel);
                if (gridProp != null &&  gridProp.Aggregates != null && groupSummary)
                {
                    ProcessSummaryRow(null!, false, context);
                }
            }
        }

        private void RenderCaption(string headerText, Group<T> context, GridColumn groupCol, string itemText)
        {
            string caption = string.Empty;
            int captionCount = context.CountItems;
            if (gridProp != null && gridProp.GroupSettings != null && gridProp.GroupSettings.CaptionTemplate != null)
            {
                var eventInfo = gridProp.GridEvents?.ExcelGroupCaptionTemplateInfo;
                if (eventInfo == null)
                {
                    caption = $"{headerText}: {context.Key} - {captionCount} {itemText}";
                    ExportRecordRow(caption);
                }
                else
                {
                    var firstCell = Sheet.Rows[rowIndex - 1].Cells[colIndex - 1];
                    var borders = firstCell.CellStyle.Borders;
                    for (var i = colIndex - 1; i < Sheet.Rows[rowIndex - 1].Cells.Count; i++)
                    {
                        var currentCell = Sheet.Rows[rowIndex - 1].Cells[i];
                        currentCell.CellStyle.Borders = borders;
                        CopyBorders(GridTableCellType.GroupCaptionCell, currentCell);
                        CopyStyles(GridTableCellType.GroupCaptionCell, currentCell);
                    }
                    
                    var eventArgs = new ExcelCaptionTemplateArgs()
                    {
                        Cell = GetCurrentCell,
                        Column = groupCol,
                        Value = $"{headerText}: {context.Key} - {captionCount} {itemText}",
                        Style = GetCurrentCell.CellStyle,
                        Count = context.CountItems,
                        Field = groupCol.Field,
                        HeaderText = groupCol.HeaderText,
                        Key = context.Key?.ToString()!,
                    };
                    if (groupCol.IsForeignColumn())
                    {
                        var foreignColumnData = groupCol.ColumnData;
                        var query = new List<WhereFilter>()
                                    {
                                        new WhereFilter()
                                        {
                                            Field = groupCol.ForeignKeyField ?? groupCol.Field,
                                            value = context.Key,
                                            IgnoreCase = false,
                                            Operator = "equal"
                                        }
                                    };
                        eventArgs.ForeignKeyValue = groupCol.ForeignKeyValue;
                        var foreignData = DataOperations.PerformFiltering(foreignColumnData as IEnumerable<object> ?? Enumerable.Empty<object>(), query, "and").ToList();
                        using (var PropertyHelper = new PropertyInfoHelper())
                        {
                            foreach (var val in foreignData)
                            {
                                eventArgs.ForeignKey = PropertyHelper.GetObject(groupCol.ForeignKeyValue!, val)?.ToString()!;
                            }
                        }
                    }
                    ExportRecordRow(eventArgs.Value.ToString()!);
                    eventInfo(eventArgs);
                }
            }
            else
            {
                var keyname = context.Key;
                var foreignColumnData = groupCol?.GetForeignData();
                if (!string.IsNullOrEmpty(groupCol?.ForeignKeyValue) && foreignColumnData != null)
                {
                    var groupedData = (context as Group<T>).Key;
                    var field = groupCol.ForeignKeyField ?? groupCol.Field;
                    var query = new List<WhereFilter>()
                    {
                        new WhereFilter()
                        {
                            Field = field,
                            value = groupedData,
                            IgnoreCase = false,
                            Operator = "equal"
                        }
                    };
                    var foreginKeyData = groupCol.GetForeignkeyFilteredData((foreignColumnData as IEnumerable<object>)!, query).FirstOrDefault();
                    keyname = PropertyHelper.GetObject(groupCol.ForeignKeyValue, foreginKeyData!);
                }
                caption = $"{headerText}: {keyname} - {captionCount} {itemText}";
                ExportRecordRow(caption);
            }

            if (gridProp != null && gridProp.GroupSettings != null && gridProp.GroupSettings.Columns?.Length < 8 && context.Level > 1)
            {
                Sheet.Rows[rowIndex - 1].Grouping.OutlineLevel = context.Level - 1;
            }
        }

        private static object? GetValue(object summaryValue, string format, bool applyFormat) 
        {
            var type = summaryValue?.GetType();
            object? Value = null;
            if (type == null)
            {
                Value = null!;
            }
            else if (type == typeof(string))
            {
                Value = summaryValue?.ToString();
            }
            else if (type == typeof(DateTime))
            {
                DateTime dateTimeValue = Convert.ToDateTime(summaryValue, CultureInfo.InvariantCulture);
                Value = applyFormat ? dateTimeValue.ToString(format, CultureInfo.CurrentCulture) : dateTimeValue;
            }
            else
            {
                decimal decimalValue = Convert.ToDecimal(summaryValue, CultureInfo.InvariantCulture);
                Value = applyFormat ? decimalValue.ToString(format, CultureInfo.CurrentCulture) : decimalValue;
            }
            return Value;
        }

        private Tuple<int, int> RenderGroupSummaryCaption(GridAggregateColumn summaryCol, GridColumn SummaryGridColumn, Group<T> context, int count, int captionSummaryIndex, AggregateTemplateType templateType)
        {
            if (SummaryGridColumn?.Visible == true)
            {
                summaryCol.ColumnName = summaryCol.ColumnName ?? summaryCol.Field;
                count = 0;
                object SummaryValue = string.Empty;
                ExportHelper<T>.GetSummaryAndCount(GridColumns!, context, summaryCol, IsHideColumnInclude, IsTemplateColumnInclude, IsCustomCommandColumnInclude, ref SummaryValue, ref count);
                colIndex = count + ExportHelper<T>.GetGroupColumnsCount(gridProp!) - 1;
                if (captionSummaryIndex == 0)
                {
                    captionSummaryIndex = colIndex - 1;
                }

                Sheet.Rows[rowIndex - 1].Cells[colIndex - 1].Value = SummaryValue;
                if (!string.IsNullOrEmpty(summaryCol.Format))
                {
                    SetValueByColumnFormat(SummaryValue, SummaryGridColumn, summaryCol);
                }
                var eventInfo = gridProp!.GridEvents?.ExcelAggregateTemplateInfo;
                if ((summaryCol.GroupCaptionTemplate != null && eventInfo != null) || gridProp.IsRenderedFromTreeGrid)
                {
                    var eventArgs = new ExcelAggregateEventArgs()
                    {
                        Cell = GetCurrentCell,
                        Column = summaryCol,
                        Value = GetValue(SummaryValue, summaryCol.Format!, true)!,
                        Style = GetCurrentCell.CellStyle,
                        GroupKey = context.Key?.ToString()!,
                        AggregateType = templateType
                    };
                    if (gridProp.IsRenderedFromTreeGrid)
                    {
                        gridProp.EventAggregator.NotifyAsync("TreeExcelAggregateTemplateInfo", eventArgs).ConfigureAwait(false);
                    }
                    else
                    {
                        if (eventInfo != null)
                        {
                            eventInfo(eventArgs);
                        }
                    }
                }
                else
                {
                    Row!.Cells[colIndex - 1].Value = summaryCol.Type + ":" + GetValue(SummaryValue, summaryCol.Format!, true);
                }
                Row!.Cells[colIndex - 1].CellStyle.HAlign = Enum.Parse<HAlignType>(GetTextAlign(SummaryGridColumn.TextAlign).ToString());
                if (Theme != "none")
                {
                    CopyStyles(GridTableCellType.CaptionCell, GetCurrentCell);
                }
            }
            return new Tuple<int, int>(count, captionSummaryIndex);
        }
              
        private void ProcessSummaryRow(IQueryable items, bool totalSummary, Group<T> GroupData = null!)
        {
            IQueryable item = items;
            bool totalSum = totalSummary;
            colIndex = 1;
            int groupedColumnsCount = ExportHelper<T>.GetGroupColumnsCount(gridProp!);
            foreach (GridAggregate summaryRow in gridProp!.Aggregates!)
            {
                int summaryColumnTitleIndex = 0;
                rowIndex++;
                Row = Sheet.Rows.Add();
                Row.Index = rowIndex;
                GetRowCells(Row);
                Sheet.Rows[this.rowIndex - 1].Grouping.OutlineLevel = (groupedColumnsCount < 8 && GroupData?.Level > 0) ? GroupData.Level : 0;               
                foreach (var summaryCol in summaryRow.Columns!)
                {
                    bool isFooterInclude = totalSummary ? (summaryCol.FooterTemplate != null) : (summaryCol.GroupFooterTemplate != null);
                    if (isFooterInclude && GridColumns != null)
                    {
                        summaryCol.ColumnName = summaryCol.ColumnName ?? summaryCol.Field;
                        var SummaryGridColumn = GridColumns.FirstOrDefault(F => F.Field == summaryCol.ColumnName);
                        if (SummaryGridColumn == null || (!SummaryGridColumn.Visible && !IsHideColumnInclude))
                        {
                            continue;
                        }
                        summaryColumnTitleIndex = GetSummaryColumnTitleIndex(summaryColumnTitleIndex, summaryCol);
                        summaryColumnTitleIndex = summaryColumnTitleIndex + (groupedColumnsCount > 0 ? groupedColumnsCount - 1 : groupedColumnsCount);
                        object summaryValue = string.Empty;
                        string prefix = string.Empty;
                        string KeyValue = summaryCol.Field + " " + "-" + " " + summaryCol.Type?.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        string GroupKeyValue = " ";
                        IDictionary<string, object> SummaryObj = ExportAggregate!;
                        if (groupSummary && ExportHelper<T>.IsGroupingEnabled(gridProp) && GroupData != null)
                        {
                            GroupKeyValue = summaryCol.Field + " " + "-" + " " + summaryCol.Type?.ToString();
                            SummaryObj = (IDictionary<string, object>)(GroupData.Aggregates ?? new Dictionary<string, object>());
                        }

                        if(SummaryObj != null)
                        {
                            foreach (var SummaryData in SummaryObj)
                            {
                                if (SummaryData.Key == KeyValue || SummaryData.Key == GroupKeyValue)
                                {
                                    summaryValue = SummaryData.Value;
                                    break;
                                }
                            }
                        }
                        
                        prefix = summaryCol.Type!.ToString()!;
                        colIndex = 0;
                        //process the summary column
                        ProcessSummaryColumn(summaryCol);
                        var indentCell = ExportHelper<T>.GetGroupColumnsCount(gridProp);
                        for (var j = indentCell > 0 ? indentCell - 1 : indentCell; j < TotalVisibleColumnsCount; j++)
                        {
                            CopyStyles(GridTableCellType.CaptionCell, Row.Cells[j]);
                        }

                        Sheet.Rows[rowIndex - 1].Cells[summaryColumnTitleIndex - 1].Value = GetValue(summaryValue, summaryCol.Format!, false);
                        Sheet.Rows[rowIndex - 1].Cells[summaryColumnTitleIndex - 1].CellStyle.HAlign = Enum.Parse<HAlignType>(GetTextAlign(SummaryGridColumn.TextAlign).ToString());
                        var value = Sheet.Rows[rowIndex - 1].Cells[summaryColumnTitleIndex - 1].Value;
                        if(!string.IsNullOrEmpty(summaryCol.Format))
                        {
                            SetValueByColumnFormat(value, SummaryGridColumn, summaryCol);
                        }
                        if (summaryRow != null)
                        {
                            var range = Sheet.Rows[rowIndex - 1].Cells[summaryColumnTitleIndex - 1];
                            range.CellStyle.HAlign = Enum.Parse<HAlignType>(GetTextAlign(SummaryGridColumn.TextAlign).ToString());
                            var eventInfo = gridProp.GridEvents?.ExcelAggregateTemplateInfo;
                            if (((summaryCol.FooterTemplate != null || summaryCol.GroupFooterTemplate != null) && eventInfo != null) || gridProp.IsRenderedFromTreeGrid)
                            {
                                var eventArgs = new ExcelAggregateEventArgs()
                                {
                                    Cell = GetCurrentCell,
                                    Column = summaryCol,
                                    Value = GetValue(summaryValue, summaryCol.Format!, true)!,
                                    Style = GetCurrentCell.CellStyle,
                                    GroupKey = GroupData?.Key?.ToString() ?? "",
                                    AggregateType = totalSummary
                                                    ? AggregateTemplateType.Footer
                                                    : AggregateTemplateType.GroupFooter
                                };
                                if (gridProp.IsRenderedFromTreeGrid)
                                {
                                    gridProp.EventAggregator.NotifyAsync("TreeExcelAggregateTemplateInfo", eventArgs).ConfigureAwait(false);
                                }
                                else
                                {
                                    if (eventInfo != null)
                                    {
                                        eventInfo(eventArgs);
                                    }
                                }
                            }
                            else
                            {
                                Sheet.Rows[rowIndex - 1].Cells[summaryColumnTitleIndex - 1].Value = prefix + ":" + GetValue(summaryValue, summaryCol.Format!, true);
                            }
                        }
                    }

                    summaryColumnTitleIndex = 0;
                }
            }
        }

        private void ProcessSummaryColumn(GridAggregateColumn summaryCol)
        {
            bool summaryColumnVisible = true;
            int groupColsCount = ExportHelper<T>.GetGroupColumnsCount(gridProp!);
            if (!string.IsNullOrEmpty(summaryCol.ColumnName) && GridColumns != null)
            {
                foreach (var column in GridColumns)
                {
                    bool visColumn = column.Visible || IsHideColumnInclude;
                    if (summaryCol.Field == column.Field && !visColumn)
                    {
                        summaryColumnVisible = visColumn;
                        break;
                    }

                    bool customCommands = (column.Commands?.Count > 0 && IsCustomCommandColumnInclude) || column.Commands == null;
                    bool tempColumn = (column.Template != null && IsTemplateColumnInclude) || column.Template == null;
                    if (visColumn && tempColumn && customCommands && column.Type != ColumnType.CheckBox)
                    {
                        colIndex++;
                    }

                    if (column.Field == summaryCol.ColumnName)
                    {
                        break;
                    }
                }

                colIndex = colIndex + (groupColsCount > 0 ? groupColsCount - 1 : groupColsCount);
            }
            else
            {
                colIndex = exportColumnCount + (groupColsCount > 0 ? groupColsCount - 1 : groupColsCount);
                while (!string.IsNullOrEmpty(GetCurrentCell?.Value?.ToString()))
                {
                    colIndex--;
                }
            }
        }
        private int GetSummaryColumnTitleIndex(int summaryColumnTitleIndex, GridAggregateColumn summaryColumn)
        {
            foreach (var column in GridColumns!)
            {
                if (column.Type != ColumnType.CheckBox)
                {
                    summaryColumnTitleIndex++;
                }

                if ((!IsHideColumnInclude && !column.Visible) || (!IsTemplateColumnInclude && column.Template != null))
                {
                    summaryColumnTitleIndex--;
                }

                if (summaryColumn.ColumnName == column.Field)
                {
                    break;
                }
            }
            return summaryColumnTitleIndex;
        }

        private void ProcessIndentCell(int indentCell)
        {
            colIndex++;
            ExcelGridCell = Row!.Cells.Add();
            ExcelGridCell.Index = indentCell;
            ExportColumn = Sheet.Columns.Add();
            ExportColumn.Index = indentCell;
            ExportRecordRow(string.Empty);
        }

        private int ProcessRecordRow(T row, int level = 0, Group<T> parentGroup = null!)
        {
            rowIndex++;
            RecordIndex++;
            Row = Sheet.Rows.Add();
            Row.Index = rowIndex;
            GetRowCells(Row);
            var eventArgs = new ExportRowDataBound<T>()
            {
                OutlineLevel = 0,
                RowData = row,
                SheetRow = Row,
                IsHidden = false,
                IsCollapsed = false
            };
            gridProp!.EventAggregator.Trigger("ExportDataBound", eventArgs);
            var sheetRow = Sheet.Rows[rowIndex - 1];
            if (eventArgs.OutlineLevel != 0)
            {
                sheetRow.Grouping.OutlineLevel = eventArgs.OutlineLevel;
            }
            if (eventArgs.IsHidden)
            {
                sheetRow.Grouping.IsHidden = eventArgs.IsHidden;
            }
            if (eventArgs.IsCollapsed)
            {
                sheetRow.Grouping.IsCollapsed = eventArgs.IsCollapsed;
            }
            int groupColumnsCount = ExportHelper<T>.GetGroupColumnsCount(gridProp);
            if (groupColumnsCount < 8 && level > 0)
            {
                sheetRow.Grouping.OutlineLevel = level;
            }

            colIndex = 0;
            for (int i = 1; i < groupColumnsCount; i++)
            {
                colIndex++;
                ExportRecordRow(string.Empty);
            }

            Sheet.IsSummaryRowBelow = false;
            Row<object>? gridRowData = null!;
            gridRowData = gridProp.Rows?.FirstOrDefault(r => r.Data is T dataOfT && EqualityComparer<T>.Default.Equals(dataOfT, row))!;

            if (gridRowData == null && ExportHelper<T>.IsGroupingEnabled(gridProp) && parentGroup != null)
            {
                gridRowData = gridProp.Rows?.FirstOrDefault(r =>
                {
                    if (r.Data is Group<T> group)
                    {
                        return group.Key?.Equals(parentGroup.Key) == true &&
                               (group.Items as IEnumerable<T>)?.Contains(row, EqualityComparer<T>.Default) == true;
                    }
                    return false;
                });
            }
            if (gridRowData == null && ExportHelper<T>.IsGroupingEnabled(gridProp))
            {
                gridRowData = gridProp.Rows?.FirstOrDefault(r =>
                {
                    if (r.Data is Group<T> groupData)
                    {
                        var items = groupData.Items as IEnumerable<T>;
                        return items?.Contains(row, EqualityComparer<T>.Default) == true;
                    }
                    return false;
                });
            }

            if (GridColumns != null && gridProp.AutoSpan != AutoSpanMode.None && gridRowData != null && gridRowData.Cells != null && !IsCsvExport) 
            {
                ForeignKey<T>.FetchForeignKeyRow(gridRowData, row!, GridColumns, distinctForeignKeyValue);
                var dataCells = gridRowData.Cells.Where(c => c.CellType == CellType.Data).ToList();
                foreach (var cell in dataCells)
                {
                    var column = cell.Column;
                    if (column == null) continue;

                    bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                    if (ExportHelper<T>.IsGroupingEnabled(gridProp) && column.Visible && gridProp.GroupSettings != null && gridProp.GroupSettings.Columns?.Contains(column.Field) == true)
                    {
                        isVisibleColumn = gridProp.GroupSettings.ShowGroupedColumn;
                    }

                    bool customCommands = (column.Commands?.Count > 0 && IsCustomCommandColumnInclude) || column.Commands == null;
                    bool templateColumn = (column.Template != null && IsTemplateColumnInclude) || column.Template == null;
                    if (isVisibleColumn && templateColumn && customCommands)
                    {
                        if (cell.IsRowSpanned == true)
                        {
                            if (column.Type != ColumnType.CheckBox)
                            {
                                int spanCols = (cell.ColSpan != null && cell.ColSpan > 1) ? (int)cell.ColSpan : 1;
                                for (int s = 0; s < spanCols; s++)
                                {
                                    colIndex++;
                                    var currentCell = GetCurrentCell;
                                    if (currentCell != null)
                                    {
                                        CopyBorders(GridTableCellType.RecordFieldCell, currentCell);
                                        CopyStyles(GridTableCellType.RecordFieldCell, currentCell);
                                    }
                                }
                            }
                        }
                        else if (cell.IsSpanned != true)
                        {
                            ProcessRecordCell(row, column, gridRowData.ForeignKeyData!, cell);
                        }
                    }
                }
            }
            else
            {
                Row<object> dataRow = new Row<object>()
                {
                    ForeignKeyData = new Dictionary<string, IEnumerable<object>>(),
                };
                ForeignKey<T>.FetchForeignKeyRow(dataRow, row!, GridColumns!, distinctForeignKeyValue);
                foreach (var column in GridColumns!)
                {
                    bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                    if (ExportHelper<T>.IsGroupingEnabled(gridProp) && column.Visible && gridProp.GroupSettings != null && gridProp.GroupSettings.Columns?.Contains(column.Field) == true)
                    {
                        isVisibleColumn = gridProp.GroupSettings.ShowGroupedColumn;
                    }

                    bool customCommands = (column.Commands?.Count > 0 && IsCustomCommandColumnInclude) || column.Commands == null;
                    bool templateColumn = (column.Template != null && IsTemplateColumnInclude) || column.Template == null;
                    if (isVisibleColumn && templateColumn && customCommands)
                    {
                        ProcessRecordCell(row, column, dataRow.ForeignKeyData);
                    }
                }
            }
            return rowIndex;
        }

        private void RenderEmptyTableBody()
        {
            rowIndex++;
            Row = Sheet.Rows.Add();
            Row.Index = rowIndex;
            GetRowCells(Row);
            colIndex = 0; // initColIndex is always 0.
            Sheet.Rows[rowIndex - 1].Height = 20;
            colIndex++;
            ExportRecordRow(EmptyText);
            CopyStyles(GridTableCellType.RecordFieldCell, GetCurrentCell);
            int colspanLength = exportColumnCount + ExportHelper<T>.GetGroupColumnsCount(gridProp!);
            Sheet.Rows[rowIndex - 1].Cells[colIndex - 1].ColumnSpan = colspanLength;
            CopyBorders(GridTableCellType.RecordFieldCell, Sheet.Rows[rowIndex - 1].Cells[colIndex - 1]);
        }

        private void ProcessHeaderContent()
        {
            rowIndex++;
            Row = Sheet.Rows.Add();
            Row.Index = rowIndex;
            colIndex = 0; // initColIndex and ChildLevel is always 0
            if (ExportHelper<T>.IsGroupingEnabled(gridProp!) && gridProp!.GroupSettings != null)
            {
                int indentCell = 0;
                foreach (string groupedColumn in gridProp.GroupSettings.Columns!)
                {
                    if (groupedColumn == gridProp.GroupSettings.Columns?.FirstOrDefault())
                    {
                        continue;
                    }
                    ++indentCell;
                    ProcessIndentCell(indentCell);
                    Sheet.Columns[colIndex - 1].Width = colindexwidth;
                }
            }
            RenderColumnHeader();
        }

        private void RenderColumnHeader()
        {
            foreach (var column in GridColumns!)
            {
                columnformat.Add(string.Empty);
                bool visColumn = column.Visible || IsHideColumnInclude;
                bool customCommands = (column.Commands?.Count > 0 && IsCustomCommandColumnInclude) || column.Commands == null;
                bool tempColumn = ((column.Template != null) && IsTemplateColumnInclude) || column.Template == null;
                if (visColumn && tempColumn && customCommands && column.Type != ColumnType.CheckBox)
                {
                    if (ExportHelper<T>.IsGroupingEnabled(gridProp!) && gridProp?.GroupSettings != null && !gridProp.GroupSettings.ShowGroupedColumn)
                    {
                        if (gridProp.GroupSettings.Columns != null && !gridProp.GroupSettings.Columns.Contains(column.Field) || IsHideColumnInclude)
                        {
                            exportColumnCount++;
                            ProcessColumnHeader(column);
                        }
                        else
                        {
                            column.Visible = gridProp.GroupSettings.ShowGroupedColumn;
                        }
                    }
                    else
                    {
                        exportColumnCount++;
                        ProcessColumnHeader(column);
                    }
                }

                if (!IsAutoFit && tempColumn && (column.Visible || IsHideColumnInclude) && customCommands && column.Width != null && !column.Width.ToString().Contains('%', StringComparison.Ordinal) && column.Type != ColumnType.CheckBox)
                {
                    if (decimal.TryParse(column.Width, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        column.Width = Math.Round(result).ToString(CultureInfo.InvariantCulture);
                    }
                    // If column.Width is null or empty, or if column.Width is "auto", set the column width to "0".
                    Sheet.Columns[colIndex - 1].Width = !string.IsNullOrEmpty(column.Width) && column.Width != "auto" ? column.Width.IndexOf("px", StringComparison.Ordinal) > 0 ? Convert.ToInt32(column.Width.Split("px")[0], CultureInfo.InvariantCulture) : Convert.ToInt32(column.Width, CultureInfo.InvariantCulture) : 0; // (int)this.PixeslToColumnWidth(Convert.ToInt32(column.Width));
                }
            }
        }

        private void ProcessRecordCell(T row, GridColumn column, IDictionary<string, IEnumerable<object>> ForeignKeyData = null!, Cell<object> cell = null!)
        {
            if (column.Type == ColumnType.None)
            {
                ExportHelper<T>.SetColumnType(row!, column, this.gridProp!);
            }

            if (column.Type != ColumnType.CheckBox)
            {
                colIndex++;
                object? value = null;
                // Correctly use the instance member 'PropertyHelper'
                value = !string.IsNullOrEmpty(column.Field) ? PropertyHelper.GetObject(column.Field, row) : value;

                if (!string.IsNullOrEmpty(column.ForeignKeyValue) && (column.GetForeignData() != null || column.ColumnData != null))
                {
                    var data = ForeignKeyData != null && ForeignKeyData.TryGetValue(column.Uid, out IEnumerable<object>? values) ? values : null;
                    if (data == null || !data.Any())
                    {
                        value = null;
                    }
                    else
                    {
                        foreach (var val in data)
                        {
                            // Correctly use the instance member 'PropertyHelper'
                            value = PropertyHelper.GetObject(column.ForeignKeyValue, val);
                        }
                    }
                }

                if (IsNumericColumn(column) && value != null && value != DBNull.Value && value.GetType().Name != "String" && string.IsNullOrEmpty(column.Format))
                {
                    value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    GetCurrentCell.Value = value;
                }

                if (value != null)
                {
                    SetValueByColumnFormat(value, column);
                }
                else
                {
                    GetCurrentCell.Value = string.Empty;
                }
                var currentCell = GetCurrentCell;
                if (GetCurrentCell != null)
                {
                    GetCurrentCell.CellStyle.HAlign = GetTextAlign(column.TextAlign);
                    CopyBorders(GridTableCellType.RecordFieldCell, GetCurrentCell);
                    CopyStyles(GridTableCellType.RecordFieldCell, GetCurrentCell);
                }
                if ((gridProp!.AutoSpan == AutoSpanMode.HorizontalAndVertical || gridProp.AutoSpan == AutoSpanMode.Row || gridProp.AutoSpan == AutoSpanMode.Column) && (!IsCsvExport) && GetCurrentCell != null)
                {
                    if (cell != null)
                    {
                        if (cell.RowSpan > 1)
                        {
                            GetCurrentCell.RowSpan = (int)cell.RowSpan;
                            GetCurrentCell.CellStyle.VAlign = VAlignType.Center;
                        }
                        if (cell.ColSpan > 1)
                        {
                            GetCurrentCell.ColumnSpan = (int)cell.ColSpan;
                            GetCurrentCell.CellStyle.VAlign = VAlignType.Center;
                        }
                    }
                }
                if (cell != null && cell.ColSpan != null && cell.ColSpan > 1)
                {
                    for (int i = 0; i < cell.ColSpan; i++)
                    {
                        int targetCellIndex = (colIndex + i) - 1;
                        if (rowIndex > 0 && targetCellIndex >= 0 && targetCellIndex < Sheet.Rows[rowIndex - 1].Cells.Count)
                        {
                            var targetCell = Sheet.Rows[rowIndex - 1].Cells[targetCellIndex];
                            if (targetCell != null)
                            {
                                CopyBorders(GridTableCellType.RecordFieldCell, targetCell);
                                CopyStyles(GridTableCellType.RecordFieldCell, targetCell);
                            }
                        }
                    }
                    colIndex += (int)cell.ColSpan - 1;
                }

                RaiseExcelQueryCellInfoEvent(row, column, value!);
                if (column.Template != null)
                {
                    RaiseExcelQueryCellInfoEvent(row, column, value!);
                }
            }
        }

        private void RaiseExcelQueryCellInfoEvent(T row, GridColumn column, object value)
        {
            var eventInfo = gridProp!.GridEvents?.ExcelQueryCellInfoEvent;
            if (eventInfo != null || gridProp.IsRenderedFromTreeGrid)
            {
                var eventArgs = new ExcelQueryCellInfoEventArgs<T>()
                {
                    Cell = GetCurrentCell,
                    Column = column,
                    Style = GetCurrentCell.CellStyle,
                    Data = row,
                    Value = value,
                    ColSpan = GetCurrentCell.ColumnSpan,
                    ColumnIndex = colIndex,
                    RowIndex = rowIndex - (isStackedHeaders ? ColumnDepth + 1 : ColumnDepth)
                };
                if(gridProp.IsRenderedFromTreeGrid)
                    gridProp.EventAggregator.NotifyAsync("TreeExcelQueryCellInfo", eventArgs).ConfigureAwait(false);
                else
                    eventInfo!(eventArgs);
            }
        }

        private static bool IsNumericColumn(GridColumn column)
        {
            switch (column.Type)
            {
                case ColumnType.Integer:
                case ColumnType.Double:
                case ColumnType.Long:
                case ColumnType.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        private void SetValueByColumnFormat(object value, GridColumn column, GridAggregateColumn summaryColumn = null!)
        {
            if (!string.IsNullOrEmpty(column.Format))
            {
                string formatstring = column.Format;
                if (summaryColumn != null)
                {
                    column.Format = summaryColumn.Format!;
                }
                if (column.Type != ColumnType.Date && column.Type != ColumnType.DateTime && column.Type != ColumnType.DateOnly && column.Type != ColumnType.TimeOnly)
                {
                    int count = 0;
                    for (count = 0; count < GridColumns!.Count; count++)
                    {
                        if (GridColumns[count].Field == column.Field)
                        {
                            break;
                        }
                    }

                    GetCurrentCell.Value = value;
                    if (IsCsvExport)
                    {
                        Type? valueType = column.ValueType;
                        if (column.Format.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith('d') && valueType != null && value != null)
                        {
                            GetCurrentCell.Value = ExportHelper<T>.FormatDConverstion(column.Format, value, valueType);
                        }
                        else
                        {
                            GetCurrentCell.Value = Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(column.Format, CultureInfo.CurrentCulture);
                        }
                    }
                    else if (rexNumber.Match(column.Format).Length > 0)
                    {
                        GetCurrentCell.CellStyle.NumberFormat = GetNumberFormat(column.Format);
                    }
                    else if (rexStandard.Match(column.Format).Length > 0)
                    {
                        GetCurrentCell.Value = Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(column.Format, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format; // Custom numeric formats
                    }
                }
            }

            if (column.Type == ColumnType.Date)
            {
                if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTimeOffset)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value!).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(column.Format))
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format;
                    }
                }
                else if (column.Format != null)
                {
                    GetCurrentCell.Value = ((DateTime)value!)!.ToString(column.Format, CultureInfo.CurrentCulture);
                    GetCurrentCell.CellStyle.NumberFormat = column.Format;
                }
                else
                {
                    DateTime dt = (DateTime)value!;
                    value = dt.ToString("r") + dt.ToString("zzz", CultureInfo.CurrentCulture) + " (" + TimeZoneInfo.Local.StandardName + ")";
                }
            }

            if (column.Type == ColumnType.DateTime)
            {
                if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTimeOffset)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value!).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTime)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value!).ToString("r") + ((DateTime)value).ToString("zzz", CultureInfo.CurrentCulture) + " (" + TimeZoneInfo.Local.StandardName + ")";
                }
                if (!string.IsNullOrEmpty(column.Format))
                {
                    GetCurrentCell.CellStyle.NumberFormat = column.Format;
                }
            }

            if (column.Type == ColumnType.DateOnly)
            {
                if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTimeOffset)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value!).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(column.Format))
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format;
                    }
                }
                else if (column.ValueType == typeof(DateTime?) || column.ValueType == typeof(DateTime))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTime)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value!).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(column.Format))
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format;
                    }
                }
                else if (column.Format != null)
                {
                    GetCurrentCell.Value = ((DateOnly)value!).ToString(column.Format, CultureInfo.CurrentCulture);
                    GetCurrentCell.CellStyle.NumberFormat = column.Format;
                }
                else
                {
                    DateOnly dateOnly = (DateOnly)value!;
                    GetCurrentCell.Value = dateOnly.ToString(CultureInfo.CurrentCulture);
                }
            }

            if (column.Type == ColumnType.TimeOnly)
            {
                if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTimeOffset)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value!).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(column.Format))
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format;
                    }
                }
                else if (column.ValueType == typeof(DateTime?) || column.ValueType == typeof(DateTime))
                {
                    GetCurrentCell.Value = column.Format != null ? ((DateTime)value!).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value!).ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(column.Format))
                    {
                        GetCurrentCell.CellStyle.NumberFormat = column.Format;
                    }
                }
                else if (column.Format != null)
                {
                    GetCurrentCell.Value = ((TimeOnly)value!).ToString(column.Format, CultureInfo.CurrentCulture);
                    GetCurrentCell.CellStyle.NumberFormat = column.Format;
                }
                else
                {
                    TimeOnly timeOnly = (TimeOnly)value!;
                    GetCurrentCell.Value = timeOnly.ToString(CultureInfo.CurrentCulture);
                }
            }

            if (value != null)
            {
                value = value.ToString() == "True" ? _trueValue : value.ToString() == "False" ? _falseValue : value;
            }

            if (GetCurrentCell != null && (column.Type == ColumnType.String || (  GetCurrentCell.Value == null)))
            {
                GetCurrentCell.Value = value?.ToString();
            }
        }

        /// <summary>
        /// Exports the record row. .
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        protected void ExportRecordRow(string value)
        {
            Sheet.Rows[rowIndex - 1].Cells[colIndex - 1].Value = value;
        }

        private void ProcessColumnHeader(GridColumn column)
        {
            colIndex++;
            ExportColumn = Sheet.Columns.Add();
            ExportColumn.Index = colIndex;
            ExcelGridCell = Sheet.Rows[rowIndex - 1].Cells.Add();
            ExcelGridCell.Index = colIndex;
            string headerText = column.HeaderText ?? column.Field;
            TextAlign hAlign = column.HeaderTextAlign != TextAlign.None ? column.HeaderTextAlign : !GridUtils.IsNoneTextAlign(column) ? column.TextAlign : TextAlign.Left;
            GetCurrentCell.CellStyle.HAlign = GetTextAlign(hAlign);
            ExportRecordRow(headerText);
            var range = GetCurrentCell;
            if (Theme != "none")
            {
                CopyBorders(GridTableCellType.ColumnHeaderCell, range);
                CopyStyles(GridTableCellType.ColumnHeaderCell, range);
            }
            if (Sheet.Rows.Count != 0 && !GetCurrentCell.CellStyle.WrapText && Row !=  null)
            {
                Row.Height = 25;
            }
            bool hasStackedColumns = GridColumns!.Count != gridProp!.Columns?.Count;
            bool isChildLevelColumn = column.Columns == null;//To check the inner columns
            bool hasNoChildren = gridProp.Columns?.Where(c => !c.HasChild).Contains(column) != true;//Filters the grid columns to only those that do NOT have child columns and Checks if the current column exists in that filtered list
            bool triggerHeaderCellInfo = hasStackedColumns ? (isChildLevelColumn ? true : gridProp.Columns?.Any(x => x.Field == column.Field) == true) : true;
            if ((triggerHeaderCellInfo && hasNoChildren && hasStackedColumns) || (!hasStackedColumns && triggerHeaderCellInfo))
            {
                var eventInfo = gridProp.GridEvents?.ExcelHeaderQueryCellInfoEvent;
                if (eventInfo != null || gridProp.IsRenderedFromTreeGrid)
                {
                    var EventArgs = new ExcelHeaderQueryCellInfoEventArgs()
                    {
                        Cell = GetCurrentCell,
                        Column = column,
                        Style = GetCurrentCell.CellStyle,
                        Value = headerText,
                        Colspan = GetCurrentCell.ColumnSpan,
                        ColumnIndex = colIndex,
                        RowIndex = rowIndex
                    };
                    if (gridProp.IsRenderedFromTreeGrid)
                        gridProp.EventAggregator?.NotifyAsync("TreeExcelHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                    else
                        eventInfo!(EventArgs);
                }
            }
        }

        private void GetRowCells(Row Row)
        {
            for (var j = 0; j < TotalVisibleColumnsCount; j++)
            {
                ExcelGridCell = Row.Cells.Add();
                ExcelGridCell.Index = j + 1;
            }
        }

        private static Syncfusion.ExcelExport.HAlignType GetTextAlign(TextAlign textalign)
        {
            switch (textalign)
            {
                case TextAlign.Left:
                case TextAlign.None:
                    return Syncfusion.ExcelExport.HAlignType.Left;
                case TextAlign.Right:
                    return Syncfusion.ExcelExport.HAlignType.Right;
                case TextAlign.Center:
                    return Syncfusion.ExcelExport.HAlignType.Center;
                default:
                    return Syncfusion.ExcelExport.HAlignType.Justify;
            }
        }
        
        /// <summary>
        /// Number format for currency, Number and decimal standard formats.    .
        /// </summary>
        /// <param name="formatString">Column Format string.</param>
        /// <remarks></remarks>
        private string GetNumberFormat(string formatString)
        {
            var cult = Intl.GetCultureInfo();
            string formatDecimalSeparator = rexExpression.Split(formatString)[1];
            NumberFormatInfo numberFormat = cult.NumberFormat;
            int currencyGroupSizes = numberFormat.CurrencyGroupSizes[0] - 1;
            int seperatorCount = !string.IsNullOrEmpty(formatDecimalSeparator) ? Convert.ToInt32(formatDecimalSeparator, CultureInfo.InvariantCulture) : 0;
            string format = string.Empty;

            if (formatString.Contains('c', StringComparison.OrdinalIgnoreCase))
            {
                format = "#" + numberFormat.CurrencyGroupSeparator;
                seperatorCount = seperatorCount == 0 ? numberFormat.CurrencyDecimalDigits : seperatorCount;
            }
            else
            {
                format = "#" + numberFormat.CurrencyGroupSeparator;
                seperatorCount = seperatorCount == 0 ? numberFormat.NumberDecimalDigits : seperatorCount;
            }

            for (int i = 0; i < currencyGroupSizes; i++)
            {
                format += "#";
            }

            format += formatDecimalSeparator == "0" ? "0" : "0" + numberFormat.CurrencyDecimalSeparator;

            if (formatDecimalSeparator != "0")
            {
                for (int d = 0; d < seperatorCount; d++)
                {
                    format += "0";
                } 
            }

            return FormatSymbols(formatString, format);
        }

        private string FormatSymbols(string columnFormat, string numberFormat)
        {
            if (columnFormat.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains('p', StringComparison.Ordinal))
            {
                if (Convert.ToInt64(GetCurrentCell.Value, CultureInfo.InvariantCulture) >= 0)
                {
                    switch (culture!.NumberFormat.PercentPositivePattern)
                    {
                        case 0:
                            numberFormat += " " + culture.NumberFormat.PercentSymbol;
                            break;
                        case 1:
                            numberFormat += culture.NumberFormat.PercentSymbol;
                            break;
                        case 2:
                            numberFormat = culture.NumberFormat.PercentSymbol + numberFormat;
                            break;
                        case 3:
                            numberFormat = culture.NumberFormat.CurrencySymbol + " " + numberFormat;
                            break;
                    }
                }
                else if (Convert.ToInt64(GetCurrentCell.Value, CultureInfo.InvariantCulture) < 0)
                {
                    switch (culture!.NumberFormat.PercentNegativePattern)
                    {
                        case 0:
                            numberFormat = culture.NumberFormat.NegativeSign + numberFormat + " " + culture.NumberFormat.PercentSymbol;
                            break;
                        case 1:
                            numberFormat = culture.NumberFormat.NegativeSign + numberFormat + culture.NumberFormat.PercentSymbol;
                            break;
                        case 2:
                            numberFormat = culture.NumberFormat.NegativeSign + culture.NumberFormat.PercentSymbol + numberFormat;
                            break;
                        case 3:
                            numberFormat = culture.NumberFormat.PercentSymbol + culture.NumberFormat.NegativeSign + numberFormat;
                            break;
                        case 4:
                            numberFormat = culture.NumberFormat.PercentSymbol + numberFormat + culture.NumberFormat.NegativeSign;
                            break;
                        case 5:
                            numberFormat += culture.NumberFormat.NegativeSign + culture.NumberFormat.PercentSymbol;
                            break;
                        case 6:
                            numberFormat += culture.NumberFormat.PercentSymbol + culture.NumberFormat.NegativeSign;
                            break;
                        case 7:
                            numberFormat = culture.NumberFormat.NegativeSign + culture.NumberFormat.PercentSymbol + " " + numberFormat;
                            break;
                        case 8:
                            numberFormat += " " + culture.NumberFormat.PercentSymbol + culture.NumberFormat.NegativeSign;
                            break;
                        case 9:
                            numberFormat = culture.NumberFormat.PercentSymbol + " " + numberFormat + culture.NumberFormat.NegativeSign;
                            break;
                        case 10:
                            numberFormat = culture.NumberFormat.PercentSymbol + " " + culture.NumberFormat.NegativeSign + numberFormat;
                            break;
                        case 11:
                            numberFormat += culture.NumberFormat.NegativeSign + " " + culture.NumberFormat.PercentSymbol;
                            break;
                    }
                }
            }
            if (columnFormat.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains('c', StringComparison.Ordinal))
            {
                if (Convert.ToInt64(GetCurrentCell.Value, CultureInfo.InvariantCulture) >= 0)
                {
                    switch (culture!.NumberFormat.CurrencyPositivePattern)
                    {
                        case 0:
                            numberFormat = culture.NumberFormat.CurrencySymbol + numberFormat;
                            break;
                        case 1:
                            numberFormat += culture.NumberFormat.CurrencySymbol;
                            break;
                        case 2:
                            numberFormat = culture.NumberFormat.CurrencySymbol + " " + numberFormat;
                            break;
                        case 3:
                            numberFormat += "[$" + culture.NumberFormat.CurrencySymbol + "]";
                            break;
                    }
                }
                else if (Convert.ToInt64(GetCurrentCell.Value, CultureInfo.InvariantCulture) < 0)
                {
                    switch (culture!.NumberFormat.CurrencyNegativePattern)
                    {
                        case 0:
                            numberFormat = "(" + culture.NumberFormat.CurrencySymbol + numberFormat + ")";
                            break;
                        case 1:
                            numberFormat = culture.NumberFormat.NegativeSign + culture.NumberFormat.CurrencySymbol + numberFormat;
                            break;
                        case 2:
                            numberFormat = culture.NumberFormat.CurrencySymbol + culture.NumberFormat.NegativeSign + numberFormat;
                            break;
                        case 3:
                            numberFormat = culture.NumberFormat.CurrencySymbol + numberFormat + culture.NumberFormat.NegativeSign;
                            break;
                        case 4:
                            numberFormat = "(" + numberFormat + culture.NumberFormat.CurrencySymbol + ")";
                            break;
                        case 5:
                            numberFormat = culture.NumberFormat.NegativeSign + numberFormat + culture.NumberFormat.CurrencySymbol;
                            break;
                        case 6:
                            numberFormat += culture.NumberFormat.NegativeSign + culture.NumberFormat.CurrencySymbol;
                            break;
                        case 7:
                            numberFormat += culture.NumberFormat.CurrencySymbol + culture.NumberFormat.NegativeSign;
                            break;
                        case 8:
                            numberFormat = culture.NumberFormat.NegativeSign + numberFormat + " " + culture.NumberFormat.CurrencySymbol;
                            break;
                        case 9:
                            numberFormat = culture.NumberFormat.NegativeSign + culture.NumberFormat.CurrencySymbol + " " + numberFormat;
                            break;
                        case 10:
                            numberFormat += " " + culture.NumberFormat.CurrencySymbol + culture.NumberFormat.NegativeSign;
                            break;
                        case 11:
                            numberFormat = culture.NumberFormat.CurrencySymbol + " " + numberFormat + culture.NumberFormat.NegativeSign;
                            break;
                        case 12:
                            numberFormat = culture.NumberFormat.CurrencySymbol + " " + culture.NumberFormat.NegativeSign + numberFormat;
                            break;
                        case 13:
                            numberFormat += culture.NumberFormat.NegativeSign + " " + culture.NumberFormat.CurrencySymbol;
                            break;
                        case 14:
                            numberFormat = "(" + culture.NumberFormat.CurrencySymbol + " " + numberFormat + ")";
                            break;
                        case 15:
                            numberFormat = "(" + numberFormat + " " + culture.NumberFormat.CurrencySymbol + ")";
                            break;
                    }
                }
            }
            return numberFormat;
        }


        /// <summary>
        /// Initializes the excel.  .
        /// </summary>
        /// <param name="book">The book.</param>
        /// <remarks></remarks>
        public void InitializeExcel(Workbook book)
        {
            if (book == null)
            {
                if (ExportProps?.Workbook != null)
                {
                    _workbook = ExportProps.Workbook;
                    int gridSheetindex = (ExportProps.GridSheetIndex < 0 || ExportProps.GridSheetIndex >= _workbook.Worksheets.Count) ? 0 : ExportProps.GridSheetIndex;
                    _sheet = _workbook.Worksheets.Count > 0 ? _workbook.Worksheets[gridSheetindex] : _workbook.Worksheets.Add();
                }
                else
                {
                    _workbook = new Workbook();
                    _sheet = _workbook.Worksheets[0];
                }

                rowIndex = 0;
                colIndex = 0;
                if (ExportProps?.Header != null)
                {
                    var TotalHeaderRowsCount = ExportProps.Header.HeaderRows;
                    for (var i = 0; i < TotalHeaderRowsCount; i++)
                    {
                        rowIndex++;
                        Row = Sheet.Rows.Add();
                        Row.Index = rowIndex;
                        var cellCount = 0;
                        if (ExportProps.Header.Rows != null && ExportProps.Header.Rows.Count != i)
                        {
                            cellCount = ExportProps.Header.Rows[i].Cells?.Count ?? 0;
                        }

                        if (cellCount > 0)
                        {
                            for (var j = 0; j < cellCount; j++)
                            {
                                ExcelGridCell = Row.Cells.Add();
                                ExcelGridCell.Index = j + 1;
                                SetRowCellValues(Row.Cells[j], ExportProps.Header.Rows![i].Cells![j]);
                            }
                        }
                    }
                }
            }
        }

        private static void SetRowCellValues(Cell ExcelCell, ExcelCell PropCell)
        {
            ExcelCell.Value = PropCell.Value;
            ExcelCell.ColumnSpan = (int)PropCell.ColSpan;
            ExcelCell.RowSpan = (int)PropCell.RowSpan;
            if (PropCell.Hyperlink != null)
            {
                ExcelCell.Value = "<a href=" + PropCell.Hyperlink.Target + ">" + (!string.IsNullOrEmpty(PropCell.Hyperlink.DisplayText) ? PropCell.Hyperlink.DisplayText : PropCell.Hyperlink.Target) + " </a>";
            }

            // style part
            if (PropCell.Style != null)
            {
                ExcelCell.CellStyle.FontColor = !string.IsNullOrEmpty(PropCell.Style.FontColor) ? ExportHelper<T>.GetHexValueFromColor(PropCell.Style.FontColor) : "#000000";
                ExcelCell.CellStyle.FontSize = PropCell.Style.FontSize > 0 ? (int)PropCell.Style.FontSize : 10;
                ExcelCell.CellStyle.FontName = PropCell.Style.FontName ?? "Calibri";
                if (PropCell.Style.HAlign != ExcelHorizontalAlign.Left)
                {
                    ExcelCell.CellStyle.HAlign = Enum.Parse<HAlignType>(PropCell.Style.HAlign.ToString());
                }

                if (PropCell.Style.VAlign != ExcelVerticalAlign.Top)
                {
                    ExcelCell.CellStyle.VAlign = Enum.Parse<VAlignType>(PropCell.Style.VAlign.ToString());
                }

                if (!string.IsNullOrEmpty(PropCell.Style.BackColor))
                {
                    ExcelCell.CellStyle.BackColor = ExportHelper<T>.GetHexValueFromColor(PropCell.Style.BackColor);
                }

                ExcelCell.CellStyle.Bold = PropCell.Style.Bold;
                ExcelCell.CellStyle.Underline = PropCell.Style.Underline;
                ExcelCell.CellStyle.Italic = PropCell.Style.Italic;
                ExcelCell.CellStyle.WrapText = PropCell.Style.WrapText;
                ExcelCell.CellStyle.Indent = (int)PropCell.Style.Indent;
            }
        }

        protected void SetFontStyles(Cell range, bool summary)
        {
            if (FontWeight == FontWeight.Bold)
            {
                range.CellStyle.Bold = true;
            }
            else
            {
                range.CellStyle.FontColor = Color.FromArgb(92, 92, 92).ToString();
                if (summary)
                {
                    if (Theme != "none")
                    {
                        CopyBorders(GridTableCellType.RecordFieldCell, range);
                        CopyStyles(GridTableCellType.RecordFieldCell, range);
                    }
                }
                else
                {
                    range.CellStyle.FontSize = 9;
                }
            }
        }

        protected void CopyBorders(GridTableCellType style, Cell range)
        {
            range.CellStyle.Borders.All.LineStyle = LineStyle.Thin;
            switch (style)
            {
                case GridTableCellType.ColumnHeaderCell:
                    if (ExportProps?.Theme?.Header?.Borders != null)
                    {
                        range.CellStyle.Borders.All.LineStyle = ExportProps.Theme.Header.Borders.LineStyle;
                    }
                    if (ExportProps?.Theme?.Header?.Borders?.Color != null)
                    {
                        range.CellStyle.Borders.All.Color = ExportProps.Theme.Header.Borders.Color;
                    }
                    else
                    {
                        range.CellStyle.Borders.All.Color = "#D3D3D3";
                    }
                    break;
                case GridTableCellType.GroupHeaderIndentCell:
                case GridTableCellType.GroupCaptionCell:
                case GridTableCellType.CaptionCell:
                    if (ExportProps?.Theme?.Caption?.Borders != null)
                    {
                        range.CellStyle.Borders.All.LineStyle = ExportProps.Theme.Caption.Borders.LineStyle;
                    }
                    if (ExportProps?.Theme?.Caption?.Borders?.Color != null)
                    {
                        range.CellStyle.Borders.All.Color = ExportProps.Theme.Caption.Borders.Color;
                    }
                    else
                    {
                        range.CellStyle.Borders.All.Color = "#F5F5F5";
                    }
                    break;
                case GridTableCellType.FirstRecord:
                case GridTableCellType.RecordFieldCell:
                case GridTableCellType.IndentCell:
                    if (ExportProps?.Theme?.Record?.Borders != null)
                    {
                        range.CellStyle.Borders.All.LineStyle = ExportProps.Theme.Record.Borders.LineStyle;
                    }
                    if (ExportProps?.Theme?.Record?.Borders?.Color != null)
                    {
                        range.CellStyle.Borders.All.Color = ExportProps.Theme.Record.Borders.Color;
                    }
                    else
                    {
                        range.CellStyle.Borders.All.Color = "#D3D3D3";
                    }
                    break;
            }
        }

        protected void CopyStyles(GridTableCellType style, Cell range)
        {
            if (Theme != "none" && AutoFormat == null)
            {
                AutoFormat = new AutoFormat();
                AutoFormat.SetTheme(AutoFormat, Theme);
            }

            if (Theme != "none" || AutoFormat != null)
            {
                if (AutoFormat?.FontFamily != null)
                {
                    range.CellStyle.FontName = AutoFormat.FontFamily;
                }

                switch (style)
                {
                    case GridTableCellType.RecordFieldCell:
                        this.RecordFieldCellTheme(range);
                        break;
                    case GridTableCellType.ColumnHeaderCell:
                    case GridTableCellType.GroupHeaderIndentCell:
                        this.HeaderCellTheme(range);
                        break;
                    case GridTableCellType.CaptionCell:
                    case GridTableCellType.GroupCaptionCell:
                        this.GroupCaptionTheme(range);
                        break;
                }
            }
        }

        private void RecordFieldCellTheme(Cell range)
        {
            if (ExportProps?.Theme?.Record != null)
            {
                range.CellStyle.FontColor = !string.IsNullOrEmpty(ExportProps.Theme.Record.FontColor) ? ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Record.FontColor) : "#000000";
                range.CellStyle.FontSize = ExportProps.Theme.Record.FontSize > 0 ? (int)ExportProps.Theme.Record.FontSize : 10;
                range.CellStyle.FontName = !string.IsNullOrEmpty(ExportProps.Theme.Record.FontName) ? ExportProps.Theme.Record.FontName : AutoFormat?.FontFamily;
                if (ExportProps.Theme.Record.HAlign != ExcelHorizontalAlign.Left)
                {
                    range.CellStyle.HAlign = Enum.Parse<HAlignType>(ExportProps.Theme.Record.HAlign.ToString());
                }

                range.CellStyle.VAlign = Enum.Parse<VAlignType>(ExportProps.Theme.Record.VAlign.ToString());

                if (!string.IsNullOrEmpty(ExportProps.Theme.Record.BackColor))
                {
                    range.CellStyle.BackColor = ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Record.BackColor);
                }

                range.CellStyle.Bold = ExportProps.Theme.Record.Bold;
                range.CellStyle.Underline = ExportProps.Theme.Record.Underline;
                range.CellStyle.Italic = ExportProps.Theme.Record.Italic;
                range.CellStyle.WrapText = ExportProps.Theme.Record.WrapText;
            }
            else
            {
                range.CellStyle.FontColor = "#000000";
                range.CellStyle.FontSize = 10;
            }
        }

        private void HeaderCellTheme(Cell range)
        {
            if (ExportProps?.Theme?.Header != null)
            {
                range.CellStyle.FontColor = !string.IsNullOrEmpty(ExportProps.Theme.Header.FontColor) ? ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Header.FontColor) : "#000000";
                range.CellStyle.FontSize = ExportProps.Theme.Header.FontSize > 0 ? (int)ExportProps.Theme.Header.FontSize : 10;
                range.CellStyle.FontName = !string.IsNullOrEmpty(ExportProps.Theme.Header.FontName) ? ExportProps.Theme.Header.FontName : AutoFormat?.FontFamily;
                if (ExportProps.Theme.Header.HAlign != ExcelHorizontalAlign.Left)
                {
                    range.CellStyle.HAlign = Enum.Parse<HAlignType>(ExportProps.Theme.Header.HAlign.ToString());
                }

                range.CellStyle.VAlign = Enum.Parse<VAlignType>(ExportProps.Theme.Header.VAlign.ToString());

                if (!string.IsNullOrEmpty(ExportProps.Theme.Header.BackColor))
                {
                    range.CellStyle.BackColor = ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Header.BackColor);
                }

                range.CellStyle.Bold = ExportProps.Theme.Header.Bold;
                range.CellStyle.Underline = ExportProps.Theme.Header.Underline;
                range.CellStyle.Italic = ExportProps.Theme.Header.Italic;
                range.CellStyle.WrapText = ExportProps.Theme.Header.WrapText;
            }
            else
            {
                range.CellStyle.FontColor = "#000000";
                range.CellStyle.FontSize = 9;
                range.CellStyle.Bold = true;
            }
        }

        private void GroupCaptionTheme(Cell range)
        {
            if (ExportProps?.Theme?.Caption != null)
            {
                range.CellStyle.FontColor = !string.IsNullOrEmpty(ExportProps.Theme.Caption.FontColor) ? ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Caption.FontColor) : "#000000";
                range.CellStyle.FontSize = ExportProps.Theme.Caption.FontSize > 0 ? (int)ExportProps.Theme.Caption.FontSize : 10;
                range.CellStyle.FontName = !string.IsNullOrEmpty(ExportProps.Theme.Caption.FontName) ? ExportProps.Theme.Caption.FontName : AutoFormat?.FontFamily;
                if (ExportProps.Theme.Caption.HAlign != ExcelHorizontalAlign.Left)
                {
                    range.CellStyle.HAlign = Enum.Parse<HAlignType>(ExportProps.Theme.Caption.HAlign.ToString());
                }

                range.CellStyle.VAlign = Enum.Parse<VAlignType>(ExportProps.Theme.Caption.VAlign.ToString());

                if (!string.IsNullOrEmpty(ExportProps.Theme.Caption.BackColor))
                {
                    range.CellStyle.BackColor = ExportHelper<T>.GetHexValueFromColor(ExportProps.Theme.Caption.BackColor);
                }

                range.CellStyle.Bold = ExportProps.Theme.Caption.Bold;
                range.CellStyle.Underline = ExportProps.Theme.Caption.Underline;
                range.CellStyle.Italic = ExportProps.Theme.Caption.Italic;
                range.CellStyle.WrapText = ExportProps.Theme.Caption.WrapText;
            }
            else
            {
                range.CellStyle.FontColor = "#000000";
                range.CellStyle.FontSize = 10;
                range.CellStyle.BackColor = "#F5F5F5";
            }
        }

        void IDisposable.Dispose()
        {
            PropertyHelper?.Dispose();
            _workbook?.Dispose();
        }


    }
#pragma warning restore BL0005
}