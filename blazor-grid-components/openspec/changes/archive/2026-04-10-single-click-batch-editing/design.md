# Single-Click Batch Editing - Design Document

## Architecture Overview

The single-click batch editing feature follows a **3-layer architecture**:

```
┌─────────────────────────────────────┐
│  Presentation Layer (Blazor)        │
│  - GridEditSettings.razor           │
│  - GridEditSettings.cs (Parameter)  │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│  Business Logic Layer (.NET)        │
│  - GridJSInteropAdaptor.cs          │
│    (JS→.NET bridge)                 │
│  - Edit<T>.cs                       │
│    (SingleClickHandler, SaveCell)   │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│  UI Event Layer (JavaScript)        │
│  - sf-grid-fn.ts                    │
│    (Click listener, UID resolution) │
│  - interfaces.ts                    │
│    (IGridOptions.allowEditOnSingleClick)  │
└─────────────────────────────────────┘
```

## Component Design

### 1. Presentation Layer

**File**: `src/GridEditSettings.cs`

```csharp
[Parameter]
public bool AllowEditOnSingleClick { get; set; }  // default: false

private bool _allowEditOnSingleClick { get; set; }
```

- Simple boolean property, opt-in only
- Serialized to JS options via `GetClientOption()`
- Change detection ensures JS listener updates dynamically

### 2. Business Logic Layer

#### A. Interop Bridge

**File**: `src/Internal/Base/GridJSInteropAdaptor.cs`

```csharp
[JSInvokable]
public async Task SingleClickEditCell(string rowUid, string cellUid)
```

**Guards**:
- Verify `AllowEditOnSingleClick == true`
- Verify `Mode == EditMode.Batch`
- Verify `AllowEditing == true`
- Verify row/cell exist and cell is data row (not caption/header)

**Action**: Delegate to `Edit<T>.SingleClickHandler(row, cell)`

#### B. Edit Handler

**File**: `src/Internal/Actions/Edit.cs`

```csharp
internal async Task SingleClickHandler(Row<object> row, Cell<object> cell)
{
    // Guard 1: Primary key check
    var keys = await Parent.GetPrimaryKeyFieldNamesAsync();
    if (keys.Count > 0 && keys[0] == cell.Column.Field && !cell.IsDirty && !IsAdd)
    {
        cell.EditDisabled = true;
        return;
    }

    // Guard 2: Column editability
    if (!cell.Column.AllowEditing) return;

    // Auto-save previous cell if open
    if (Parent.IsEdit)
    {
        await SaveCell();
        await ValidateNextCell();
        if (Parent.IsEdit) return;  // validation blocked
    }

    // Open new cell
    await EditCell(row, cell);
}
```

**Key Design Decisions**:
- No new save logic; reuses existing `SaveCell()` method
- No new validation logic; reuses existing `ValidateNextCell()` method
- Validation failure blocks move; user stays on failed cell
- All event firing delegated to `EditCell()` and `SaveCell()`

### 3. UI Event Layer

#### A. Click Listener

**File**: `scripts/sf-grid-fn.ts`

```typescript
private singleClickEditCellHandler = (e: MouseEventArgs): void => {
    const cell = closest(e.target as Element, 'td.e-rowcell');
    if (!cell) return;
    
    const row = closest(cell, 'tr.e-row');
    if (!row) return;
    
    const rowUid = row.getAttribute('data-uid');
    const cellUid = cell.getAttribute('data-uid');
    
    if (rowUid && cellUid) {
        this.dotNetRef.invokeMethodAsync('SingleClickEditCell', rowUid, cellUid);
    }
};
```

**Guards**:
- Closest selector for `td.e-rowcell` excludes headers and non-cell elements
- Closest selector for `tr.e-row` ensures valid row context
- `data-uid` attributes validate UID presence before invoking

#### B. Event Registration

**File**: `scripts/sf-grid-fn.ts`

```typescript
// wireEvents()
if (this.options.allowEditOnSingleClick && this.options.allowEditing) {
    this.delegateSingleClickHandler = this.singleClickEditCellHandler.bind(this);
    const gridContent = this.getContent();
    if (gridContent) {
        EventHandler.add(gridContent, 'click', this.delegateSingleClickHandler, this);
    }
}

// unWireEvents()
if (this.options.allowEditOnSingleClick) {
    const gridContent = this.getContent();
    if (gridContent && this.delegateSingleClickHandler) {
        EventHandler.remove(gridContent, 'click', this.delegateSingleClickHandler);
    }
}
```

**Dynamic Option Updates**:
```typescript
// setOptions()
if (oldOptions.allowEditOnSingleClick !== newOptions.allowEditOnSingleClick) {
    if (newOptions.allowEditOnSingleClick && !oldOptions.allowEditOnSingleClick) {
        // Attach listener
    } else if (!newOptions.allowEditOnSingleClick && oldOptions.allowEditOnSingleClick) {
        // Remove listener
    }
}
```

## Data Flow

```
User Single-Clicks Cell
       │
       ▼
JS Click Event Handler
       │
       ├─ Find cell: closest(e.target, 'td.e-rowcell')
       ├─ Find row: closest(cell, 'tr.e-row')
       ├─ Extract UIDs: row.data-uid, cell.data-uid
       │
       ▼
dotNetRef.invokeMethodAsync('SingleClickEditCell', rowUid, cellUid)
       │
       ▼
GridJSInteropAdaptor.SingleClickEditCell(rowUid, cellUid)
       │
       ├─ Guard: AllowEditOnSingleClick, Mode, AllowEditing
       ├─ Resolve: row & cell by UID lookup
       │
       ▼
Edit<T>.SingleClickHandler(row, cell)
       │
       ├─ Guard: Primary key, AllowEditing
       ├─ Auto-save: if (Parent.IsEdit) { SaveCell() + ValidateNextCell() }
       │
       ▼
Edit<T>.EditCell(row, cell)
       │
       ├─ Fire: OnCellEdit event
       ├─ Validate: EditContext if applicable
       ├─ Select: Row (unless PersistSelection)
       ├─ Update: Parent.IsEdit = true, cell.IsEdit = true
       │
       ▼
UI Re-render: Cell enters edit mode
```

## Cross-Feature Interaction Matrix

| Feature | Interaction | Design |
|---------|-------------|--------|
| **Grouping** | Caption rows not editable | JS guard: `closest('tr.e-row')` only; captions are non-data rows |
| **Virtualization** | UID-based lookup works in virtual window | Edit<T> uses row.Uid, not DOM index |
| **Frozen Columns** | Both panes editable | No freeze-specific logic; existing EditCell works |
| **Selection** | Row selected on single-click | Existing EditCell logic; PersistSelection respected |
| **Validation** | Click-away blocked on error | SaveCell() validation reused; tooltip shown |
| **Keyboard Tab** | Auto-save then move | ValidateNextCell() reused; keyboard flow unchanged |
| **Sorting/Filtering** | Data re-renders; edit closes | Existing EventAggregator + ContentStateChanged |
| **Paging** | Page change auto-saves cell | Existing pattern: check Parent.IsEdit before page change |

## Event Firing Sequence

### Single-Click Edit (New Cell Entry)

1. **OnCellEdit** (fired by EditCell)
   - args.ColumnName, ColumnName, Data, PrimaryKey, ValidationRules, etc.
   - Cancel = true blocks edit activation

2. **OnActionBegin** (fired by ModelChanged after EditCell)
   - RequestType = "BeginEdit"

3. **OnActionComplete** (fired after render)
   - RequestType = "BeginEdit"

### Click-Away (Cell Save)

1. **OnCellSave** (fired by SaveCell)
   - args.Value, PreviousValue, RowData, ColumnName, etc.
   - Cancel = true blocks save; cell stays open

2. **CellSaved** (fired by SaveCell)
   - Fired only if OnCellSave did not cancel

3. **OnActionBegin** / **OnActionComplete** (around save)
   - RequestType = "SaveCell"

## Error Handling Strategy

| Scenario | Handling |
|----------|----------|
| **Primary Key Column (existing row)** | `cell.EditDisabled = true`; no edit |
| **Non-Editable Column** | `return` early; no edit |
| **Batch Mode Not Active** | Guard in JSInvokable; no-op |
| **Validation Error on Click-Away** | `SaveCell()` fails; `Parent.IsEdit` remains true; tooltip shown |
| **Previous Cell Invalid** | `ValidateNextCell()` detects; current cell stays open; new cell not opened |

## Performance Considerations

1. **Listener Scope**: Attached to grid content, not document → fewer event propagations
2. **Conditional Registration**: Only attached when both `allowEditOnSingleClick=true` AND `allowEditing=true`
3. **UID Resolution**: O(n) lookup already used by existing features; no new algorithmic cost
4. **Memory**: Single listener function reused; no per-row overhead

## Backward Compatibility

- **Default**: `AllowEditOnSingleClick = false` → zero impact
- **Double-Click**: Still works independently; no interference
- **Keyboard**: Tab, F2, Escape all work unchanged
- **APIs**: No signature changes; no breaking changes
- **Events**: All existing events fire with identical args

## Testing Strategy

### Unit Level
- [ ] SingleClickHandler method guards all tested
- [ ] SaveCell() reuse verified (no new paths)
- [ ] Event args structure verified

### Integration Level
- [ ] Single-click → edit activation
- [ ] Click-away → save + validation
- [ ] Validation failure → block move
- [ ] Keyboard navigation unchanged
- [ ] Selection modes work

### Cross-Feature Level
- [ ] Grouping + single-click
- [ ] Virtualization + single-click
- [ ] Frozen columns + single-click
- [ ] Paging + single-click
- [ ] Filtering + single-click

### Regression Level
- [ ] Double-click still works
- [ ] F2 key still works
- [ ] Tab/Shift+Tab unchanged
- [ ] All events fire

---

**Design Status**: COMPLETE  
**Architecture Pattern**: Layered (UI → Business → Data)  
**Risk Level**: LOW (reuses existing code paths)  
**Pair With**: editing-skill, validation-skill, selection-skill
