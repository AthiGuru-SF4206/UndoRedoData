using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Provides cell register and rendering logic.
    /// </summary>
    internal class CellComponentService
    {
        private Dictionary<string, RenderFragment<object>> _cellComponents = new Dictionary<string, RenderFragment<object>>();
        private Dictionary<string, Func<Row<object>, Cell<object>, RenderFragment>> _cellRenderers = new Dictionary<string, Func<Row<object>, Cell<object>, RenderFragment>>();
        private Dictionary<int, Func<Row<object>, Cell<object>, RenderFragment>> _cellRenderersByIndex = new Dictionary<int, Func<Row<object>, Cell<object>, RenderFragment>>();

        public Func<Row<object>, Cell<object>, string>? CellSelector { get; set; }

        public bool HasCell(string name) => name == null ? false : _cellComponents.ContainsKey(name);

        public bool HasRenderer(string name) => _cellRenderers.ContainsKey(name);

        public bool HasRenderer(int index) => _cellRenderersByIndex.ContainsKey(index);

        public RenderFragment<object> GetCell(string name) => HasCell(name) ? _cellComponents[name] : null!;

        public Func<Row<object>, Cell<object>, RenderFragment> GetCellRenderer(string name) => HasRenderer(name) ? _cellRenderers[name] : null!;

        public Func<Row<object>, Cell<object>, RenderFragment> GetCellRendererByIndex(int index) => HasRenderer(index) ? _cellRenderersByIndex[index] : null!;

        public void AddCell(string name, RenderFragment<object> fragment)
        {
            if (HasCell(name))
            {
                _cellComponents[name] = fragment;
            }
            else
            {
                _cellComponents.Add(name, fragment);
            }
        }

        public void AddRender(string name, Func<Row<object>, Cell<object>, RenderFragment> renderFunc)
        {
            if (HasRenderer(name))
            {
                _cellRenderers[name] = renderFunc;
            }
            else
            {
                _cellRenderers.Add(name, renderFunc);
            }
        }

        public void AddRender(int index, Func<Row<object>, Cell<object>, RenderFragment> renderFunc)
        {
            if (HasRenderer(index))
            {
                _cellRenderersByIndex[index] = renderFunc;
            }
            else
            {
                _cellRenderersByIndex.Add(index, renderFunc);
            }
        }
    }
}
