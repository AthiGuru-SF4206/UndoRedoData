using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid templates.
    /// </summary>
    public partial class GridTemplates : SfOwningComponentBase
    {
        #region Cascading Context
        [CascadingParameter]
        internal IGrid? TemplateParent { get; set; }
        #endregion

        #region Child Content
        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }
        #endregion

        #region Row and Detail Templates
        /// <summary>
        /// Gets or sets the row template to customize row elements.
        /// </summary>
        /// <remarks>
        /// The RowTemplate content must be TD elements and the number of TD elements must match the number of datagrid columns.
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<object>? RowTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template to customize detail row element.
        /// </summary>
        /// <remarks>
        /// Use DetailTemplate to render hierarchy grid. It supports N level of nested grids.
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<object>? DetailTemplate { get; set; }
        #endregion

        #region Specialized Templates
        /// <summary>
        /// Gets or sets the template to customize empty grid row element.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.EmptyRecordTemplateContext"/>.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<EmptyRecordTemplateContext>? EmptyRecordTemplate { get; set; }

        /// <summary>
        /// Gets or sets a template to customize the tooltip content when hovering over grid cells and headers.
        /// </summary>
        /// <value>
        /// A <see cref="RenderFragment{TooltipTemplateContext}"/> used to define custom tooltip content. No default value.
        /// </value>
        /// <remarks>
        /// <para>
        /// This template enables rich customization of tooltip content in the Blazor DataGrid. It is rendered when tooltips are enabled via the <c>ShowTooltip</c> property.
        /// </para>
        /// <para>
        /// The template receives a context parameter of type <see cref="Syncfusion.Blazor.Grids.TooltipTemplateContext"/>, which provides access to relevant data such as the grid column, row data, and cell value.
        /// </para>
        /// <para>
        /// To activate tooltip, ensure that the <c>ShowTooltip</c> property is set to <see langword="true"/>. Without this setting, the <c>TooltipTemplate</c> will not be rendered.
        /// </para>
        /// <para>
        /// This feature is useful for displaying additional contextual information, such as formatted values, icons, or images—especially when working with templated columns or truncated content.
        /// </para>
        /// </remarks>
        /// <example>
        /// Demonstrates how to define a custom <see cref="Syncfusion.Blazor.Grids.GridTemplates.TooltipTemplate"/> to display enhanced tooltip content.
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" ShowTooltip="true">
        /// <GridTemplates>
        /// <TooltipTemplate>
        /// var tooltip = context as TooltipTemplateContext;
        /// <span><b>@tooltip.Value</b></span>
        /// </TooltipTemplate>
        /// </GridTemplates>
        /// <GridColumns>
        /// <GridColumn Field="CustomerID" HeaderText="Customer ID" Width="150"></GridColumn>
        /// </GridColumns>
        /// </SfGrid>
        ///
        /// @code {
        /// public class Order
        /// {
        /// public string CustomerID { get; set; }
        /// }
        /// }
        /// ]]></code>
        /// </example>

        [Parameter]
        [JsonIgnore]
        public RenderFragment<TooltipTemplateContext>? TooltipTemplate { get; set; }
        #endregion

        #region Toolbar Template
        /// <summary>
        /// Render custom toolbar using ToolbarTemplate property. It replaces the in-built toolbar
        /// and click actions must be handled in custom toolbar itself.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<object>? ToolbarTemplate { get; set; }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Invoked during component initialization.
        /// Sets the current instance as the grid template provider if a template parent exists.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if(TemplateParent != null)
            {
                TemplateParent.GridTemplates = this;
            }
            // Telemetry for Templates
            GridTelemetryHelper.LogTelemetry(true, "Templates");

        }
        #endregion
    }
}