using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

#region
using Syncfusion.Blazor.Data;
using Syncfusion.PdfExport;
using Syncfusion.Blazor.Internal;
using Syncfusion.ExcelExport;
#endregion

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Helper for grid pdf export.
    /// </summary>
    /// <typeparam name="T">TValue of grid.</typeparam>
    internal class GridPdfExport<T> : IDisposable
    {
        public GridPdfExport()
        {
        }

        private CultureInfo? culture;
        private string _fileName = "Export.pdf";

        private object? HeaderFont { get; set; }

        private object? ContentFont { get; set; }

        private object? CaptionFont { get; set; }
        private int InitColIndex { get; set; }

        private bool _isChildGridInclude;
        private bool IsCellPadding = true;
        private IEnumerable<object>? _dataSource;
        private SfGrid<T>? GridProperty;
        private PropertyInfoHelper<T>? GridPropertyHelper = new PropertyInfoHelper<T>();
        private List<PdfSpannedRow>? SpannedCellIndex = new List<PdfSpannedRow>();

        private PdfExportProperties? PdfExportProps { get; set; }

        private Dictionary<string, object>? ExportAggregate { get; set; }

        private object? _document;
        private ExportType _exportType;
        private DocumentOption _documentOption;
        private PdfVersion _pdfVersion = PdfVersion.Version1_3;
        private bool groupSummary;
        private bool IsTemplateColumnInclude;
        private bool IsCustomCommandColumnInclude;
        private bool _detailrow;
        private bool _unicode;
        private int rowIndex;

        private List<GridColumn>? GridColumns { get; set; }

        private Dictionary<object, Dictionary<object, IEnumerable<object>>> distinctForeignKeyValue = new Dictionary<object, Dictionary<object, IEnumerable<object>>>();
        private AutoFormat? _autoFormat;
        internal float space = 20;
        private string _theme = "default-theme";
        private string _trueValue = "true";
        private string _falseValue = "false";
        public string _emptyText = "No Records to display";

        private int exportColumnCount;
        private int colIndex;
        private int _ColumnIndentWidth = 20;
        internal bool isNewSheet;
        private bool _localSave;
        private PdfGrid? grid;
        private PdfGridRow? row;

        private PdfDocument? _pdfDocument;
        private PdfPage? _targetPage;
        private PdfSection? Section;

        public System.Drawing.RectangleF Bounds { get; set; }

        private PdfFont? fontStyle;

        private int RecordIndex;
        private FontWeight _fontweight;
        private int GroupCurrentIndex;
        private int GroupRecordCount;
        private List<object> dataclone = new List<object>();
        private bool _isHideColumnInclude;

        private bool _isAutoFit { get; set; }

        private bool _isSummaryRow = true;

        private int ColumnDepth { get; set; }

        public Type? SourceType { get; set; }

        private int TotalVisibleColumnsCount;
        private MemoryStream ms = new MemoryStream();

        [Inject]
        [System.Text.Json.Serialization.JsonIgnore]
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
        /// Gets or sets the FontWeight.    .
        /// </summary>
        /// <value>FontWeight.</value>
        /// <remarks></remarks>
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

        public bool Unicode
        {
            get
            {
                return _unicode;
            }

            set
            {
                _unicode = value;
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

        /// <summary>
        /// Gets or sets the AutoFormat of the file.    .
        /// </summary>
        /// <value>The AutoFormat of the file.</value>
        /// <remarks></remarks>
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
        /// Gets or sets the IsSummaryRow included in the file. .
        /// </summary>
        /// <value>IsSummaryRow in file.</value>
        /// <remarks></remarks>
        public bool IsSummaryRow
        {
            get
            {
                return _isSummaryRow;
            }

            set
            {
                _isSummaryRow = value;
            }
        }

        // public Func<IQueryable, SummaryColumn, object> QueryCustomSummaryInfo
        // {
        //    get;

        // set;

        // }
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
        /// Gets or sets the documentoption of the file.    .
        /// </summary>
        /// <value>The documentoption of the file.</value>
        /// <remarks></remarks>
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
        /// Gets or sets the name of the file.  .
        /// </summary>
        /// <value>The name of the file.</value>
        /// <remarks></remarks>
        public ExportType ExportType
        {
            get
            {
                return _exportType;
            }

            set
            {
                _exportType = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.  .
        /// </summary>
        /// <value>The name of the file.</value>
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

        /// <summary>
        /// Gets or sets the Pdf version.   .
        /// </summary>
        /// <value>The Pdf version.</value>
        /// <remarks></remarks>
        public PdfVersion PdfVersion
        {
            get
            {
                return _pdfVersion;
            }

            set
            {
                _pdfVersion = value;
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

        /// <summary>
        /// Gets or sets the RTL check for RTL languages.   .
        /// </summary>
        /// <value>CheckRTL boolean value.</value>
        /// <remarks></remarks>
        public bool CheckRTLText
        {
            get;
            set;
        }

        public PdfDocument PdfDocument
        {
            get
            {
                return _pdfDocument!;
            }

            set
            {
                _pdfDocument = value;
            }
        }

        public PdfPage TargetPage
        {
            get
            {
                return _targetPage!;
            }

            set
            {
                _targetPage = value;
            }
        }

        internal async Task PdfExportHelper(SfGrid<T> GridModel, object dataSource = null!)
        {
            _trueValue = ExportLocalizer!.GetText("Grid_True");
            _falseValue = ExportLocalizer.GetText("Grid_False");
            EmptyText = ExportLocalizer.GetText("Grid_EmptyRecord");
            await ExecuteResult(GridModel, dataSource).ConfigureAwait(true);
        }

        public async Task<MemoryStream> PdfExport(SfGrid<T> GridModel, PdfExportProperties CustomPdfProperties = null!, bool isMemoryStreamExport = false)
        {
            if (GridModel.AllowPdfExport)
            {
                var eventStart = GridModel.GridEvents?.OnPdfExport;
                if (eventStart != null)
                {
                    eventStart("OnPdfExport");
                }
                await GridModel.EventAggregator.NotifyAsync("PdfExport", "OnPdfExport").ConfigureAwait(true);
                GridProperty = GridModel;
                GridColumns = GridUtils.GetColumns(GridModel)?.Clone()!;
                ExportLocalizer = GridModel.Localizer!;
                if (CustomPdfProperties != null)
                {
                    PdfExportProps = CustomPdfProperties;
                    FileName = CustomPdfProperties.FileName ?? FileName;
                    IsHideColumnInclude = PdfExportProps.IncludeHiddenColumn;
                    IsTemplateColumnInclude = PdfExportProps.IncludeTemplateColumn;
                    IsCustomCommandColumnInclude = PdfExportProps.IncludeCommandColumn;
                    if (PdfExportProps.DataSource != null)
                    {
                        if (PdfExportProps.DataSource is SfDataManager && GridProperty.DataManager?.Json == null)
                        {
                            GridProperty.DataManager = (SfDataManager)PdfExportProps.DataSource;
                        }
                        else
                        {
#pragma warning disable BL0005
                            if(GridProperty.DataManager != null)
                            {
                                GridProperty.DataManager.Json = (IEnumerable<object>)PdfExportProps.DataSource;
                            }
                           
                            DataSource = (IEnumerable<object>)PdfExportProps.DataSource;
                        }
                    }
                }
                PdfExportTheme();

                GridModel.EventAggregator.Trigger("ToolbarStateChanged", "PdfExporting");
                if (GridModel.IsRenderedFromTreeGrid)
                {
                    await PdfExportHelper(GridModel, DataSource).ConfigureAwait(true);
                }
                else
                {
                    await Task.Run(() => PdfExportHelper(GridModel, DataSource)).ConfigureAwait(false);
                }
                if (LocalSave)
                {
                    await Task.Run(() => SaveLocal(FilePath!)).ConfigureAwait(false);
                }
                else
                {
                    await Task.Run(() => _pdfDocument?.Save(ms)).ConfigureAwait(false);
                    if (isMemoryStreamExport)
                    {
                        GridModel.EventAggregator.Trigger("ToolbarStateChanged", "PdfExportCompleted");
                        return ms;
                    }
                    else
                    {
                        ms.Position = 0;
                        await (GridProperty?.InvokeMethod("sfBlazor.Grid.exportSave", new object[] { _fileName, Convert.ToBase64String(ms.ToArray()) }))!.ConfigureAwait(true)!;
                        var eventInfo = GridProperty?.GridEvents?.ExportComplete;
                        if (eventInfo != null)
                        {
                            eventInfo("success");
                        }
                    }
                    GridModel.EventAggregator.Trigger("ToolbarStateChanged", "PdfExportCompleted");
                }
            }
            return null!;
        }

        private void PdfExportTheme()
        {
            if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
            {
                if (PdfExportProps?.Theme?.Header != null)
                {
                    HeaderFont = GetFont(PdfExportProps.Theme.Header, (float)10.5);
                }
                else
                {
                    HeaderFont = new PdfStandardFont(PdfFontFamily.Helvetica, (float)10.5);
                }

                if (PdfExportProps?.Theme?.Caption != null)
                {
                    CaptionFont = GetFont(PdfExportProps.Theme.Caption, 10);
                }
                else
                {
                    CaptionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                }

                if (PdfExportProps?.Theme?.Record != null)
                {
                    ContentFont = GetFont(PdfExportProps.Theme.Record, (float)9.75);
                }
                else
                {
                    ContentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                }
            }
        }
        private static void DrawLine(PdfPageTemplateElement Element, PdfHeaderFooterContent Content)
        {
            PdfPen Pen = new PdfPen(new PdfColor(0, 0, 0));
            if (Content?.Style != null)
            {  
                Pen = !string.IsNullOrEmpty(Content.Style.PenColor) ? new PdfPen(ExportHelper<T>.GetDrawingColorFromHexString(Content.Style.PenColor)) : Pen;
                Pen.DashStyle = Enum.Parse<PdfExport.PdfDashStyle>(Content.Style.DashStyle.ToString());
            }
            if (Content != null)
            {
                float X1 = (float)Content.Points!.X1;
                float X2 = (float)Content.Points.X2;
                float Y1 = (float)Content.Points.Y1;
                float Y2 = (float)Content.Points.Y2;

                Element.Graphics?.DrawLine(Pen, X1, Y1, X2, Y2);
            }
        }

        private static void DrawText(PdfPageTemplateElement Element, PdfHeaderFooterContent Content)
        {
            PdfFont Font;
            if (Content?.Font != null && Content.IsTrueType)
            {
                byte[] fontByte = Convert.FromBase64String((string)Content.Font);
                MemoryStream ttf = new MemoryStream(fontByte);
                Font = new PdfTrueTypeFont(ttf, Content.Style?.FontSize != 0 ? (int)Content.Style?.FontSize! : 9);
            }
            else
            {
                PdfFontFamily fontName = !string.IsNullOrEmpty((string)Content?.Font!) ? GetFontFamily((string)Content.Font) : PdfFontFamily.Helvetica;
                Font = new PdfStandardFont(fontName, Content?.Style != null && Content.Style.FontSize != 0 ? (int)Content.Style.FontSize : 9);
            }

            PdfSolidBrush Brush = null!;
            PdfPen Pen = new PdfPen(new PdfColor(0, 0, 0));
            if (Content?.Style != null)
            {
                Pen = !string.IsNullOrEmpty(Content.Style.PenColor) ? new PdfPen(ExportHelper<T>.GetDrawingColorFromHexString(Content.Style.PenColor)) : Pen;
                Pen.DashStyle = Enum.Parse<PdfExport.PdfDashStyle>(Content.Style.DashStyle.ToString());
            }

            if (Content?.Style?.TextPenColor != null)
            {
                Pen = !string.IsNullOrEmpty(Content.Style.TextPenColor) ? new PdfPen(ExportHelper<T>.GetDrawingColorFromHexString(Content.Style.TextPenColor)) : Pen;
            }

            if (Content?.Style?.TextBrushColor != null)
            {
                Brush = new PdfSolidBrush(new PdfColor(ExportHelper<T>.GetDrawingColorFromHexString(Content.Style?.TextBrushColor!)));
                Pen = null!;
            }
            else
            {
                Brush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            }

            string? Value = Content?.Value?.ToString();
            float X = Content?.Position != null ? (float)(Content.Position.X * 0.75) : 0;
            float Y = Content?.Position != null ? (float)(Content.Position.Y * 0.75) : 0;
            PdfStringFormat Format = new PdfStringFormat();
            Format.TextDirection = Content!.TextDirection;
            PageNumberResult Result = SetContentFormat(Content, Format);
            if (Result?.Format != null)
            {
                Element.Graphics?.DrawString(Value, Font, Pen, Brush, new RectangleF(X, Y, Result.Size.Width, Result.Size.Height), Result.Format);
            }
            else
            {
                Element.Graphics?.DrawString(Value, Font, Pen, Brush, new RectangleF(X, Y, Element.Bounds.Width - X, Element.Graphics.ClientSize.Height - Y), Format);
            }
        }

        private static void DrawPageNumber(PdfPageTemplateElement Element, PdfHeaderFooterContent Content)
        {
            PdfFont Font;
            if (Content?.Font != null && Content.IsTrueType)
            {
                byte[] fontByte = Convert.FromBase64String((string)Content.Font);
                MemoryStream ttf = new MemoryStream(fontByte);
                Font = new PdfTrueTypeFont(ttf, Content.Style?.FontSize != 0 ? (int)Content.Style?.FontSize! : 9);
            }
            else
            {
                PdfFontFamily fontName = !string.IsNullOrEmpty((string)Content?.Font!) ? GetFontFamily((string)Content.Font) : PdfFontFamily.TimesRoman;
                Font = new PdfStandardFont(fontName, Content?.Style != null && Content.Style.FontSize != 0 ? (int)Content.Style.FontSize : 9);
            }

            PdfSolidBrush Brush = null!;
            if (Content?.Style?.TextBrushColor != null)
            {
                Brush = new PdfSolidBrush(new PdfColor(ExportHelper<T>.GetDrawingColorFromHexString(Content.Style?.TextBrushColor!)));
            }
            else
            {
                Brush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            }

            PdfPageNumberField PageNumber = new PdfPageNumberField(Font, Brush);
            PageNumber.NumberStyle = GetPageNumberStyle(Content!.PageNumberType);

            PdfCompositeField compositeField = null!;

            string Format = string.Empty;
            if (!string.IsNullOrEmpty(Content?.Format))
            {
                if ((Content.Format).Contains("$total", StringComparison.Ordinal) && (Content.Format).Contains("$current", StringComparison.Ordinal))
                {
                    PdfPageCountField PageCount = new PdfPageCountField(Font);
                    if ((Content.Format).Contains("$total", StringComparison.Ordinal) && !(Content.Format).Contains("$current", StringComparison.Ordinal))
                    {
                        Format = Content.Format.Replace("$current", "0", StringComparison.Ordinal);
                        Format = Format.Replace("$total", "1", StringComparison.Ordinal);
                    }
                    else
                    {
                        Format = Content.Format.Replace("$current", "1", StringComparison.Ordinal);
                        Format = Format.Replace("$total", "0", StringComparison.Ordinal);
                    }

                    compositeField = new PdfCompositeField(Font, Brush, Format, PageNumber, PageCount);
                }
                else if ((Content.Format).Contains("$current", StringComparison.Ordinal) && (Content.Format).Contains("$total", StringComparison.Ordinal))
                {
                    Format = Content.Format.Replace("$current", "0", StringComparison.Ordinal);
                    compositeField = new PdfCompositeField(Font, Brush, Format, PageNumber);
                }
                else
                {
                    PdfPageCountField PageCount = new PdfPageCountField(Font);
                    Format = Content.Format.Replace("$total", "0", StringComparison.Ordinal);
                    compositeField = new PdfCompositeField(Font, Brush, Format, PageCount);
                }
            }
            else
            {
                Format = "{0}";
                compositeField = new PdfCompositeField(Font, Brush, Format, PageNumber);
            }

            float X = (float)(Content!.Position!.X * 0.75);
            float Y = (float)(Content.Position.Y * 0.75);
            PageNumberResult Result = SetContentFormat(Content, compositeField.StringFormat);
            if (Result?.Format != null)
            {
                compositeField.StringFormat = Result.Format;
                compositeField.Bounds = new RectangleF(X, Y, Result.Size.Width, Result.Size.Height);
            }

            compositeField.Draw(Element.Graphics, X, Y);
        }

       private static void DrawImage(PdfPageTemplateElement Element, PdfHeaderFooterContent Content)
        {
            float X = (float)(Content.Position!.X * 0.75);
            float Y = (float)(Content.Position.Y * 0.75);
            float Width = 0;
            float Height = 0;
            if (Content?.Size != null)
            {
                Width = (float)(Content.Size.Width * 0.75);
                Height = (float)(Content.Size.Height * 0.75);
            }

            byte[] Data = Convert.FromBase64String(Content!.Src!);
            MemoryStream stream = new MemoryStream(Data);
            PdfBitmap image = new PdfBitmap(stream);
            if (Width > 0)
            {
                Element.Graphics?.DrawImage(image, X, Y, Width, Height);
            }
            else
            {
                Element.Graphics?.DrawImage(image, X, Y);
            }
            image.Dispose();
        }

        private static PdfPageTemplateElement DrawPageTemplate(PdfPageTemplateElement Element, PdfHeader Header = null!, PdfFooter Footer = null!)
        {
            List<PdfHeaderFooterContent>? Contents = Header?.Contents ?? Footer?.Contents;

            if (Contents != null)
            {
                foreach (PdfHeaderFooterContent Content in Contents)
                {
                    switch (Content.Type)
                    {
                        case ContentType.Line:
                            DrawLine(Element, Content);
                            break;
                        case ContentType.Image:
                            if (string.IsNullOrEmpty(Content.Src))
                            {
                                var src = nameof(Content.Src);
                                throw new ArgumentNullException(src, "Please enter the valid base64 string in image content...");
                            }
                            DrawImage(Element, Content);
                            break;
                        case ContentType.PageNumber:
                            DrawPageNumber(Element, Content);
                            break;
                        case ContentType.Text:
                            var Value = Content.Value;
                            if (Value == null || Value.GetType().Name != "String")
                            {
                                var value = nameof(Value);
                                throw new ArgumentNullException(value, "Please enter the valid input value in text content...");
                            }
                            DrawText(Element, Content);
                            break;
                    }
                }
            }

            return Element;
        }

        public async Task ExecuteResult(SfGrid<T> GridModel, object dataSource = null!)
        {
            culture = Intl.GetCulture();
            exportColumnCount = 0;
            GridProperty = GridModel;
            DataSource = (IEnumerable<object>)dataSource;
            this.UpdateDateFilterValue();
            if (PdfExportProps?.DataSource == null || GridProperty.Aggregates?.Count > 0)
            {
                DataResult ExportData = await ExportHelper<T>.DataProcess(GridProperty, PdfExportProps == null || PdfExportProps.ExportType == ExportType.AllPages).ConfigureAwait(true);
                DataSource = (IEnumerable<object>)(ExportData.Result ?? Enumerable.Empty<object>());
                ExportAggregate = (Dictionary<string, object>)(ExportData.Aggregates ?? new Dictionary<string, object>());
            }
            List<GridColumn> gridcolumns = GridModel.IsFixedColumnPresent() ? GridModel.RearrangeColumns(GridColumns!) : GridColumns!;
            GridColumns = PdfExportProps?.Columns?.Count > 0 ? GridUtils.GetColumns(GridModel, PdfExportProps.Columns).Clone()! : gridcolumns!;
            InitializePdf(_pdfDocument!, HeaderText!);
            int templateColumnCount = GridColumns?.Count(col => col.Template != null && col.Visible) ?? 0;
            int hideColumnCount = GridColumns?.Count(col => !col.Visible && (col.Template == null || !col.Visible && col.Template != null) && col.Type != ColumnType.CheckBox) ?? 0;
            int count = GridColumns?.Count(col => col.Type != ColumnType.CheckBox) ?? 0;
            int columnCount;
            if (IsHideColumnInclude && IsTemplateColumnInclude)
            {
                columnCount = count;
            }
            else if (IsHideColumnInclude)
            {
                columnCount = count - templateColumnCount;
            }
            else if (IsTemplateColumnInclude)
            {
                columnCount = count - hideColumnCount;
            }
            else
            {
                columnCount = count - (templateColumnCount + hideColumnCount);
            }

            if(!IsCustomCommandColumnInclude && IsHideColumnInclude)
            {
                columnCount = columnCount - GridColumns?.Where(col => col.Commands != null).Count() ?? 0;
            }
            else if(!IsCustomCommandColumnInclude)
            {
                columnCount = columnCount - GridColumns?.Where(col => col.Commands != null && col.Visible).Count() ?? 0;
            }

            if (!IsTemplateColumnInclude && IsHideColumnInclude)
            {
                columnCount = columnCount - GridColumns?.Where(col => col.Template != null && !col.Visible).Count() ?? 0;
            }

            TotalVisibleColumnsCount = columnCount;

            if (columnCount > 6 && grid != null)
            {
                grid.Style.AllowHorizontalOverflow = true;
            }

            grid!.Columns.Add(columnCount + (ExportHelper<T>.GetGroupColumnsCount(GridProperty) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty) - 1 : 0));
            IsAutoFit = PdfExportProps?.DisableAutoFitWidth != true;
            grid.Style.AllowHorizontalOverflow = PdfExportProps?.AllowHorizontalOverflow ?? grid.Style.AllowHorizontalOverflow;
            grid.Style.AllowHorizontalOverflow = PdfExportProps?.DisableAutoFitWidth ?? grid.Style.AllowHorizontalOverflow;
            if (!grid.Style.AllowHorizontalOverflow && columnCount > 6)
            {
                IsCellPadding = false;
            }

            ExportHandler();
        }

        private void UpdateDateFilterValue()
        {
            if (GridProperty != null && GridProperty.FilterSettings != null &&  GridProperty.FilterSettings.Columns?.Count > 0)
            {
                for (int j = 0; j < GridProperty.FilterSettings.Columns?.Count; j++)
                {
                    if (GridProperty.FilterSettings.Columns[j].Value != null)
                    {
                        string fieldname = GridProperty.FilterSettings.Columns[j].Field;
                        var column = GridUtils.GetColumnByField(fieldname, GridColumns!);

                        if (column == null && GridColumns!.Any(col => col.IsGridForeignColumn))
                        {
                            var fColsList = ForeignKey<T>.GetForeignKeyColumnsAsync(GridColumns!);
                            column = fColsList.Where(col => col.ForeignKeyValue!.Equals(fieldname, StringComparison.Ordinal)).FirstOrDefault();
                        }
                    }
                }
            }
        }

        //Todo SaveLocal
        public void SaveLocal(string filepath)
        {
            string FileLocation = filepath;
            _pdfDocument?.Save(ms);
        }

        /// <summary>
        /// Initializes the excel.  .
        /// </summary>
        /// <param name="pdfDocument">The Pdf document.</param>
        /// <param name="headerText">The headerText.</param>
        /// <remarks></remarks>
        public void InitializePdf(PdfDocument pdfDocument, string headerText)
        {
            string HeaderText = headerText;
            if (pdfDocument == null)
            {
                _pdfDocument = new PdfDocument();
            }
            else
            {
                _pdfDocument = pdfDocument;
            }

            _pdfDocument.PageSettings.Size = PdfExportProps != null ? GetPageSize(PdfExportProps.PageSize) : new SizeF(595, 842);
            _pdfDocument.PageSettings.Orientation = PdfExportProps?.PageOrientation == PageOrientation.Landscape ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait;
            if (PdfExportProps?.Header?.Contents != null)
            {
                PdfHeader Header = PdfExportProps.Header;
                var Position = new PointF(0, (float)Header.FromTop);
                var Size = new SizeF(_pdfDocument.PageSettings.Width - 80, (float)(Header.Height * 0.75));
                var Bounds = new RectangleF(Position, Size);
                PdfPageTemplateElement HeaderTemplate = new PdfPageTemplateElement(Bounds);
                HeaderTemplate = DrawPageTemplate(HeaderTemplate, Header);
                _pdfDocument.Template.Top = HeaderTemplate;
            }

            if (PdfExportProps?.Footer?.Contents != null)
            {
                PdfFooter Footer = PdfExportProps.Footer;
                var Position = new PointF(0, (float)((_pdfDocument.PageSettings.Width - 80) - (Footer.FromBottom * 0.75)));
                var Size = new SizeF(_pdfDocument.PageSettings.Width - 80, (float)(Footer.Height * 0.75));
                var Bounds = new RectangleF(Position, Size);
                _pdfDocument.Template.Bottom = DrawPageTemplate(new PdfPageTemplateElement(Bounds), null!, Footer);
            }

            grid = new PdfGrid();
            if (Bounds.IsEmpty)
            {
                Section = _pdfDocument.Sections.Add();
                TargetPage = Section.Pages.Add();
            }
            else
            {
                Section = _pdfDocument.Sections.Add();
                TargetPage = Section.Pages.Add();
            }

            if (PdfExportProps != null)
            {
                grid.RepeatHeader = PdfExportProps.IsRepeatHeader;
            }
        }

        private static object GetFont(PdfThemeStyle FontDetails, float Number = 0)
        {
            float FontSize = FontDetails.FontSize > 0 ? (float)FontDetails.FontSize : Number;
            PdfFontStyle FontStyle = PdfFontStyle.Regular;
            PdfGridFont GridFont = FontDetails.Font!;

            if (FontDetails.Italic)
            {
                FontStyle |= PdfFontStyle.Italic;
            }

            if (FontDetails.Bold)
            {
                FontStyle |= PdfFontStyle.Bold;
            }

            if (FontDetails.Strikeout)
            {
                FontStyle |= PdfFontStyle.Strikeout;
            }

            if (FontDetails.Underline)
            {
                FontStyle |= PdfFontStyle.Underline;
            }

            if (GridFont != null && GridFont.IsTrueType)
            {
                FontSize = GridFont.FontSize > 0 ? (float)GridFont.FontSize : FontSize;
                byte[] fontByte = Convert.FromBase64String((string)GridFont.FontFamily!);
                MemoryStream ttf = new MemoryStream(fontByte);
                if (GridFont.FontStyle.HasValue)
                {
                    FontStyle = GridFont.FontStyle.Value;
                }

                return new PdfTrueTypeFont(ttf, FontSize, FontStyle);
            }

            PdfFontFamily FontName = !string.IsNullOrEmpty(FontDetails.FontName) ? GetFontFamily(FontDetails.FontName) : PdfFontFamily.TimesRoman;
            return new PdfStandardFont(FontName, FontSize, FontStyle);
        }

        internal static SizeF GetPageSize(PdfPageSize pageSize)
        {
            switch (pageSize.ToString())
            {
                case "Letter":
                    return new SizeF(612, 792);
                case "Note":
                    return new SizeF(540, 720);
                case "Legal":
                    return new SizeF(612, 1008);
                case "A0":
                    return new SizeF(2380, 3368);
                case "A1":
                    return new SizeF(1684, 2380);
                case "A2":
                    return new SizeF(1190, 1684);
                case "A3":
                    return new SizeF(842, 1190);
                case "A5":
                    return new SizeF(421, 595);
                case "A6":
                    return new SizeF(297, 421);
                case "A7":
                    return new SizeF(210, 297);
                case "A8":
                    return new SizeF(148, 210);
                case "A9":
                    return new SizeF(105, 148);
                default:
                    return GetPageSizeOfBFormat(pageSize);
            }
        }

        private static SizeF GetPageSizeOfBFormat(PdfPageSize pageSize)
        {
            switch (pageSize.ToString())
            {
                case "B0":
                    return new SizeF(2836, 4008);
                case "B1":
                    return new SizeF(2004, 2836);
                case "B2":
                    return new SizeF(1418, 2004);
                case "B3":
                    return new SizeF(1002, 1418);
                case "B4":
                    return new SizeF(709, 1002);
                case "B5":
                    return new SizeF(501, 709);
                case "Archa":
                    return new SizeF(648, 864);
                case "Archb":
                    return new SizeF(864, 1296);
                case "Archc":
                    return new SizeF(1296, 1728);
                case "Archd":
                    return new SizeF(1728, 2592);
                case "Arche":
                    return new SizeF(2592, 3456);
                case "Flsa":
                    return new SizeF(612, 936);
                case "HalfLetter":
                    return new SizeF(396, 612);
                case "Letter11x17":
                    return new SizeF(792, 1224);
                case "Ledger":
                    return new SizeF(1224, 792);
                default:
                    return new SizeF(595, 842);
            }
        }

        private static PdfFontFamily GetFontFamily(string FontName)
        {
            switch (FontName)
            {
                case "TimesRoman":
                    return PdfFontFamily.TimesRoman;
                case "Courier":
                    return PdfFontFamily.Courier;
                case "Symbol":
                    return PdfFontFamily.Symbol;
                case "ZapfDingbats":
                    return PdfFontFamily.ZapfDingbats;
                default:
                    return PdfFontFamily.Helvetica;
            }
        }

        private void ExportHandler()
        {
            rowIndex = 0;
            colIndex = 1;
            IterateElements();
            PdfGridLayoutFormat format = new PdfGridLayoutFormat();
            format.Break = PdfLayoutBreakType.FitElement;
            if (Bounds.IsEmpty)
            {
                grid?.Draw(TargetPage, System.Drawing.PointF.Empty);
            }
            else
            {
                grid?.Draw(TargetPage, Bounds, format);
            }
        }
        
        private void IterateElements()
        {
            if (PdfExportProps == null || (PdfExportProps.IncludeHeaderRow))
            {
                colIndex = InitColIndex;
                if (grid?.Rows.Count == 0)
                {
                    rowIndex = -1;
                }
                List<GridColumn>? gridColumns = (GridProperty != null && GridProperty.IsFixedColumnPresent()) ? GridProperty.RearrangeColumns((List<GridColumn>)GridProperty.Columns!) : ((List<GridColumn>)GridProperty!.Columns!);
                int ColCount = PdfExportProps?.Columns?.Count > 0 ? PdfExportProps.Columns.Count : gridColumns!.Count;
                List<GridColumn> columnsForDepthCalculation = (PdfExportProps?.Columns?.Count > 0 ? (List<GridColumn>)(PdfExportProps.Columns.Where(x => x.Visible)).ToList() : ((List<GridColumn>)GridProperty.Columns!)!);
                ColumnDepth = ExportHelper<T>.MeasureColumnDepth(columnsForDepthCalculation);
                if (GridColumns?.Count != ColCount || ColumnDepth > 0)
                {
                    List<GridColumn> stackedColumns = PdfExportProps?.Columns?.Count > 0 ? PdfExportProps.Columns : ((List<GridColumn>)GridProperty.Columns!)!;
                    stackedColumns = stackedColumns.Where(column => (column.Visible || IsHideColumnInclude) && column.Type != ColumnType.CheckBox && (column.Template == null || IsTemplateColumnInclude) && (column.Commands == null || IsCustomCommandColumnInclude)).ToList();
                    ProcessStackedHeader(stackedColumns);
                }

                ProcessHeaderContent();
            }

            ProcessGridContents();
            if (PdfExportProps != null)
            {
                if (PdfExportProps.BeginCellLayout != null && grid != null)
                {
                    grid.BeginCellLayout += PdfExportProps.BeginCellLayout;
                }
                if (PdfExportProps.EndCellLayout != null && grid != null )
                {
                    grid.EndCellLayout += PdfExportProps.EndCellLayout;
                }
            }
        }
 
        private void ProcessStackedHeader(List<GridColumn> Cols)
        {
            SpannedCellIndex?.Clear();
            var ColDepth = ColumnDepth;
            if (ColDepth > 0)
            {
                for (var i = 0; i < ColDepth; i++)
                {
                    SpannedCellIndex?.Add(new PdfSpannedRow() { RowIndex = i, ColumnIndex = 0 });
                    row = grid!.Headers.Add(1)[SpannedCellIndex!.Count - 1];
                    rowIndex++;
                }
                GroupedIndentBorder();

                foreach (GridColumn Column in Cols)
                {
                    GenerateStackedRows(Column, 1, ColDepth, SpannedCellIndex!);
                }
            }
        }

        private void ApplyHeaderStyles(GridColumn column, int rowIndex, int currentCellIndex, string sHAlign)
        {
            grid!.Headers[rowIndex - 1].Cells[currentCellIndex].Value = column.HeaderText;
            grid.Headers[rowIndex - 1].Cells[currentCellIndex].StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(sHAlign);
            if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
            {
                CopyStyles(GridTableCellType.ColumnHeaderCell, grid.Headers[rowIndex - 1].Cells[currentCellIndex]);
                CopyBorders(GridTableCellType.ColumnHeaderCell, grid.Headers[rowIndex - 1].Cells[currentCellIndex]);
            }
        }

        private void GenerateStackedRows(GridColumn Col, int RowIndex, int ColDepth, List<PdfSpannedRow> SpannedCellIndex, bool isChildLevelStacked = false)
        {
            int CurrentCellIndex = 0;
            string sHAlign = Col.HeaderTextAlign != TextAlign.None ? Col.HeaderTextAlign.ToString() : !GridUtils.IsNoneTextAlign(Col) ? Col.TextAlign.ToString() : "Left";
            int groupCount = ExportHelper<T>.IsGroupingEnabled(GridProperty!) ? (ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 1 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : 0) : 0;
            if (GridProperty != null && GridProperty.GroupSettings != null &&  GridProperty.GroupSettings.Columns != null)
            {
                CurrentCellIndex += groupCount;
            }

            if (Col.Columns == null)
            {
                CurrentCellIndex = ExportHelper<T>.GetCurrentPdfCellIndex(SpannedCellIndex, CurrentCellIndex, RowIndex - 1, colIndex);
                colIndex++;
                row!.Cells[CurrentCellIndex].Value = Col.HeaderText;
                ApplyHeaderStyles(Col, RowIndex, CurrentCellIndex, sHAlign);
                for (var i = 0; i < SpannedCellIndex.Count; i++)
                {
                    SpannedCellIndex[i].SpannedCell = CurrentCellIndex;
                    SpannedCellIndex[i].ColumnIndex = ExportHelper<T>.IsGroupingEnabled(GridProperty!) ? CurrentCellIndex + 1 - groupCount : CurrentCellIndex + 1;
                }

                var eventInfo = GridProperty?.GridEvents?.PdfHeaderQueryCellInfoEvent;
                grid!.Headers[RowIndex - 1].Cells[CurrentCellIndex].RowSpan = isChildLevelStacked ? SpannedCellIndex.Count : SpannedCellIndex.Count + 1;
                if (eventInfo != null || GridProperty!.IsRenderedFromTreeGrid)
                {
                    var EventArgs = new PdfHeaderQueryCellInfoEventArgs()
                    {
                        Cell = row.Cells[CurrentCellIndex],
                        Column = Col,
                        Style = row.Cells[CurrentCellIndex].Style,
                        PdfGridColumn = grid.Columns[CurrentCellIndex],
                        RowIndex = RowIndex,
                        ColumnIndex = CurrentCellIndex + 1
                    };
                    if (GridProperty!.IsRenderedFromTreeGrid)
                    {
                        GridProperty.EventAggregator.NotifyAsync("TreePdfHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                    }
                    else
                    {
                        if (eventInfo != null)
                        {
                            eventInfo(EventArgs);
                        }
                    }
                }

                grid.Headers[RowIndex - 1].Cells[CurrentCellIndex].StringFormat.LineAlignment = PdfVerticalAlignment.Bottom;
                SpannedCellIndex[RowIndex - 1].ColumnIndex = GridProperty?.GroupSettings!.Columns != null ? CurrentCellIndex + 1 - groupCount : CurrentCellIndex + 1;
            }
            else if (Col.Columns.Count > 0)
            {
                int ColSpan = ExportHelper<T>.GetColSpan(Col, 0, GridProperty!);
                CurrentCellIndex = ExportHelper<T>.GetCurrentPdfCellIndex(SpannedCellIndex, CurrentCellIndex, RowIndex - 1, colIndex);
                if (ColSpan > 0)
                {
                    if (GridProperty != null && !GridProperty.Columns!.Any(column => column.Columns?.Count > 0))
                    {
                        row!.Cells[CurrentCellIndex].Value = Col.HeaderText;
                    }
                    ApplyHeaderStyles(Col, RowIndex, CurrentCellIndex, sHAlign);
                    var eventInfo = GridProperty?.GridEvents?.PdfHeaderQueryCellInfoEvent;
                    grid!.Headers[RowIndex - 1].Cells[CurrentCellIndex].ColumnSpan = ColSpan;
                    grid.Headers[RowIndex - 1].Cells[CurrentCellIndex].Value = Col.HeaderText;
                    if (eventInfo != null || GridProperty!.IsRenderedFromTreeGrid)
                    {
                        var EventArgs = new PdfHeaderQueryCellInfoEventArgs()
                        {
                            Cell = row!.Cells[CurrentCellIndex],
                            Column = Col,
                            Style = row.Cells[CurrentCellIndex].Style,
                            PdfGridColumn = grid.Columns[CurrentCellIndex],
                            RowIndex = RowIndex,
                            ColumnIndex = CurrentCellIndex + 1
                        };
                        if (GridProperty!.IsRenderedFromTreeGrid)
                        {
                            GridProperty.EventAggregator.NotifyAsync("TreePdfHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                        }
                        else
                        {
                            if (eventInfo != null)
                            {
                                eventInfo(EventArgs);
                            }
                        }
                    }

                    SpannedCellIndex[RowIndex - 1].ColumnIndex = (GridProperty != null && GridProperty.GroupSettings != null &&  GridProperty.GroupSettings.Columns != null) ? CurrentCellIndex + ColSpan - groupCount : CurrentCellIndex + ColSpan;
                }

                foreach (GridColumn InnerCol in Col.Columns)
                {
                    if (InnerCol.Columns != null)
                    {
                        GenerateStackedRows(InnerCol, RowIndex + 1, --ColDepth, SpannedCellIndex);
                    }
                    else
                    {
                        if (SpannedCellIndex.Count > RowIndex)
                        {
                            GenerateStackedRows(InnerCol, RowIndex + 1, --ColDepth, SpannedCellIndex, true);
                        }
                    }
                }
            }
        }

        private void ProcessHeaderContent()
        {
            rowIndex++;
            colIndex = InitColIndex;
            row = grid!.Headers.Add(1)[SpannedCellIndex!.Count];
            GroupedIndentBorder();

            foreach (var column in GridColumns!)
            {
                bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                bool customCommands = (column.Commands != null && IsCustomCommandColumnInclude) || column.Commands == null;
                bool isTemplateColumn = ((column.Template != null) && IsTemplateColumnInclude) || column.Template == null;
                if (isVisibleColumn && isTemplateColumn && customCommands && column.Type != ColumnType.CheckBox)
                {
                    if (ExportHelper<T>.IsGroupingEnabled(GridProperty!) && ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 && GridProperty != null && GridProperty.GroupSettings != null && !GridProperty.GroupSettings.ShowGroupedColumn && PdfExportProps?.Columns == null)
                    {
                        if (GridProperty.GroupSettings.Columns != null && !GridProperty.GroupSettings.Columns.Contains(column.Field))
                        {
                            exportColumnCount++;
                            ProcessColumnHeader(column);
                        }
                        else
                        {
#pragma warning disable BL0005
                            column.Visible = GridProperty.GroupSettings.ShowGroupedColumn;
                        }
                    }
                    else
                    {
                        exportColumnCount++;
                        ProcessColumnHeader(column);
                    }
                }
            }
        }
        private void ProcessColumnHeader(GridColumn column)
        {
            string headerText = column.HeaderText ?? column.Field;
            colIndex++;
            string hAlign = column.HeaderTextAlign != TextAlign.None ? column.HeaderTextAlign.ToString() : !GridUtils.IsNoneTextAlign(column) ? column.TextAlign.ToString() : "Left";
            row!.Cells[colIndex - 1].StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(hAlign);
            row.Cells[colIndex - 1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

            if (column.Width != null && !IsAutoFit && !column.Width.ToString(culture).Contains('%', StringComparison.Ordinal))
            {
                grid!.Columns[colIndex - 1].Width = column.Width != "auto" ? column.Width.Contains("px", StringComparison.Ordinal) ? (float)Convert.ToInt32(column.Width.Split("px")[0], culture) : (float)Convert.ToInt32(column.Width, culture) : 120;
            }

            ExportRecordRow(headerText, row);
            if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
            {
                CopyStyles(GridTableCellType.ColumnHeaderCell, row.Cells[colIndex - 1]);
                CopyBorders(GridTableCellType.ColumnHeaderCell, row.Cells[colIndex - 1]);
            }
            bool hasStackedColumns = GridColumns!.Count != GridProperty?.Columns?.Count;
            bool isChildLevelColumn = column.Columns == null;//To check the inner columns
            bool hasNoChildren = GridProperty?.Columns?.Where(c => !c.HasChild).Contains(column) != true;//Filters the grid columns to only those that do NOT have child columns and Checks if the current column exists in that filtered list
            bool triggerHeaderCellInfo = hasStackedColumns ? (isChildLevelColumn ? true : GridProperty?.Columns?.Any(x => x.Field == column.Field) == true) : true;
            if ((triggerHeaderCellInfo && hasNoChildren && hasStackedColumns) || (!hasStackedColumns && triggerHeaderCellInfo))
            {
                var eventInfo = GridProperty?.GridEvents?.PdfHeaderQueryCellInfoEvent;
                if (eventInfo != null || GridProperty?.IsRenderedFromTreeGrid == true)
                {
                    var EventArgs = new PdfHeaderQueryCellInfoEventArgs()
                    {
                        Cell = row.Cells[colIndex - 1],
                        Column = column,
                        Style = row.Cells[colIndex - 1].Style,
                        PdfGridColumn = grid!.Columns[colIndex - 1],
                        ColumnIndex = colIndex,
                        RowIndex = rowIndex + 1
                    };
                    if (GridProperty?.IsRenderedFromTreeGrid == true)
                        GridProperty.EventAggregator?.NotifyAsync("TreePdfHeaderQueryCellInfoEvent", EventArgs).ConfigureAwait(false);
                    else
                        eventInfo!(EventArgs);
                }
            }
        }

        private void ProcessIndentCell(GridTableCellType style, PdfGridRow row)
        {
            colIndex++;
            ExportRecordRow(string.Empty, row);
            CopyStyles(style, row.Cells[colIndex - 1]);
            if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
            {
                if (style == GridTableCellType.IndentCell)
                {
                    CopyBorders(style, row.Cells[colIndex - 1]);
                }
                else
                {
                    CopyBorders(GridTableCellType.CaptionCell, row.Cells[colIndex - 1]);
                }
            }
        }

        private void ProcessGridContents()
        {
            rowIndex = -1;
            if (DataSource != null && DataSource.AsQueryable().Any() && ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 && PdfExportProps?.DataSource == null)
            {
                RenderGroupedData((IEnumerable<Group<T>>)DataSource);
                if (GridProperty?.Aggregates != null)
                {
                    ProcessSummaryRow(totalSummary: true);
                }
            }
            else
            {
                RenderRecord();
            }
        }

        /// <summary>
        /// Perform Sorting.    .
        ///  </summary>
        private void RenderGroupedData(IEnumerable<Group<T>> groupedDatasource, int groupLevel = 0)
        {
            foreach (var grouRecord in groupedDatasource)
            {
                RenderGroupedRows(grouRecord, groupLevel);
            }
        }

        private void ProcessSummaryRow(Group<T> GroupData = null!, bool totalSummary = false)
        {
            colIndex = 1;
            foreach (GridAggregate summaryRow in GridProperty!.Aggregates!)
            {
                int summaryColumnTitleIndex = 0;
                rowIndex++;
                row = grid!.Rows.Add();
                foreach (var summaryCol in summaryRow.Columns!)
                {
                    bool isFooterInclude = totalSummary ? (summaryCol.FooterTemplate != null) : (summaryCol.GroupFooterTemplate != null);
                    if (isFooterInclude)
                    {
                        summaryCol.ColumnName = summaryCol.ColumnName ?? summaryCol.Field;
                        var SummaryGridColumn = GridColumns!.FirstOrDefault(F => F.Field == summaryCol.ColumnName);
                        if (SummaryGridColumn == null || (!SummaryGridColumn.Visible && !IsHideColumnInclude))
                        {
                            continue;
                        }
                        summaryCol.ColumnName = summaryCol.ColumnName ?? summaryCol.Field;
                        summaryColumnTitleIndex = this.GetSummaryColumnTitleIndex(summaryColumnTitleIndex, summaryCol);
                        summaryColumnTitleIndex = summaryColumnTitleIndex + (ExportHelper<T>.GetGroupColumnsCount(GridProperty) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty) - 1 : 0);
                        object summaryValue = string.Empty;
                        string prefix = string.Empty;
                        string KeyValue = summaryCol.Field + " " + "-" + " " + summaryCol.Type!.ToString()!.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        string GroupKeyValue = " ";
                        IDictionary<string, object>? SummaryObj = ExportAggregate;
                        if (groupSummary && ExportHelper<T>.IsGroupingEnabled(GridProperty) && GroupData != null)
                        {
                            GroupKeyValue = summaryCol.Field + " " + "-" + " " + summaryCol.Type.ToString();
                            SummaryObj = (IDictionary<string, object>)(GroupData.Aggregates ?? new Dictionary<string, object>());
                        }

                        foreach (var SummaryData in SummaryObj!)
                        {
                            if (SummaryData.Key == KeyValue || SummaryData.Key == GroupKeyValue)
                            {
                                summaryValue = SummaryData.Value == null
                                            ? null!
                                            : Convert.ToDecimal(SummaryData.Value, CultureInfo.CurrentCulture).ToString(summaryCol.Format, CultureInfo.CurrentCulture);
                                break;
                            }
                        }

                        prefix = summaryCol.Type.ToString()!;
                        colIndex = 0;
                        this.ProcessSummaryColumn(summaryCol);

                        if (AutoFormat?.HeaderFontSize != 0)
                        {
                            SetFontStyles(row.Cells[colIndex - 1]);
                        }

                        row.Cells[colIndex - 1].Value = summaryValue == null
                                                    ? null!
                                                    : prefix + " " + summaryValue.ToString()!;
                        row.Cells[colIndex - 1].Style.Font = fontStyle;
                        row.Cells[colIndex - 1].StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(GetTextAlign(SummaryGridColumn.TextAlign).ToString());

                        if (GridProperty.Aggregates != null)
                        {
                            row.Cells[summaryColumnTitleIndex - 1].Value = row.Cells[colIndex - 1].Value;
                            row.Cells[summaryColumnTitleIndex - 1].Style.Font = fontStyle;
                            SetFontStyles(row.Cells[summaryColumnTitleIndex - 1]);
                        }

                        if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                        {
                            foreach (PdfGridCell cell in row.Cells)
                            {
                                CopyStyles(GridTableCellType.CaptionCell, cell);
                                CopyBorders(GridTableCellType.CaptionCell, cell);
                            }
                        }

                        var eventInfo = GridProperty.GridEvents?.PdfAggregateTemplateInfo;
                        if (((summaryCol.FooterTemplate != null || summaryCol.GroupFooterTemplate != null) && eventInfo != null) || GridProperty.IsRenderedFromTreeGrid)
                        {
                            var eventArgs = new PdfAggregateEventArgs()
                            {
                                Cell = row.Cells[colIndex - 1],
                                Column = summaryCol,
                                Value = summaryValue!,
                                Style = row.Cells[colIndex - 1].Style,
                                GroupKey = GroupData?.Key?.ToString() ?? "",
                                AggregateType = totalSummary
                                                ? AggregateTemplateType.Footer
                                                : AggregateTemplateType.GroupFooter
                            };
                            if (GridProperty.IsRenderedFromTreeGrid)
                                GridProperty.EventAggregator?.NotifyAsync("TreePdfAggregateEventArgs", eventArgs).ConfigureAwait(false);
                            else
                                eventInfo!(eventArgs);
                        }
                        else
                        {
                            row.Cells[summaryColumnTitleIndex - 1].Value = prefix + ":" + " " + summaryValue;
                        }
                    }

                    summaryColumnTitleIndex = 0;
                }
            }
        }

        private void ProcessSummaryColumn(GridAggregateColumn summaryCol)
        {
            bool summaryColumnVisible = true;
            if (!string.IsNullOrEmpty(summaryCol.ColumnName))
            {
                foreach (var column in GridColumns!)
                {
                    bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                    if (summaryCol.Field == column.Field && !isVisibleColumn)
                    {
                        summaryColumnVisible = isVisibleColumn;
                        break;
                    }

                    bool customCommands = (column.Commands != null && IsCustomCommandColumnInclude) || column.Commands == null;
                    bool isTemplateColumn = ((column.Template != null) && IsTemplateColumnInclude) || column.Template == null;
                    if (isVisibleColumn && isTemplateColumn && customCommands && column.Type != ColumnType.CheckBox)
                    {
                        colIndex++;
                    }

                    if (column.Field == summaryCol.ColumnName)
                    {
                        break;
                    }
                }

                colIndex = colIndex + (ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : 0);
            }
            else
            {
                colIndex = exportColumnCount + ExportHelper<T>.GetGroupColumnsCount(GridProperty!);
                colIndex = ExportHelper<T>.IsGroupingEnabled(GridProperty!) ? colIndex + 1 : 0;
                while (!string.IsNullOrEmpty(row!.Cells[colIndex - 1].Value?.ToString()))
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

        private void RenderGroupedRows(Group<T> context, int groupLevel)
        {
            var captionSummaryIndex = 0;
            rowIndex++;
            colIndex = 0;
            row = grid!.Rows.Add();
            exportColumnCount = exportColumnCount > 0 ? exportColumnCount : TotalVisibleColumnsCount;
            int groupCount = ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : 0;
            var colspanLength = exportColumnCount + ExportHelper<T>.GetGroupColumnsCount(GridProperty!);
            string groupColumnName = context.Field ?? "";
            var groupCol = GridUtils.GetColumnByField(groupColumnName!, GridProperty?.Columns!);
            bool flag = (groupCol?.Type == ColumnType.Date || groupCol?.Type == ColumnType.DateTime);
            var keyName = context.Key == null ? (flag ? context.Key : string.Empty) : context.Key;
            string? formatstring = groupCol?.Format;
            int count = 0;
            List<int> tempCount = new List<int>();
            if (GridProperty?.Aggregates != null)
            {
                foreach (GridAggregate sumRow in GridProperty.Aggregates)
                {
                    foreach (GridAggregateColumn summaryCol in sumRow.Columns!)
                    {
                        GridColumn? SummaryGridColumn = GridColumns!.FirstOrDefault(e => e.Field == summaryCol.Field);
                        groupSummary = !groupSummary ? summaryCol.GroupFooterTemplate != null : groupSummary;
                        if (summaryCol.GroupCaptionTemplate != null && SummaryGridColumn?.Visible == true)
                        {
                            Tuple<int, int, List<int>> tuple = this.RenderGroupSummaryCaption(summaryCol, SummaryGridColumn, context, count, captionSummaryIndex, tempCount, AggregateTemplateType.GroupCaption);
                            captionSummaryIndex = tuple.Item2;
                            tempCount = tuple.Item3;
                            count = tuple.Item1;
                        }
                    }
                    colspanLength = exportColumnCount + (ExportHelper<T>.GetGroupColumnsCount(GridProperty) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty) - 1 : 0);
                    this.SummaryRowStyleBorders(count, colspanLength);
                    break;
                }
            }

            count = tempCount.Count > 0 ? tempCount.Min() : count;
            colspanLength = exportColumnCount + ExportHelper<T>.GetGroupColumnsCount(GridProperty!);
            colIndex = 0;
            int mergeCount = count != 0 ? count + ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : colspanLength;
            string caption = string.Empty;

            int captionCount = context.CountItems;
            string itemText = captionCount == 1 ? ExportLocalizer!.GetText("Grid_Item") : ExportLocalizer!.GetText("Grid_Items");
            string txt = context.HeaderText ?? (!string.IsNullOrEmpty(groupCol?.HeaderText) ? groupCol.HeaderText : context.Field ?? "");
            string headerText = txt ?? "";
            if (!string.IsNullOrEmpty(groupCol?.ForeignKeyValue) && groupCol?.GetForeignData() != null)
            {
                object foreignColumnData = groupCol.GetForeignData();
                var FData = GridUtils.GetForeignData(groupCol, context, foreignColumnData);
                foreach (var val in (List<object>)FData)
                {
                    keyName = GridPropertyHelper?.GetObject(groupCol.ForeignKeyValue, val);
                }
            }

            string groupKey = headerText.Replace("{{:key}}", Convert.ToString((!string.IsNullOrEmpty(formatstring) ? string.Format(culture, formatstring, keyName) : keyName ?? ""), culture), StringComparison.Ordinal);

            RenderCaption(headerText, context, groupCol!, itemText, groupLevel);

            if (mergeCount > 0)
            {
                mergeCount = mergeCount - (groupLevel + 1);
                if (mergeCount > 0)
                {
                    row.Cells[this.colIndex - 1].ColumnSpan = captionSummaryIndex != 0 ? captionSummaryIndex - (colIndex - 1) : mergeCount;
                }
                else if (mergeCount <= 0)
                {
                    row.Cells[this.colIndex - 1].ColumnSpan = captionSummaryIndex != 0 ? 1 : mergeCount + 1;
                }
            }
            Color CaptionColor = Color.FromArgb(220, 220, 220);
            CaptionColor = this.GetCaptionColor(CaptionColor);
            if (groupCount > 1)
            {
                UpdateTheme();
            }
            captionSummaryIndex = colIndex;
            this.UpdateCaptionTheme(captionSummaryIndex, CaptionColor);
            this.RenderGroupSummaryDataRow(context, groupLevel);
        }

        private Color GetCaptionColor(Color captionColor)
        {
            if (PdfExportProps?.Theme?.Caption?.Border != null && !string.IsNullOrEmpty(PdfExportProps.Theme.Caption.Border.Color))
            {
                captionColor = ExportHelper<T>.GetDrawingColorFromHexString(PdfExportProps.Theme.Caption.Border.Color!);
            }
            return captionColor;
        }

        private void UpdateCaptionTheme(int captionSummaryIndex, Color CaptionColor)
        {
            for (var i = captionSummaryIndex - 1; i < row!.Cells.Count; i++)
            {
                if (i == row.Cells.Count - 1)
                {
                    row.Cells[i].Style.Borders.Left = new PdfPen(new PdfSolidBrush(System.Drawing.Color.Transparent));
                    row.Cells[i].Style.Borders.Right = new PdfPen(new PdfSolidBrush(CaptionColor));
                    row.Cells[i].Style.Borders.Top = new PdfPen(new PdfSolidBrush(CaptionColor));
                    row.Cells[i].Style.Borders.Bottom = new PdfPen(new PdfSolidBrush(CaptionColor));
                }
                else
                {
                    row.Cells[i].Style.Borders.Left = new PdfPen(new PdfSolidBrush(System.Drawing.Color.Transparent));
                    row.Cells[i].Style.Borders.Right = new PdfPen(new PdfSolidBrush(System.Drawing.Color.Transparent));
                    row.Cells[i].Style.Borders.Top = new PdfPen(new PdfSolidBrush(CaptionColor));
                    row.Cells[i].Style.Borders.Bottom = new PdfPen(new PdfSolidBrush(CaptionColor));
                }

                if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                {
                    CopyStyles(GridTableCellType.GroupCaptionCell, grid!.Rows[rowIndex].Cells[i]);
                    CopyBorders(GridTableCellType.GroupCaptionCell, grid.Rows[rowIndex].Cells[colIndex - 1]);
                }
            }
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
                    ProcessRecordRow(record);
                }

                if (GridProperty?.Aggregates != null && groupSummary)
                {
                    ProcessSummaryRow(context);
                }
            }
            else
            {
                groupLevel++;
                RenderGroupedData((context.Items as IEnumerable<Group<T>>)!, groupLevel);
                if (GridProperty?.Aggregates != null && groupSummary)
                {
                    ProcessSummaryRow(context);
                }
            }
        }

        private void UpdateTheme()
        {
            if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
            {
                CopyStyles(GridTableCellType.GroupCaptionCell, grid!.Rows[rowIndex].Cells[colIndex - 1]);
                SetTableOptions(TableOptions.IndentCellWidth, colIndex - 1, _ColumnIndentWidth, row!);
                CopyBorders(GridTableCellType.CaptionCell, grid.Rows[rowIndex].Cells[colIndex - 1]);
            }
            row!.Cells[this.colIndex - 1].Style.Borders.Right = new PdfPen(new PdfSolidBrush(System.Drawing.Color.Transparent));
        }

        private void RenderCaption(string headerText, Group<T> context, GridColumn groupCol, string itemText, int groupLevel)
        {
            string caption = string.Empty;
            int captionCount = context.CountItems;
            for (var i = 0; i < groupLevel + 1; i++)
            {
                if (row != null && row.Cells.Count > i)
                {
                    row.Cells[i].Value = string.Empty;
                    if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                    {
                        CopyStyles(GridTableCellType.GroupCaptionCell, grid!.Rows[rowIndex].Cells[i]);
                        CopyBorders(GridTableCellType.GroupCaptionCell, grid.Rows[rowIndex].Cells[i]);
                    }
                    colIndex++;
                }
            }

            if (GridProperty != null && GridProperty.GroupSettings != null && GridProperty.GroupSettings.CaptionTemplate != null)
            {
                var eventInfo = GridProperty.GridEvents?.PdfGroupCaptionTemplateInfo;
                if (eventInfo == null)
                {
                    caption = $"{headerText}: {context.Key} - {captionCount} {itemText}";
                    ExportRecordRow(caption, row!);
                }
                else
                {
                    var eventArgs = new PdfCaptionTemplateArgs()
                    {
                        Cell = row!.Cells[colIndex - 1],
                        Column = groupCol,
                        Value = $"{headerText}: {context.Key} - {captionCount} {itemText}",
                        Style = row.Cells[colIndex - 1].Style,
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
                        foreach (var val in foreignData)
                        {
                            eventArgs.ForeignKey = GridPropertyHelper?.GetObject(groupCol.ForeignKeyValue!, val)?.ToString()!;
                        }
                    }
                    ExportRecordRow(eventArgs.Value.ToString()!, row);
                    eventInfo(eventArgs);
                }
            }
            else
            {
                var keyname = context.Key;
                var foreignColumnData = groupCol?.GetForeignData();
                if (!string.IsNullOrEmpty(groupCol?.ForeignKeyValue) && foreignColumnData != null)
                {
                    var groupedData = (context as Group<T>)?.Key;
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
                    keyname = GridPropertyHelper?.GetObject(groupCol.ForeignKeyValue, foreginKeyData!);
                }
                caption = $"{headerText}: {keyname} - {captionCount} {itemText}";
                ExportRecordRow(caption, row!);
            }
        }

        private Tuple<int, int, List<int>> RenderGroupSummaryCaption(GridAggregateColumn summaryCol, GridColumn SummaryGridColumn, Group<T> context, int count, int captionSummaryIndex, List<int> tempCount, AggregateTemplateType templateType)
        {
                summaryCol.ColumnName = summaryCol.ColumnName ?? summaryCol.Field;
                count = 0;
                object SummaryValue = string.Empty;
                string Prefix = string.Empty;
                ExportHelper<T>.GetSummaryAndCount(GridColumns!, context, summaryCol, IsHideColumnInclude, IsTemplateColumnInclude, IsCustomCommandColumnInclude, ref SummaryValue, ref count);
                tempCount.Add(count);
                colIndex = count + ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1;
                if (captionSummaryIndex == 0)
                {
                    captionSummaryIndex = colIndex - 1;
                }

                ExportRecordRow(SummaryValue?.ToString()!, row!);
                var eventInfo = GridProperty?.GridEvents?.PdfAggregateTemplateInfo;
                if (eventInfo != null || GridProperty?.IsRenderedFromTreeGrid == true)
                {
                    var eventArgs = new PdfAggregateEventArgs()
                    {
                        Cell = row?.Cells[colIndex - 1],
                        Column = summaryCol,
                        Value = Convert.ToDecimal(SummaryValue, CultureInfo.InvariantCulture).ToString(summaryCol.Format, CultureInfo.CurrentCulture),
                        Style = row?.Cells[colIndex - 1].Style,
                        GroupKey = context.Key?.ToString()!,
                        AggregateType = templateType
                    };
                    if (GridProperty?.IsRenderedFromTreeGrid == true)
                        GridProperty.EventAggregator?.NotifyAsync("TreePdfAggregateEventArgs", eventArgs).ConfigureAwait(false);
                    else
                        eventInfo!(eventArgs);
                }
                else
                {
                    row!.Cells[colIndex - 1].Value = summaryCol.Type + ":" + Convert.ToDecimal(SummaryValue, CultureInfo.InvariantCulture).ToString(summaryCol.Format, CultureInfo.CurrentCulture);
                }
                row!.Cells[colIndex - 1].StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(GetTextAlign(SummaryGridColumn.TextAlign).ToString());
                if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                {
                    CopyStyles(GridTableCellType.ColumnHeaderCell, grid!.Rows[rowIndex].Cells[colIndex - 1]);
                    CopyBorders(GridTableCellType.CaptionCell, row.Cells[colIndex - 1]);
                }
            return new Tuple<int, int, List<int>>(count, captionSummaryIndex, tempCount);
        }

        private void SummaryRowStyleBorders(int count, int colspanLength)
        {
            if (count != 0 && count < colspanLength)
            {
                for (int i = count; i < colspanLength; i++)
                {
                    colIndex = i;
					if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
					{
                         CopyStyles(GridTableCellType.CaptionCell, row!.Cells[colIndex]);
                         CopyBorders(GridTableCellType.GroupCaptionCell, row.Cells[colIndex]);
                     }
                }
            }
        }

        private static TextAlign GetTextAlign(TextAlign textAlign)
        {
            return textAlign == TextAlign.None ? TextAlign.Left : textAlign;
        }

        private void RenderEmptyTableBody()
        {
            rowIndex++;
            this.colIndex = 0;
            colIndex++;
            row = grid!.Rows.Add();
            row.Height = 20f;
            ExportRecordRow(EmptyText, row);
            CopyStyles(GridTableCellType.GroupCaptionCell, grid.Rows[rowIndex].Cells[colIndex - 1]);
            int colspanLength = exportColumnCount + (ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : 0);
            CopyBorders(GridTableCellType.EmptyCell, row.Cells[colIndex]);
            if (colspanLength > 0)
            {
                MergeCells(rowIndex, colIndex - 1, colspanLength - (colIndex - 1));
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

                    if (PdfExportProps?.PdfDetailRowMode == PdfDetailRowMode.Expand && ((IGrid)GridProperty!).GridTemplates?.DetailTemplate != null)
                    {
                        PdfGridRow gridRow = grid!.Rows.Add();
                        PdfGridCell cell = gridRow.Cells[0];
                        cell.ColumnSpan = gridRow.Cells.Count - 0;
                        var eventInfo = GridProperty.GridEvents?.PdfDetailTemplateExporting;
                        if (eventInfo != null)
                        {
                            var eventArgs = new PdfDetailTemplateEventArgs<T>()
                            {
                                ParentRow = new ParentRowInfo<T>() { Data = (T)row, Columns = GridColumns, Index = rowIndex },
                                RowInfo = new PdfDetailTemplateRowSettings() { }
                            };
                            eventInfo(eventArgs);
                            RecordFieldCellBorder(cell, string.Empty, 1);
                            cell.Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
                            cell.Value = ProcessDetailTemplate(eventArgs);
                        }
                    }
                }

                if (GridProperty?.Aggregates != null)
                {
                    ProcessSummaryRow(totalSummary: true);
                }
            }
            else
            {
                RenderEmptyTableBody();
            }
        }

        private object ProcessDetailTemplate(PdfDetailTemplateEventArgs<T> args)
        {
            if (args.RowInfo!.Headers != null || args.RowInfo.Rows != null)
            {
                var pdfGrid = new PdfGrid();
                void ProcessRow(PdfDetailTemplateRow detailTemplateRow, PdfGridRow gridRow, bool isHeader)
                {
                    for (var j = 0; j < detailTemplateRow.Cells?.Count; j++)
                    {
                        var currentCell = detailTemplateRow.Cells[j];
                        var index = currentCell.Index ?? j;
                        var pdfCell = gridRow.Cells[(int)index];
                        if (isHeader)
                        {
                            HeaderBorder(pdfCell, 1);
                            HeaderCellTheme(pdfCell);
                        }
                        else
                        {
                            RecordFieldCellBorder(pdfCell, string.Empty, 1);
                            RecordFieldCellTheme(pdfCell);
                        }
                        if (currentCell.RowSpan > 0)
                        {
                            pdfCell.RowSpan = (int)currentCell.RowSpan;
                        }
                        if (currentCell.ColumnSpan > 0)
                        {
                            pdfCell.ColumnSpan = (int)currentCell.ColumnSpan;
                        }
                        pdfCell.Value = currentCell.CellValue;
                        if (currentCell.Image != null)
                        {
                            pdfCell.Value = currentCell.Image;
                        }
                        if (currentCell.Hyperlink != null)
                        {
                            pdfCell.Value = GridPdfExport<T>.SetHyperLink(currentCell.Hyperlink);
                        }
                        if(currentCell.Style != null)
                        {
                            var FontColor = currentCell.Style.FontColor;
                            pdfCell.Style.BackgroundBrush = new PdfSolidBrush(System.Drawing.Color.White);
                            pdfCell.Style.TextBrush = !string.IsNullOrEmpty(FontColor) ? new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(FontColor)) : (isHeader ? new PdfSolidBrush(new PdfColor(102, 102, 102)) : new PdfSolidBrush(new PdfColor(0, 0, 0)));
                            pdfCell.Style.Font = (PdfFont)GetFont(currentCell.Style, isHeader ? (float)10.5 : (float)9.75);
                        }
                    }
                    if (detailTemplateRow.ChildRowInfo != null)
                    {
                        PdfGridRow nestedrow = pdfGrid.Rows.Add();
                        PdfGridCell cell = nestedrow.Cells[0];
                        cell.ColumnSpan = nestedrow.Cells.Count - 0;
                        CopyStyles(GridTableCellType.RecordFieldCell, cell);
                        CopyBorders(GridTableCellType.RecordFieldCell, cell);
                        cell.Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
                        cell.Value = ProcessDetailTemplate(new PdfDetailTemplateEventArgs<T>()
                        {
                            ParentRow = args.ParentRow,
                            RowInfo = detailTemplateRow.ChildRowInfo
                        });
                    }
                }
                pdfGrid.Columns.Add(GetColumnCount(args.RowInfo));
                if (args.RowInfo.Headers != null)
                {
                    pdfGrid.Headers.Add(args.RowInfo.Headers.Count);
                    for (var i = 0; i < args.RowInfo.Headers.Count; i++)
                    {
                        PdfGridRow gridHeader = pdfGrid.Headers[0];
                        ProcessRow(args.RowInfo.Headers[0], gridHeader, true);
                    }
                }
                if (args.RowInfo.Rows != null)
                {
                    for (var i = 0; i < args.RowInfo.Rows.Count; i++)
                    {
                        ProcessRow(args.RowInfo.Rows[i], pdfGrid.Rows.Add(), false);
                    }
                }
                return pdfGrid;
            }
            else if (args.RowInfo.Image != null)
            {
                return args.RowInfo.Image;
            }
            else if (args.RowInfo.Text != null)
            {
                return args.RowInfo.Text;
            }
            else if (args.RowInfo.Hyperlink != null)
            {
                return GridPdfExport<T>.SetHyperLink(args.RowInfo.Hyperlink);
            }
            return "";
        }

        private static PdfUriAnnotation SetHyperLink(Hyperlink link)
        {
            PdfUriAnnotation uriAnnotation = new PdfUriAnnotation(new RectangleF(0, 0, 100, 100), "mailto:" + link.Target?.ToString());
            uriAnnotation.Text = link.DisplayText ?? link.Target;
            return uriAnnotation;
        }

        private static int GetColumnCount(PdfDetailTemplateRowSettings args)
        {
            var count = args?.ColumnCount ?? 0;
            if (args?.Headers != null && args.Headers[0].Cells != null)
            {
                var headerCount = args.Headers[0].Cells!.Count;
                return count > headerCount ? count : headerCount;
            }
            if (args?.Rows != null && args.Rows[0].Cells != null)
            {
                var rowsCount = args.Rows[0].Cells!.Count;
                return count > rowsCount ? count : rowsCount;
            }
            return count;
        }


        /// <summary>
        /// Sets the table options. .
        /// </summary>
        /// <param name="tableOptions">The table options.</param>
        /// <param name="Idx">The idx.</param>
        /// <param name="value">The value.</param>
        /// <param name="row"></param>
        /// <remarks></remarks>
        protected void SetTableOptions(TableOptions tableOptions, int Idx, int value, PdfGridRow row)
        {
            switch (tableOptions)
            {
                case TableOptions.ColumnHeaderRowHeight:
                case TableOptions.CaptionRowHeight:
                    row.Height = value;
                    break;
                case TableOptions.RecordRowHeight:
                    row.Height = value;
                    break;
                case TableOptions.IndentCellWidth:
                    if (Idx > 0 && grid!.Columns.Count > Idx)
                    {
                        grid.Columns[Idx - 1].Width = value;
                    }
                    break;
            }
        }

        /// <summary>
        /// Exports the record row. .
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The record row to be exported.</param>
        /// <remarks></remarks>
        protected void ExportRecordRow(string value, PdfGridRow row)
        {
            if (value != null)
            {
                row.Cells[colIndex - 1].Value = value.ToString();
            }
        }
        /// <summary>
        /// Exports the record row. .
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The record row to be exported.</param>
        /// <remarks></remarks>
        protected void ExportCaptionRow(string value, PdfGridRow row)
        {
            if (value != null)
            {
                row.Cells[colIndex].Value = value.ToString();
            }
        }

        private void MergeCells(int rowIdx, int colIdx, int lastcolIdx)
        {
            grid!.Rows[rowIdx].Cells[colIdx].ColumnSpan = lastcolIdx;
        }

        private int ProcessRecordRow(T data)
        {
            rowIndex++;
            RecordIndex++;
            this.colIndex = 0; //this.initColIndex never assigned
            row = grid!.Rows.Add();
            int groupCount = (ExportHelper<T>.GetGroupColumnsCount(GridProperty!) > 0 ? ExportHelper<T>.GetGroupColumnsCount(GridProperty!) - 1 : 0);
            for (int i = 0; i < groupCount; i++)
            {
                ProcessIndentCell(GridTableCellType.IndentCell, row);
            }
            if ((GridProperty?.AutoSpan == AutoSpanMode.HorizontalAndVertical || GridProperty?.AutoSpan == AutoSpanMode.Column || GridProperty?.AutoSpan == AutoSpanMode.Row))
            {
                var gridRowData = GridProperty.Rows?.FirstOrDefault(r => r.Data is T && r.Data.Equals(data));
                if (gridRowData != null)
                {
                    var dataCells = gridRowData.Cells.Where(c => c.CellType == CellType.Data).ToList();

                    foreach (var cell in dataCells)
                    {
                        var column = cell.Column;
                        if (column == null) { continue; }

                        bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                        if (GridProperty != null && GridProperty.GroupSettings != null && GridProperty.GroupSettings.Columns != null && column.Visible && GridProperty.GroupSettings.Columns.Contains(column.Field))
                        {
                            isVisibleColumn = GridProperty.GroupSettings.ShowGroupedColumn;
                        }

                        bool customCommands = (column.Commands != null && IsCustomCommandColumnInclude) || column.Commands == null;
                        bool isTemplateColumn = (column.Template != null && IsTemplateColumnInclude) || column.Template == null;
                        if ((isVisibleColumn && isTemplateColumn && customCommands) || (PdfExportProps?.Columns != null && isVisibleColumn))
                        {
                            ProcessRecord(data, cell, row, gridRowData.ForeignKeyData!);
                        }
                    }
                }
            }
            else
            {
                Row<object> dataRow = new Row<object>()
                {
                    ForeignKeyData = new Dictionary<string, IEnumerable<object>>()
                };
                ForeignKey<T>.FetchForeignKeyRow(dataRow, data!, GridColumns!, distinctForeignKeyValue);

                foreach (GridColumn column in GridColumns!)
                {
                    bool isVisibleColumn = column.Visible || IsHideColumnInclude;
                    if (GridProperty != null && GridProperty.GroupSettings != null && GridProperty.GroupSettings.Columns != null && column.Visible && GridProperty.GroupSettings.Columns.Contains(column.Field))
                    {
#pragma warning disable BL0005
                        isVisibleColumn = GridProperty.GroupSettings.ShowGroupedColumn;
                    }

                    bool customCommands = (column.Commands != null && IsCustomCommandColumnInclude) || column.Commands == null;
                    bool isTemplateColumn = (column.Template != null && IsTemplateColumnInclude) || column.Template == null;
                    if ((isVisibleColumn && isTemplateColumn && customCommands) || (PdfExportProps?.Columns != null))
                    {
                        ProcessRecordCell(data, column, row, dataRow.ForeignKeyData);
                    }
                }
            }
                return rowIndex;
        }
        private void ProcessRecordCell(T data, GridColumn column, PdfGridRow row, IDictionary<string, IEnumerable<object>> ForeignKeyData = null!)
        {
            if (column.Type == ColumnType.None)
            {
                ExportHelper<T>.SetColumnType(data!, column, GridProperty!);
            }

            if (column.Type != ColumnType.CheckBox)
            {
                colIndex++;
                object value = null!;
                int dataInx = dataclone.IndexOf(data!) + 1;
                value = !string.IsNullOrEmpty(column.Field) ? GridPropertyHelper?.GetObject(column.Field, data)! : value!;

                if (!string.IsNullOrEmpty(column.ForeignKeyValue) && (column.GetForeignData() != null || column.ColumnData != null))
                {
                    var FData = ForeignKeyData != null && ForeignKeyData.TryGetValue(column.Uid, out IEnumerable<object>? values) ? values : null!;
                    if (FData == null || (FData as List<object>)!.Count == 0)
                    {
                        value = null!;
                    }

                    if (FData != null)
                    {
                        foreach (var val in (List<object>)FData)
                        {
                            value = GridPropertyHelper?.GetObject(column.ForeignKeyValue!, val)!;
                        }
                    }
                }

                if (IsNumericColumn(column) && value != null && value != DBNull.Value && value.GetType().Name != "String")
                {
                    value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }

                var currentCell = row.Cells[colIndex - 1];
                if (value != null)
                {
                    this.SetValueByColumnFormat(value, column);
                }
                else
                {
                    currentCell.Value = string.Empty;
                }
                currentCell.StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(GetTextAlign(column.TextAlign).ToString());
                if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                {
                    CopyStyles(GridTableCellType.RecordFieldCell, currentCell);
                    CopyBorders(GridTableCellType.RecordFieldCell, currentCell);
                }

                var eventInfo = GridProperty?.GridEvents?.PdfQueryCellInfoEvent;
                RaisePdfQueryCellInfoEvent(row, column, data, currentCell, eventInfo!);

                if (column.Template != null)
                {
                    RaisePdfQueryCellInfoEvent(row, column, data, currentCell, eventInfo!);
                }
            }
        }

        private void ProcessRecord(T data, Cell<object> cell, PdfGridRow row, IDictionary<string, IEnumerable<object>> ForeignKeyData = null!)
        {
            var column = cell.Column;
            if (column != null && column.Type == ColumnType.None)
            {
                ExportHelper<T>.SetColumnType(data!, column, GridProperty!);
            }

            if (column != null &&  column.Type != ColumnType.CheckBox)
            {
                colIndex++;
               
                
                object value = null!;
                int dataInx = dataclone.IndexOf(data!) + 1;
                value = !string.IsNullOrEmpty(column.Field) ? GridPropertyHelper?.GetObject(column.Field, data)! : value!;

                if (!string.IsNullOrEmpty(column.ForeignKeyValue) && (column.GetForeignData() != null || column.ColumnData != null))
                {
                    var FData = ForeignKeyData != null && ForeignKeyData.TryGetValue(column.Uid, out IEnumerable<object>? values) ? values : null!;
                    if (FData == null || (FData as List<object>)!.Count == 0)
                    {
                        value = null!;
                    }

                    if (FData != null)
                    {
                        foreach (var val in (List<object>)FData)
                        {
                            value = GridPropertyHelper?.GetObject(column.ForeignKeyValue!, val)!;
                        }
                    }
                }

                if (IsNumericColumn(column) && value != null && value != DBNull.Value && value.GetType().Name != "String")
                {
                    value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }
                var currentCell = row.Cells[colIndex - 1];
                if (cell.IsRowSpanned || cell.IsSpanned)
                {
                    currentCell.Style.Borders.All = new PdfPen(System.Drawing.Color.Transparent);
                    currentCell.Style.BackgroundBrush = new PdfSolidBrush(System.Drawing.Color.Transparent);
                    return;
                }
                if (cell.IsSpanned)
                {
                    currentCell.Style.Borders.All = new PdfPen(System.Drawing.Color.Transparent);
                    currentCell.Style.BackgroundBrush = new PdfSolidBrush(System.Drawing.Color.Transparent);
                    return;
                }
                if ((GridProperty?.AutoSpan == AutoSpanMode.HorizontalAndVertical || GridProperty?.AutoSpan == AutoSpanMode.Column || GridProperty?.AutoSpan == AutoSpanMode.Row))
                {
                    bool isSpannedCell = cell?.RowSpan > 1 || (cell?.ColSpan.HasValue == true && cell.ColSpan > 1);
                    if (cell?.RowSpan > 1)
                    {
                        currentCell.RowSpan = (int)cell.RowSpan;
                    }

                    if (cell != null && cell.ColSpan.HasValue && cell.ColSpan > 1)
                    {
                        currentCell.ColumnSpan = cell.ColSpan.Value;
                    }

                    if (isSpannedCell)
                    {
                        currentCell.StringFormat.Alignment = PdfTextAlignment.Center;
                        currentCell.StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    }
                }
                if (value != null)
                {
                    this.SetValueByColumnFormat(value, column);
                }
                else
                {
                    currentCell.Value = string.Empty;
                }
                currentCell.StringFormat.Alignment = Enum.Parse<PdfTextAlignment>(GetTextAlign(column.TextAlign).ToString());
                if (PdfExportProps == null || PdfExportProps.IsThemeEnabled)
                {
                    CopyStyles(GridTableCellType.RecordFieldCell, currentCell);
                    CopyBorders(GridTableCellType.RecordFieldCell, currentCell);
                }

                var eventInfo = GridProperty?.GridEvents?.PdfQueryCellInfoEvent;
                RaisePdfQueryCellInfoEvent(row, column, data, currentCell, eventInfo!);

                if (column.Template != null)
                {
                    RaisePdfQueryCellInfoEvent(row, column, data, currentCell, eventInfo!);
                }
            }
        }

        private void RaisePdfQueryCellInfoEvent(PdfGridRow row, GridColumn column, T data, PdfGridCell currentCell, Action<PdfQueryCellInfoEventArgs<T>> eventInfo)
        {
            if (eventInfo != null || (GridProperty != null && GridProperty.IsRenderedFromTreeGrid))
            {
                var EventArgs = new PdfQueryCellInfoEventArgs<T>()
                {
                    Cell = currentCell,
                    ColSpan = currentCell.ColumnSpan,
                    Column = column,
                    Data = data,
                    Value = currentCell.Value,
                    Style = currentCell.Style,
                    Row = row,
                    ColumnIndex = colIndex,
                    RowIndex = rowIndex + 1
                };
                if (GridProperty != null && GridProperty.IsRenderedFromTreeGrid)
                    GridProperty.EventAggregator.NotifyAsync("TreePdfQueryCellInfo", EventArgs).ConfigureAwait(false);
                else
                    eventInfo!(EventArgs);
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

        private void SetValueByColumnFormat(object value, GridColumn column)
        {
            if (!string.IsNullOrEmpty(column.Format) && (column.Type == ColumnType.Integer || column.Type == ColumnType.Double || column.Type == ColumnType.Long || column.Type == ColumnType.Decimal))
            {
                Type? valueType = column.ValueType;
                if (column.Format.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith('d') && valueType != null && value != null)
                {
                    row!.Cells[colIndex - 1].Value = ExportHelper<T>.FormatDConverstion(column.Format, value, column.ValueType);
                }
                else
                {
                    row!.Cells[colIndex - 1].Value = value != null ? Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(column.Format, CultureInfo.CurrentCulture) : string.Empty;
                }
            }
            else
            {
                if (column.Type == ColumnType.Date)
                {
                    if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTimeOffset)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (column.Format != null)
                    {
                        row!.Cells[colIndex - 1].Value = ((DateTime)value).ToString(column.Format, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        DateTime dt = (DateTime)value;
                        value = dt.ToString("r") + dt.ToString("zzz", CultureInfo.CurrentCulture) + " (" + TimeZoneInfo.Local.StandardName + ")";
                        row!.Cells[colIndex - 1].Value = value;
                    }
                }
                else if (column != null && column.Type == ColumnType.DateTime)
                {
                    if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTimeOffset)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTime)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                    }
                } 
                else if (column != null && column.Type == ColumnType.DateOnly)
                {
                    if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTimeOffset)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (column.ValueType == typeof(DateTime?) || column.ValueType == typeof(DateTime))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTime)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (column.Format != null)
                    {
                        row!.Cells[colIndex - 1].Value = ((DateOnly)value).ToString(column.Format, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        DateOnly dateOnly = (DateOnly)value;
                        row!.Cells[colIndex - 1].Value = dateOnly.ToString(CultureInfo.CurrentCulture);
                    }
                }
                else if (column != null && column.Type == ColumnType.TimeOnly)
                {
                    if (column.ValueType == typeof(DateTimeOffset?) || column.ValueType == typeof(DateTimeOffset))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTimeOffset)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTimeOffset)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (column.ValueType == typeof(DateTime?) || column.ValueType == typeof(DateTime))
                    {
                        row!.Cells[colIndex - 1].Value = column.Format != null ? ((DateTime)value).ToString(column.Format, CultureInfo.CurrentCulture) : ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (column.Format != null)
                    {
                        row!.Cells[colIndex - 1].Value = ((TimeOnly)value).ToString(column.Format, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        TimeOnly timeOnly = (TimeOnly)value;
                        row!.Cells[colIndex - 1].Value = timeOnly.ToString(CultureInfo.CurrentCulture);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        value = value.ToString() == "True" ? _trueValue : value.ToString() == "False" ? _falseValue : value;
                    }

                    row!.Cells[colIndex - 1].Value = value?.ToString() ?? string.Empty;
                }
            }

            if ((GridProperty != null && GridProperty.AllowTextWrap) || IsAutoFit)
            {
                row.Cells[colIndex - 1].StringFormat.WordWrap = PdfWordWrapType.Word;
                row.Cells[colIndex - 1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
            }
        }

        protected void CopyStyles(GridTableCellType style, PdfGridCell cell)
        {
            if (Theme != "none" && AutoFormat == null)
            {
                AutoFormat = new AutoFormat();
                AutoFormat.SetTheme(AutoFormat, Theme);
            }

            if (Theme != "none" || AutoFormat != null)
            {
                switch (style)
                {
                    case GridTableCellType.RecordFieldCell:
                        RecordFieldCellTheme(cell);
                        break;
                    case GridTableCellType.ColumnHeaderCell:
                    case GridTableCellType.GroupHeaderIndentCell:
                        HeaderCellTheme(cell);
                        break;
                    case GridTableCellType.CaptionCell:
                    case GridTableCellType.GroupCaptionCell:
                        GroupCaptionTheme(cell);
                        break;
                    case GridTableCellType.CaptionSummary:
                        cell.Style.BackgroundBrush = new PdfSolidBrush(AutoFormat.CaptionBackColor);
                        fontStyle = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                        // TODO CaptionSummary
                        // fontStyle = new PdfTrueTypeFont(new Font(this._fileFont, AutoFormat.ContentFontSize), 10);
                        cell.Style.Font = fontStyle;
                        break;
                }
            }
            // TODO RTL
            // cell.StringFormat. = this.CheckRTLText ? this.IsRTLText((cell.Value ?? "").ToString()) : this.GridProperty.EnableRtl;
        }

        private void RecordFieldCellTheme(PdfGridCell cell)
        {
            var ContentFontColor = PdfExportProps?.Theme?.Record?.FontColor;
            bool IsRecordTrueTypeFont = PdfExportProps?.Theme?.Record?.Font?.IsTrueType ?? false;
            if (IsCellPadding)
            {
                cell.Style.CellPadding = new PdfPaddings(5, 5, 2, 2);
            }

            cell.Style.BackgroundBrush = new PdfSolidBrush(System.Drawing.Color.White);
            cell.Style.TextBrush = !string.IsNullOrEmpty(ContentFontColor) ? new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(ContentFontColor)) : new PdfSolidBrush(new PdfColor(0, 0, 0));
            cell.Style.Font = IsRecordTrueTypeFont ? (PdfTrueTypeFont)ContentFont! : (PdfFont)ContentFont!;
        }

        private void HeaderCellTheme(PdfGridCell cell)
        {
            var HeaderFontColor = PdfExportProps?.Theme?.Header?.FontColor;
            bool IsHeaderTrueTypeFont = PdfExportProps?.Theme?.Header?.Font?.IsTrueType ?? false;
            if (IsCellPadding)
            {
                cell.Style.CellPadding = new PdfPaddings(5, 5, 2, 2);
            }

            cell.Style.BackgroundBrush = new PdfSolidBrush(System.Drawing.Color.White);
            cell.Style.TextBrush = !string.IsNullOrEmpty(HeaderFontColor) ? new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(HeaderFontColor)) : new PdfSolidBrush(new PdfColor(102, 102, 102));
            cell.Style.Font = IsHeaderTrueTypeFont ? (PdfTrueTypeFont)HeaderFont! : (PdfFont)HeaderFont!;
        }

        private void GroupCaptionTheme(PdfGridCell cell)
        {
            bool hasPdfGroupCaptionTemplateInfo = GridProperty?.GridEvents?.PdfGroupCaptionTemplateInfo != null;
            var fontColor = PdfExportProps?.Theme?.Caption?.FontColor;
            bool IsCaptionTrueTypeFont = PdfExportProps?.Theme?.Caption?.Font?.IsTrueType ?? false;
            cell.Style.TextBrush = !string.IsNullOrEmpty(fontColor) ? new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(fontColor)) : hasPdfGroupCaptionTemplateInfo ? cell.Style.TextBrush : new PdfSolidBrush(new PdfColor(0, 0, 0));
            cell.Style.BackgroundBrush = cell.Style.BackgroundBrush ?? new PdfSolidBrush(AutoFormat.AltRowBackColor);
            cell.Style.Font = IsCaptionTrueTypeFont ? (PdfTrueTypeFont)CaptionFont! : (PdfFont)CaptionFont!;
            cell.Style.CellPadding = (IsCellPadding && cell.Style.CellPadding == null) ? new PdfPaddings(5, 5, 2, 2) : cell.Style.CellPadding;
        }

        private void GroupedIndentBorder()
        {
            if (ExportHelper<T>.IsGroupingEnabled(GridProperty!))
            {
                var groupingLimit = GridProperty?.GroupSettings?.Columns!.Length > 0 ? GridProperty?.GroupSettings?.Columns!.Length - 1 : 0;

                for (int i = 0; i < groupingLimit; i++)
                {
                    ProcessIndentCell(GridTableCellType.GroupHeaderIndentCell, row!);
                    SetTableOptions(TableOptions.IndentCellWidth, colIndex, _ColumnIndentWidth, row!);
                }
            }
        }

        protected void SetFontStyles(PdfGridCell cell)
        {
            if (FontWeight == FontWeight.Bold)
            {
                fontStyle = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            }
            else
            {
                fontStyle = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                cell.Style.TextBrush = new PdfSolidBrush(AutoFormat.GroupContentFontColor);
            }
        }

        /// <summary>
        /// Copies the borders. .
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="cell">The cell.</param>
        /// <remarks></remarks>
        protected void CopyBorders(GridTableCellType style, PdfGridCell cell)
        {
            float lineWidth = 1;
            string border = string.Empty;
            switch (style)
            {
                case GridTableCellType.ColumnHeaderCell:
                    HeaderBorder(cell, lineWidth);
                    break;
                case GridTableCellType.GroupCaptionCell:
                case GridTableCellType.CaptionCell:
                    GroupCaptionBorder(cell, lineWidth);
                    break;
                case GridTableCellType.RecordFieldCell:
                    RecordFieldCellBorder(cell, border, lineWidth);
                    break;
                case GridTableCellType.GroupHeaderIndentCell:
                case GridTableCellType.IndentCell:
                    PdfPen IndentCellpen = new PdfPen(new PdfColor(220, 220, 220), lineWidth);
                    cell.Style.Borders.Bottom = IndentCellpen;
                    cell.Style.Borders.Left = IndentCellpen;
                    cell.Style.Borders.Right = IndentCellpen;
                    cell.Style.Borders.Top = IndentCellpen;
                    break;
                case GridTableCellType.FirstRecord:
                    cell.Style.Borders.Top = new PdfPen(new PdfSolidBrush(AutoFormat.ContentBorderColor), lineWidth);
                    cell.Style.Borders.Bottom = new PdfPen(new PdfSolidBrush(AutoFormat.ContentBorderColor), lineWidth);
                    cell.Style.Borders.Left = new PdfPen(new PdfSolidBrush(AutoFormat.ContentBorderColor), lineWidth);
                    cell.Style.Borders.Right = new PdfPen(new PdfSolidBrush(AutoFormat.ContentBorderColor), lineWidth);
                    break;
            }
        }
        private void HeaderBorder(PdfGridCell cell, float lineWidth)
        {
            if (PdfExportProps?.Theme?.Header?.Border != null)
            {
                var Headerborder = PdfExportProps?.Theme.Header.Border.Color;
                lineWidth = PdfExportProps?.Theme.Header.Border.Width > 0 ? (float)PdfExportProps?.Theme.Header.Border.Width! : lineWidth;
                PdfPen Headerpen = !string.IsNullOrEmpty(Headerborder) ? new PdfPen(new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(Headerborder)), lineWidth) : new PdfPen(new PdfSolidBrush(AutoFormat.HeaderBorderColor), lineWidth);
                cell.Style.Borders.Bottom = Headerpen;
                cell.Style.Borders.Left = Headerpen;
                cell.Style.Borders.Right = Headerpen;
                cell.Style.Borders.Top = Headerpen;
            }
            else
            {
                PdfPen Headerpen = new PdfPen(new PdfColor(220, 220, 220), lineWidth);
                cell.Style.Borders.Bottom = Headerpen;
                cell.Style.Borders.Left = Headerpen;
                cell.Style.Borders.Right = Headerpen;
                cell.Style.Borders.Top = Headerpen;
            }
        }
        private void GroupCaptionBorder(PdfGridCell cell, float lineWidth)
        {
            if (PdfExportProps?.Theme?.Caption?.Border != null)
            {
                var CaptionBorder = PdfExportProps?.Theme.Caption.Border.Color;
                lineWidth = PdfExportProps?.Theme.Caption.Border.Width > 0 ? (float)PdfExportProps?.Theme.Caption.Border.Width! : lineWidth;
                PdfPen Captionpen = !string.IsNullOrEmpty(CaptionBorder) ? new PdfPen(new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(CaptionBorder)), lineWidth) : new PdfPen(new PdfSolidBrush(AutoFormat.HeaderBorderColor), lineWidth);
                cell.Style.Borders.Bottom = Captionpen;
                cell.Style.Borders.Left = Captionpen;
                cell.Style.Borders.Right = Captionpen;
                cell.Style.Borders.Top = Captionpen;
            }
            else
            {
                PdfPen Captionpen = new PdfPen(new PdfColor(220, 220, 220), lineWidth);
                cell.Style.Borders.Bottom = Captionpen;
                cell.Style.Borders.Left = Captionpen;
                cell.Style.Borders.Right = Captionpen;
                cell.Style.Borders.Top = Captionpen;
            }
        }

        private void RecordFieldCellBorder(PdfGridCell cell, string border, float lineWidth)
        {
            if (PdfExportProps?.Theme?.Record?.Border != null)
            {
                border = PdfExportProps?.Theme.Record.Border.Color!;
                lineWidth = PdfExportProps?.Theme.Record.Border.Width > 0 ? (float)PdfExportProps?.Theme.Record.Border.Width! : lineWidth;
                PdfPen pen = !string.IsNullOrEmpty(border) ? new PdfPen(new PdfSolidBrush(ExportHelper<T>.GetDrawingColorFromHexString(border)), lineWidth) : new PdfPen(new PdfSolidBrush(AutoFormat.HeaderBorderColor), lineWidth);
                cell.Style.Borders.Bottom = pen;
                cell.Style.Borders.Left = pen;
                cell.Style.Borders.Right = pen;
                cell.Style.Borders.Top = pen;
            }
            else
            {
                PdfPen pen = new PdfPen(new PdfColor(220, 220, 220), lineWidth);
                cell.Style.Borders.Bottom = pen;
                cell.Style.Borders.Left = pen;
                cell.Style.Borders.Right = pen;
                cell.Style.Borders.Top = pen;
            }
        }

        private static PageNumberResult SetContentFormat(PdfHeaderFooterContent Content, PdfStringFormat Format)
        {
            if (Content.Size != null)
            {
                float Width = (float)(Content.Size.Width * 0.75);
                float Height = (float)(Content.Size.Height * 0.75);
                Format = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
                if (Content.Style?.HAlign != null)
                {
                    switch (Content.Style.HAlign.ToString())
                    {
                        case "Right":
                            Format.Alignment = PdfTextAlignment.Right;
                            break;
                        case "Center":
                            Format.Alignment = PdfTextAlignment.Center;
                            break;
                        case "Justify":
                            Format.Alignment = PdfTextAlignment.Justify;
                            break;
                        default:
                            Format.Alignment = PdfTextAlignment.Left;
                            break;
                    }
                }

                if (Content.Style?.VAlign != null)
                {
                    Format = GetVerticalAlignment(Content.Style.VAlign.ToString(), Format);
                }

                return new PageNumberResult() { Format = Format, Size = new SizeF(Width, Height) };
            }

            return null!;
        }
        private static PdfStringFormat GetVerticalAlignment(string VerticalAlign, PdfStringFormat Format = null!, string TextAlign = " ")
        {
            if (Format == null)
            {
                Format = new PdfStringFormat();
                Format = GetHorizontalAlignment(TextAlign, Format);
            }

            switch (VerticalAlign)
            {
                case "Bottom":
                    Format.LineAlignment = PdfVerticalAlignment.Bottom;
                    break;
                case "Middle":
                    Format.LineAlignment = PdfVerticalAlignment.Middle;
                    break;
                case "Top":
                    Format.LineAlignment = PdfVerticalAlignment.Top;
                    break;
            }

            return Format;
        }

        private static PdfStringFormat GetHorizontalAlignment(string TextAlign, PdfStringFormat Format = null!)
        {
            if (Format == null)
            {
                Format = new PdfStringFormat();
            }

            switch (TextAlign)
            {
                case "Right":
                    Format.Alignment = PdfTextAlignment.Right;
                    break;
                case "Center":
                    Format.Alignment = PdfTextAlignment.Center;
                    break;
                case "Justify":
                    Format.Alignment = PdfTextAlignment.Justify;
                    break;
                case "Left":
                case "None":
                    Format.Alignment = PdfTextAlignment.Left;
                    break;
            }

            return Format;
        }

        private static PdfNumberStyle GetPageNumberStyle(PdfPageNumberType pageNumberType)
        {
            switch (pageNumberType.ToString())
            {
                case "LowerLatin":
                    return PdfNumberStyle.LowerLatin;
                case "LowerRoman":
                    return PdfNumberStyle.LowerRoman;
                case "UpperLatin":
                    return PdfNumberStyle.UpperLatin;
                case "UpperRoman":
                    return PdfNumberStyle.UpperRoman;
                default:
                    return PdfNumberStyle.Numeric;
            }
        }

        internal class PageNumberResult
        {
            public PdfStringFormat? Format { get; set; }

            public SizeF Size { get; set; }
        }

        // TODO RTLFormatValue
        void IDisposable.Dispose()
        {
            ms?.Dispose();
            GridPropertyHelper?.Dispose();
            _pdfDocument?.Dispose();
        }
    }
}