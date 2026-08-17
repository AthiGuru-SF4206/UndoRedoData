using Syncfusion.PdfExport;
using System.Drawing;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Gets or sets a value that indicates the auto format for an element.
    /// </summary>
    public class AutoFormat
    {
        /// <summary>
        /// Gets or sets a value that indicates the font-family for an element.
        /// </summary>
        public string? FontFamily { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the font-size for an element.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the border-size for an element with four values.
        /// </summary>
        public int BorderSize { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the border-type for an element.
        /// </summary>
        public string? BorderType { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the background end color for header element.
        /// </summary>
        public Color HeaderBackEndColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the border bottom color for header element.
        /// </summary>
        public Color HeaderBorderBottomColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the border color for header element.
        /// </summary>
        public Color HeaderBorderColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the font color for header element.
        /// </summary>
        public Color HeaderFontColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the font size for header element.
        /// </summary>
        public int HeaderFontSize { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the background color for the group header.
        /// </summary>
        public Color GroupHeaderBackColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the border color for the group header.
        /// </summary>
        public Color GHeaderBorderColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the background color for the group header.
        /// </summary>
        public PdfColor ContentBackColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the border color for the content.
        /// </summary>
        public Color ContentBorderColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the font size for the content.
        /// </summary>
        public int ContentFontSize { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the font color for the groupcontent.
        /// </summary>
        public Color GroupContentFontColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the border color for the groupcaption.
        /// </summary>
        public Color GroupCaptionBorderColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the background color for an alternate row.
        /// </summary>
        public Color AltRowBackColor { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the background color for the caption.
        /// </summary>
        public Color CaptionBackColor { get; set; }

        /// <summary>
        /// Sets the theme for the specified AutoFormat object by applying predefined styles.
        /// </summary>
        public static void SetTheme(AutoFormat autoFormat, string theme)
        {
            var formatTheme = theme;
            if (autoFormat != null)
            {
                autoFormat.HeaderFontSize = 9;
                autoFormat.HeaderFontColor = Color.Black;
                autoFormat.GroupHeaderBackColor = Color.Gray;
                autoFormat.HeaderBorderBottomColor = Color.LightGray;
                autoFormat.HeaderBorderColor = Color.LightGray;
                autoFormat.FontFamily = "Calibri";
                autoFormat.ContentFontSize = 10;
                autoFormat.ContentBackColor = Color.White;
                autoFormat.GroupContentFontColor = Color.FromArgb(92, 92, 92);
                autoFormat.AltRowBackColor = Color.WhiteSmoke;
                autoFormat.GHeaderBorderColor = Color.Gray;
                autoFormat.ContentBorderColor = Color.LightGray;
                autoFormat.GroupCaptionBorderColor = Color.WhiteSmoke;
            }
        }
    }
}
