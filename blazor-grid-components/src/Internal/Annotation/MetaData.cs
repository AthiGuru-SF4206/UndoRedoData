using System;
using System.Collections.Generic;
using System.Reflection;

namespace Syncfusion.Blazor
{
    internal class Metadata
    {
        /// <summary>
        /// Gets or sets the order weight of the column.
        /// </summary>
        /// <remarks>
        /// Columns are sorted in increasing order based on the order value. 
        /// Columns without this attribute have an order value of 0. 
        /// Negative values are valid and can be used to position a column before all non-negative columns. 
        /// If an order is not specified, presentation layers should consider using the value 10000. 
        /// This value lets explicitly-ordered fields be displayed before and after the fields that do not have a specified order.
        /// </remarks>
        internal int Order { get; set; }

        internal bool IsPrimaryKey { get; set; }

        internal bool IsIdentity { get; set; }

        internal string? ForeignKey { get; set; }

        internal bool Visible { get; set; } = true;

        internal bool ApplyFormatInEditMode { get; set; }

        internal bool NeedsHtmlEncode { get; set; }

        internal bool ReadOnly { get; set; }

        internal string? HeaderText { get; set; }

        internal string? Watermark { get; set; }

        internal bool AutoGenerateField { get; set; } = true;

        internal string? GroupDisplayName { get; set; }

        internal string? FormatString { get; set; }

        internal Dictionary<string, object>? Validations { get; set; }

        internal string? CustomDataType { get; set; }

        internal PropertyInfo? Property { get; set; }

        internal bool AutoGenerateFilter { get; set; } = true;

        internal string? NullDisplayText { get; set; }

        internal bool ConvertEmptyStringToNull { get; set; }

        internal string? Description { get; set; }

        internal Metadata()
        {
            
        }
    }
}
