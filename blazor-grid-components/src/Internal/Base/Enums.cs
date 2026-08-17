namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Specifies the grid`s cell type.
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// Define the cell as Data cell
        /// </summary>
        Data,

        /// <summary>
        /// Define the cell as detail row`s cell
        /// </summary>
        Detail,

        /// <summary>
        /// Define the cell as detail indent cell
        /// </summary>
        DetailIndent,

        /// <summary>
        /// Define the cell as indent cell
        /// </summary>
        Indent,

        /// <summary>
        /// Define the cell as row drag and drop indent cell
        /// </summary>
        RowDrag,

        /// <summary>
        /// Define the cell as expand cell
        /// </summary>
        Expand,

        /// <summary>
        /// Define the cell as group caption cell
        /// </summary>
        GroupCaption,

        /// <summary>
        /// Define the cell as empty group caption cell
        /// </summary>
        GroupCaptionEmpty,

        /// <summary>
        /// Define the cell as caption summary cell
        /// </summary>
        CaptionSummary,

        /// <summary>
        /// Define the cell as summary/aggregate cell
        /// </summary>
        Summary,

        /// <summary>
        /// Define the cell as command column cell
        /// </summary>
        CommandColumn,

        /// <summary>
        /// Define the cell as stacked header cell
        /// </summary>
        StackedHeader
    }
}
