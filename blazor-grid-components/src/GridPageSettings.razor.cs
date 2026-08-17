using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Linq;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid paging.
    /// </summary>
    public partial class GridPageSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Defines the current page number of the pager.
        /// </summary>
        [Parameter]
        public int CurrentPage { get; set; } = 1;

        private int _currentPage { get; set; }

        /// <summary>
        /// If EnableQueryString set to true,
        /// then it pass current page information as a query string along with the URL while navigating to other page.
        /// </summary>
        [Parameter]
        public bool EnableQueryString { get; set; }

        private bool _enableQueryString { get; set; }

        /// <summary>
        /// Defines the number of pages to be displayed in the pager container.
        /// </summary>
        [Parameter]
        public int PageCount { get; set; } = 8;

        private int _pageCount { get; set; }

        /// <summary>
        /// Defines the number of records to be displayed per page.
        /// </summary>
        [Parameter]
        public int PageSize { get; set; } = 12;

        private int _pageSize { get; set; }

        /// <summary>
        /// If PageSizes set to true or Array of values,
        /// It renders DropDownList in the pager which allow us to select pageSize from DropDownList.
        /// </summary>
        [Parameter]
        public object? PageSizes { get; set; }

        private object? _pageSizes { get; set; }

        /// <summary>
        /// Defines the template which renders customized elements in pager instead of default elements.
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Navigations.PagerTemplateContext"/> of the grid.
        /// </remarks>
        /// </summary>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<object>? Template { get; set; }

        /// <summary>
        /// Enable or disable the ExternalMessage.
        /// </summary>
        public bool EnableExternalMessage { get; set; }

        /// <summary>
        /// Defines the pager External message.
        /// </summary>
        public string? ExternalMessage { get; set; }

        internal static async Task<GridPageSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridPageSettings = new GridPageSettings();
            GridPageSettings.Parent = (IGrid)baseComponent;
            GridPageSettings.BaseParent = (IGrid)baseComponent;

            // GridPageSettings.IsAutoInitialized = true;
            await GridPageSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridPageSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current paging settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperties(nameof(IGrid.PageSettings), this);
            _currentPage = CurrentPage;
            _enableQueryString = EnableQueryString;
            _pageCount = PageCount;
            _pageSize = PageSize;
            _pageSizes = PageSizes;
            if(BaseParent!.EnablePersistence){
                await BaseParent.CallStateHasChangedAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Synchronizes paging-related properties such as current page, page size, and page count,
        /// and notifies the parent component if any changes occur.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            CurrentPage = _currentPage = await UpdateProperty(nameof(CurrentPage), CurrentPage, _currentPage).ConfigureAwait(true);
            EnableQueryString = _enableQueryString = await UpdateProperty(nameof(EnableQueryString), EnableQueryString, _enableQueryString).ConfigureAwait(true);
            PageCount = _pageCount = await UpdateProperty(nameof(PageCount), PageCount, _pageCount).ConfigureAwait(true);
            PageSize = _pageSize = await UpdateProperty(nameof(PageSize), PageSize, _pageSize).ConfigureAwait(true);
            if (PageSize == 0)
            {
                _pageSize = PageSize = 12;
            }
            PageSizes = _pageSizes = await UpdateProperty(nameof(PageSizes), PageSizes, _pageSizes!).ConfigureAwait(true);

            BaseParent!.UpdateChildProperties(nameof(IGrid.PageSettings), this);

            if (PropertyChanges.Count > 0)
            {
                ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.PageSettings), this);
                PropertyChanges.Clear();
                await BaseParent.PropertyChanged().ConfigureAwait(true);
            }
        }

        internal async Task UpdateProperties(string key, object value)
        {
            if (key == nameof(PageSize))
            {
                var pageSize = DirectParameters.TryGetValue("PageSize", out object? val) ? (int)val : PageSize;
                PageSize = _pageSize = await UpdateProperty(nameof(PageSize), pageSize, (int)value).ConfigureAwait(true);
            }
            else if (key == nameof(CurrentPage))
            {
                var currentPage = DirectParameters.TryGetValue("CurrentPage", out object? val) ? (int)val : CurrentPage;
                CurrentPage = _currentPage = await UpdateProperty(nameof(CurrentPage), currentPage, (int)value).ConfigureAwait(true);
            }
        }
    }
}
