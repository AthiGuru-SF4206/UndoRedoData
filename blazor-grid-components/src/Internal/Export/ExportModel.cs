namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Specifies the document option.
    /// </summary>
    /// <exclude/>
    public enum DocumentOption
    {
        /// <summary>
        /// Append at end
        /// </summary>
        AppendAtEnd,

        /// <summary>
        /// Append at last page
        /// </summary>
        LastPage
    }

    /// <summary>
    /// Specifies font weight.
    /// </summary>
    /// <exclude/>
    public enum FontWeight
    {
        /// <summary>
        /// Text is bold
        /// </summary>
        Bold,

        /// <summary>
        /// Text is normal
        /// </summary>
        Normal
    }

    /// <summary>
    /// spanned row.
    /// </summary>
    /// <exclude/>
    internal class SpannedRow
    {
        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }

        public int SpannedCell { get; set; }
    }

    internal class PdfSpannedRow
    {
        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }

        public int SpannedCell { get; set; } = -1;
    }

    /// <summary>
    /// Defines cell types.
    /// </summary>
    public enum GridTableCellType
    {
        /// <summary>
        /// Group caption cell.
        /// </summary>
        GroupCaptionCell,

        /// <summary>
        /// Group caption cell.
        /// </summary>
        FirstRecord,

        /// <summary>
        /// Indent cell in group header section.
        /// </summary>
        GroupHeaderIndentCell,

        /// <summary>
        /// Field cell in summary row.
        /// </summary>
        SummaryFieldCell,

        /// <summary>
        /// Indent cell in group.
        /// </summary>
        IndentCell,

        /// <summary>
        /// The top-left header cell.
        /// </summary>
        TopLeftHeaderCell,

        /// <summary>
        /// Any row header cell.
        /// </summary>
        RowHeaderCell,

        /// <summary>
        /// Column header cell.
        /// </summary>
        ColumnHeaderCell,

        /// <summary>
        /// PlusMinus cell in a record row.
        /// </summary>
        RecordPlusMinusCell,

        /// <summary>
        /// Field cell in a non-alternate record row.
        /// </summary>
        RecordFieldCell,

        /// <summary>
        /// Field cell in an alternate record row.
        /// </summary>
        AlternateRecordFieldCell,

        /// <summary>
        /// Any header cell in an alternate record row.
        /// </summary>
        AlternateRecordRowHeaderCell,

        /// <summary>
        /// Empty cell
        /// </summary>
        EmptyCell,

        /// <summary>
        /// Caption Cell
        /// </summary>
        CaptionCell,

        /// <summary>
        /// Master Cell
        /// </summary>
        MasterCell,

        /// <summary>
        /// Master Header Cell
        /// </summary>
        MasterHeaderCell,
        /// <summary>
        /// Caption Summary Cell
        /// </summary>
        CaptionSummary
    }

    // public enum MultipleExportType
    // {
    //    AppendToSheet,
    //    NewSheet
    // }

    /// <summary>
    /// Enum for TableOptions   .
    /// </summary>
    public enum TableOptions
    {
        /// <summary>
        /// ColumnHeaderRowHeight
        /// </summary>
        /// <remarks></remarks>
        ColumnHeaderRowHeight,

        /// <summary>
        /// RecordRowHeight
        /// </summary>
        /// <remarks></remarks>
        RecordRowHeight,

        /// <summary>
        /// CaptionRowHeight
        /// </summary>
        /// <remarks></remarks>
        CaptionRowHeight,

        /// <summary>
        /// IndentCellWidth
        /// </summary>
        /// <remarks></remarks>
        IndentCellWidth,

        /// <summary>
        /// ContentCellWidth
        /// </summary>
        /// <remarks></remarks>
        ContentCellWidth
    }

    /// <summary>
    /// Defines the export model.
    /// </summary>
    /// <exclude/>
    public class ExportModel
    {
        ///// <summary>
        ///// Defines the color model.
        ///// </summary>
        ///// <exclude/>
        //public partial struct Color
        //{
        //    public string Name { get; set; }

        //    public int Red { get; set; }

        //    public int Green { get; set; }

        //    public int Blue { get; set; }

        //    public bool isUnKnownColor { get; set; }

        //    public static implicit operator Color(string value)
        //    {
        //        return new Color
        //        {
        //            Name = value
        //        };
        //    }

        //    public Color(int r, int g, int b, string name)
        //    {
        //        Red = r;
        //        Green = g;
        //        Blue = b;
        //        Name = name;
        //        isUnKnownColor = true;
        //    }
        //}
    }
}
