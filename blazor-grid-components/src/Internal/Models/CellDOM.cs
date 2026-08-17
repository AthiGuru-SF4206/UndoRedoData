using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Handles cell and row customization.
    /// </summary>
    /// <remarks>
    /// An instance of this class is passed in QueryCellInfo and RowDataBound events as argument
    /// through which the conditional class and styles can be added to cell/row.
    /// </remarks>
    public class CellDOM
    {
        [JsonPropertyName("id")]
        internal string? ID { get; set; }

        [JsonPropertyName("xPath")]
        internal string? XPath { get; set; }

        [JsonPropertyName("domUUID")]
        internal string? DomUUID { get; set; }

        [JsonPropertyName("elementID")]
        internal string? ElementID { get; set; }

        internal bool HasChanges { get; set; }

        internal List<string> ClassList { get; set; }

        internal List<string> Styles { get; set; }

        internal IDictionary<string, object> AttributeList { get; set; }

        internal IJSRuntime? JsRuntime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellDOM"/> class with optional class, style, and attribute lists.
        /// </summary>
        public CellDOM(List<string> classList = null!, List<string> styleList = null!, IDictionary<string, object> attributeList = null!)
        {
            ClassList = classList;
            Styles = styleList;
            AttributeList = attributeList;
        }

        /// <summary>
        /// Add multiple class names to the specific cell.
        /// </summary>
        /// <param name="classList">List of class names.</param>
        public void AddClass(string[] classList) => AddItem(ClassList, classList);

        /// <summary>
        /// Add multiple style rules to the specific cell.
        /// </summary>
        /// <param name="styles">List of style rules.</param>
        public void AddStyle(string[] styles) => AddItem(Styles, styles);

        /// <summary>
        /// Add attributes to the given DOM element.
        /// </summary>
        /// <param name="attributes"> List of key and values to be added.</param>
        public void SetAttribute(IDictionary<string, object> attributes)
        {
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    AttributeList.AddOrUpdateItem(attribute.Key, attribute.Value);
                }
            }

            HasChanges = true;
        }

        /// <summary>
        /// Add array of values to the list.
        /// </summary>
        /// <param name="AddTo">Source list to be added.</param>
        /// <param name="values">Array values to be added to the list. </param>
        protected void AddItem(List<string> AddTo, string[] values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    AddTo?.AddOrSkip(value);
                }
            }

            HasChanges = true;
        }
    }
}