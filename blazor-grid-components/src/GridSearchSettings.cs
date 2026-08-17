using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid searching.
    /// </summary>
    public partial class GridSearchSettings : SfDataBoundComponent
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
        /// Specifies the collection of fields included in search operation. By default, bounded columns of the Grid are included.
        /// </summary>
        [Parameter]
        public string[]? Fields { get; set; }

        private string[]? _fields { get; set; }

        /// <summary>
        /// If ignoreAccent set to true, then filter ignores the diacritic characters or accents while filtering.
        /// </summary>
        /// <remarks>Ignore accent is supported by remote data alone.
        /// IgnoreAccent key will be sent to server and operation should be handled at user level.</remarks>
        [Parameter]
        public bool IgnoreAccent { get; set; }

        private bool _ignoreAccent { get; set; }

        /// <summary>
        /// If IgnoreCase is set to false, searches records that match exactly, else
        /// searches records that are case insensitive(uppercase and lowercase letters treated the same).
        /// </summary>
        [Parameter]
        public bool IgnoreCase { get; set; } = true;

        private bool _ignoreCase { get; set; }

        /// <summary>
        /// Specifies the key value to search Grid records at initial rendering.
        /// You can also get the current search key.
        /// </summary>
        [Parameter]
        public string Key { get; set; } = string.Empty;

        private string? _key { get; set; }

        /// <summary>
        /// Defines the operator to search records.
        /// </summary>
        [Parameter]
        public Syncfusion.Blazor.Operator Operator { get; set; }

        private Syncfusion.Blazor.Operator _operator { get; set; }

        internal static async Task<GridSearchSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridSearchSettings = new GridSearchSettings();
            GridSearchSettings.Parent = (IGrid)baseComponent;
            GridSearchSettings.BaseParent = (IGrid)baseComponent;
            await GridSearchSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridSearchSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current search settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperties(nameof(IGrid.SearchSettings), this);
            _fields = Fields;
            _ignoreAccent = IgnoreAccent;
            _ignoreCase = IgnoreCase;
            _key = Key;
            _operator = Operator;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Synchronizes search-related properties such as fields, case sensitivity, and operator settings,
        /// and notifies the parent component if any changes occur.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            Fields = _fields = await UpdateProperty(nameof(Fields), Fields, _fields!).ConfigureAwait(true);
            IgnoreAccent = _ignoreAccent = await UpdateProperty(nameof(IgnoreAccent), IgnoreAccent, _ignoreAccent).ConfigureAwait(true);
            IgnoreCase = _ignoreCase = await UpdateProperty(nameof(IgnoreCase), IgnoreCase, _ignoreCase).ConfigureAwait(true);
            Key = _key = await UpdateProperty(nameof(Key), Key, _key!).ConfigureAwait(true);
            Operator = _operator = await UpdateProperty(nameof(Operator), Operator, _operator).ConfigureAwait(true);

            if (PropertyChanges.Count > 0 && BaseParent != null)
            {
                ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.SearchSettings), this);
                PropertyChanges.Clear();
                await BaseParent.PropertyChanged().ConfigureAwait(true);
            }
        }

        internal async Task UpdateProperties(string key, string value)
        {
            if (key == nameof(Key))
            {
                var keyValue = DirectParameters.TryGetValue("Key", out object? val) ? val : Key;
                Key = _key = (string)await UpdateProperty(nameof(Key), keyValue, value).ConfigureAwait(true);
            }
        }
    }
}
