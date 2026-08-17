# UndoRedo Integration Reference Guide

Quick reference for developers integrating UndoRedo with Syncfusion Blazor DataGrid.

**Generated**: August 12, 2026

---

## Quick Navigation

### Keyboard Events
```
JS → GridJSInteropAdaptor.GridKeyDown()
   → FocusHandler.ProcessGridKeyDown()
   → FocusHandler.ProcessKeyDown()
   → FocusHandler.ProcessKeyCombination() ← INTERCEPT HERE FOR Ctrl+Z/Y
```

### Edit Operations
```
Edit.SaveCell()    → Fires OnCellSave, CellSaved
Edit.BulkAddRow()  → Fires OnBatchAdd
Edit.BulkDelete()  → Fires OnBatchDelete
Edit.BatchSave()   → Fires OnBatchSave
Edit.BatchClose()  → Fires OnBatchCancel
```

### Dirty Tracking
```
Cell.IsDirty:          bool (true if value changed)
Row.IsDirty:           bool (true if any cell dirty)
Row.EditedData:        object (non-null if dirty)
Row.Action:            EditAction (None/Added/Edited/Deleted)
```

---

## File Locations & Methods

### 1. Keyboard Navigation

**File**: `src/Internal/Actions/FocusHandler.cs`

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| ProcessKeyDown | 230 | `async Task ProcessKeyDown(KeyboardEventArgs e, Row<object> row, Cell<object> cell, bool isHeader = false)` | Handle cell-level keyboard events |
| ProcessGridKeyDown | 589 | `async Task ProcessGridKeyDown(KeyboardEventArgs e, BeforeCellFocus? bf = null, bool isPagerFocused = true, bool isToolbarFocused = false, int? cellIndex = null, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false)` | Grid-level keyboard routing |
| ProcessKeyCombination | ~700 | `async Task ProcessKeyCombination(string keyCombination, int totalPages, BeforeCellFocus? bf = null, bool isPagerFocused = true, KeyboardEventArgs? e = null)` | Route key combinations to actions **← HOOK HERE** |
| GetNextCellIndex | ~765 | `static int GetNextCellIndex(bool isTabKey, int currentCellIndex, Row<object> editedRow)` | Calculate next cell for Tab/ShiftTab |

**Key Constants**:
```csharp
_parent.EditModule        // Access Edit operations
_parent.FocusModule       // Access focus state
_parent.IsMacDevice       // Platform detection
_parent.EventAggregator   // Event notifications
```

### 2. JS Interop Entry

**File**: `src/Internal/Base/GridJSInteropAdaptor.cs`

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| GridKeyDown | 585 | `[JSInvokable] async ValueTask GridKeyDown(object args, bool value, bool isPagerFocused, bool isToolbarFocused, int? cellIndex, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false, bool isMultiSelectPopUpOpened = false)` | JS keyboard event entry point |

**Usage**:
```csharp
// From JavaScript:
// await dotnetRef.invokeMethodAsync('GridKeyDown', keyboardEventArgs, ...);

// Key deserialization:
KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(
    args?.ToString()!,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
);
```

### 3. Key Detection

**File**: `src/Internal/Base/Utils.cs` (Lines 700+)

**Key Check Methods** (extend KeyboardEventArgs):
```csharp
// Undo/Redo checks
e.IsCtrlZ()              // Ctrl+Z
e.IsCtrlY()              // Ctrl+Y

// Editing checks
e.IsF2()                 // F2
e.IsDelete()             // Delete
e.IsEsc()                // Escape

// Navigation checks
e.IsUpArrow()            // Arrow Up
e.IsDownArrow()          // Arrow Down
e.IsLeftArrow()          // Arrow Left
e.IsRightArrow()         // Arrow Right
e.IsHome()               // Home
e.IsEnd()                // End
e.IsTab()                // Tab
e.IsShiftTab()           // Shift+Tab

// Get combination string
string combo = e.GetKeyCombination(isMacDevice: false);
// Returns: "Tab", "ShiftTab", "CtrlZ", "CtrlY", etc.
```

### 4. Edit Operations - SaveCell

**File**: `src/Internal/Actions/Edit.cs`

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| SaveCell | 454 | `async Task SaveCell(bool ForceSave = false, bool isDelete = false, bool isEscapeKey = false, bool focusLastGridCell = false, bool focusFirstCellOnShiftTab = false)` | Save edited cell with events |
| BeginEdit | 350 | Similar pattern | Enter edit mode on cell |
| EndEdit | Line 454+ | Inside SaveCell | Exit edit mode |

**Event Hooks**:
```csharp
// BEFORE save (cancellable)
CellSaveArgs<T> args = new CellSaveArgs<T>()
{
    ColumnName = OriginalCell!.Column!.Field,    // Field name
    Value = EditedValue!,                        // New value
    PreviousValue = PreviousVal!,                // Old value
    RowData = (T)OriginalRow.Data!,              // Original row
    Data = (T)CloneData!,                        // Modified clone
    IsForeignKey = OriginalCell.Column.IsForeignColumn(),
    Column = OriginalCell.Column,
    Cancel = false
};

await SfBaseUtils.InvokeEvent<CellSaveArgs<T>>(
    Parent.GridEvents?.OnCellSave,   // ← HOOK POINT 1
    args
).ConfigureAwait(true);

if (args.Cancel) return;  // ← Can cancel here

// AFTER save (non-cancellable)
var cellSavedArgs = new CellSavedArgs<T>() { ... };
await SfBaseUtils.InvokeEvent<CellSavedArgs<T>>(
    Parent.GridEvents?.CellSaved,    // ← HOOK POINT 2
    cellSavedArgs!
).ConfigureAwait(true);
```

**State Updates**:
```csharp
// Dirty tracking update
OriginalRow.IsDirty = GridUtils.CompareValues<object>(
    args.PreviousValue, 
    args.Value
);
OriginalRow.EditedData = OriginalRow.IsDirty ? CloneData! : null!;
OriginalCell.IsDirty = OriginalRow.IsDirty ? !OriginalCell.IsDirty ? ... : true;
```

### 5. Batch Operations

**File**: `src/Internal/Actions/Edit.cs`

#### BulkAddRow (Add Row)

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| BulkAddRow | 697 | `async Task BulkAddRow(object data = null!)` | Add new row in batch mode |

**Event**:
```csharp
var args = new BeforeBatchAddArgs<T>()
{
    DefaultData = (T)CloneData!,
    PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync())?.ToArray()!,
    Cancel = false,
    EditContext = EditContext,
    Index = 0,
    Parent = Parent
};

await SfBaseUtils.InvokeEvent<BeforeBatchAddArgs<T>>(
    Parent.GridEvents?.OnBatchAdd,  // ← HOOK POINT
    args
).ConfigureAwait(true);

if (args.Cancel) return;
```

**State**:
```csharp
Row.Action = EditAction.Added;        // Mark as added
Row.EditedData = CloneData;            // Store edited data
Row.IsDirty = true;                    // Mark dirty
row.IsAddedTop = true;                 // OR IsAddedBottom = true
Parent.Rows.Insert(index, row);        // Add to collection
```

#### BulkDelete (Delete Row)

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| BulkDelete | 958 | `async Task BulkDelete(string Field, object data, bool isDelete = false)` | Mark row for deletion in batch mode |

**Event**:
```csharp
var args = new BeforeBatchDeleteArgs<T>()
{
    PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync())?.ToArray()!,
    RowIndex = (int)(dataRow.Index ?? Parent.SelectionModule?.SelectedRow()?.Index ?? -1),
    RowData = (T)data! ?? Parent.SelectedRecords[0],
    Cancel = false,
    Parent = Parent
};

await SfBaseUtils.InvokeEvent<BeforeBatchDeleteArgs<T>>(
    Parent.GridEvents?.OnBatchDelete,  // ← HOOK POINT
    args
).ConfigureAwait(true);

if (args.Cancel) return;
```

**State**:
```csharp
row.IsDirty = true;                    // Mark dirty
row.Action = EditAction.Deleted;       // Mark for deletion
row.Cells?.ForEach(_ => _.IsDirty = true);
```

#### BatchSave (Commit)

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| BatchSave | 764 | `async Task BatchSave()` | Save all batch changes to database |

**Event**:
```csharp
var batchChanges = GetBatchChanges();  // Returns BatchChanges<T>

var args = new BeforeBatchSaveArgs<T>()
{
    BatchChanges = batchChanges,
    Cancel = false,
    Parent = Parent
};

if (Parent.GridEvents?.OnBatchSave.HasDelegate == true)
{
    await Parent.GridEvents.OnBatchSave.InvokeAsync(args)
        .ConfigureAwait(true);  // ← HOOK POINT
    
    if (args.Cancel) return;
}

// Actual save
var ChangesUpdated = await Parent.DataModule!.SaveChanges(
    batchChanges,
    PrimaryKey!
).ConfigureAwait(true);
```

**BatchChanges Structure**:
```csharp
public class BatchChanges<T>
{
    public List<T> AddedRecords { get; set; }      // Rows with EditAction.Added
    public List<T> ChangedRecords { get; set; }    // Rows with EditAction.Edited
    public List<T> DeletedRecords { get; set; }    // Rows with EditAction.Deleted
}
```

#### BatchClose (Discard)

| Method | Line | Signature | Purpose |
|--------|------|-----------|---------|
| BatchClose | 985 | `async Task BatchClose(bool escapeKey = false)` | Discard unsaved batch changes |

**Event**:
```csharp
var cancelArgs = new BeforeBatchCancelArgs<T>()
{
    BatchChanges = GetBatchChanges(),
    Cancel = false,
    Parent = Parent
};

if (Parent.GridEvents?.OnBatchCancel.HasDelegate == true)
{
    await Parent.GridEvents.OnBatchCancel.InvokeAsync(cancelArgs)
        .ConfigureAwait(true);  // ← HOOK POINT
    
    if (cancelArgs.Cancel) return;
}
```

**State Rollback**:
```csharp
if (!escapeKey && Parent.Rows != null)
{
    Parent.Rows.RemoveAll(_ => _.IsAddedTop || _.IsAddedBottom);
    
    foreach (var row in Parent.Rows)
    {
        row.EditedData = null!;
        row.IsDirty = false;
        row.Action = EditAction.None;
        row.Cells?.ForEach(_ => { _.IsDirty = false; });
    }
}
```

---

## Event Models Reference

### ActionEventArgs<T>
**Location**: `src/EventModels/Grids.cs:6123`

```csharp
public class ActionEventArgs<T> : GridEventBaseArgs
{
    public string? Action { get; internal set; }           // "Add", "Edit", "Delete"
    public bool Cancel { get; set; }                       // Set to true to cancel
    public T? Data { get; set; }                           // Modified row data
    public T? RowData { get; set; }                        // Original row data
    public Action RequestType { get; internal set; }       // Action.Add, .Edit, .Delete, etc.
    public int? Index { get; internal set; }               // Row index
    public string? Type { get; internal set; }             // "ActionBegin" or "ActionComplete"
    public EditContext? EditContext { get; set; }          // Validation context
    public SfGrid<T>? Parent { get; internal set; }        // Parent grid reference
}
```

### CellSaveArgs<T> / CellSavedArgs<T>
**Location**: `src/EventModels/Grids.cs:1188`

```csharp
public class CellSaveArgs<T> : CellSavedArgs<T>
{
    public bool Cancel { get; set; }  // Set to true to cancel save
}

public class CellSavedArgs<T> : GridEventBaseArgs
{
    public string? ColumnName { get; internal set; }       // Field name
    public object? PreviousValue { get; internal set; }    // Old value
    public object? Value { get; set; }                     // New value
    public T? RowData { get; internal set; }               // Original row
    public T? Data { get; set; }                           // Modified row
    public GridColumn? Column { get; internal set; }       // Column metadata
    public bool IsForeignKey { get; internal set; }        // Is foreign key column?
    public CellDOM? CellInfo { get; internal set; }        // DOM info
    public SfGrid<T>? Parent { get; internal set; }        // Parent grid
}
```

### BatchChanges<T>
**Location**: `src/EventModels/Grids.cs`

```csharp
public class BatchChanges<T>
{
    public List<T> AddedRecords { get; set; }
    public List<T> ChangedRecords { get; set; }
    public List<T> DeletedRecords { get; set; }
}
```

---

## EditAction Enum
**Location**: `src/Enumeration/GridsEnumerations.cs:1153`

```csharp
public enum EditAction
{
    [EnumMember(Value = "None")]
    None,           // No action

    [EnumMember(Value = "Edited")]
    Edited,         // Row has been edited

    [EnumMember(Value = "Deleted")]
    Deleted,        // Row marked for deletion

    [EnumMember(Value = "Added")]
    Added           // Row added in batch mode
}
```

---

## GridEditSettings Configuration
**Location**: `src/GridEditSettings.cs`

```csharp
[Parameter] public EditMode Mode { get; set; } = EditMode.Normal;
    // Expected values: Normal, Dialog, Batch

[Parameter] public bool AllowAdding { get; set; }
[Parameter] public bool AllowEditing { get; set; }
[Parameter] public bool AllowDeleting { get; set; }
[Parameter] public bool AllowEditOnDblClick { get; set; } = true;
[Parameter] public bool AllowEditOnSingleClick { get; set; }
[Parameter] public bool AllowNextRowEdit { get; set; }

[Parameter] public NewRowPosition NewRowPosition { get; set; } = NewRowPosition.Top;
    // Expected values: Top, Bottom

// PROPOSED for UndoRedo:
[Parameter] public bool EnableUndoRedo { get; set; } = false;
[Parameter] public int UndoRedoLimit { get; set; } = 100;
```

---

## Grid Components & Modules

**Main Grid**: `src/SfGrid.razor.cs`

```csharp
public class SfGrid<T> : SfBaseComponent
{
    // Critical modules
    internal Edit<T>? EditModule { get; set; }
    internal FocusHandler<T>? FocusModule { get; set; }
    
    // Event propagation
    internal EventAggregator? EventAggregator { get; set; }
    
    // Event handlers
    public GridEvents<T>? GridEvents { get; set; }
    
    // Configuration
    public GridEditSettings? EditSettings { get; set; }
    
    // State
    public bool IsEdit { get; set; }
    public bool IsAdd { get; set; }
    public List<Row<object>>? Rows { get; set; }
    public List<T>? CurrentViewData { get; set; }
    
    // Data module
    internal GridDataModule<T>? DataModule { get; set; }
    
    // Focus state
    public int? SelectedRowIndex { get; set; }
    public int? SelectedCellIndex { get; set; }
}
```

---

## Event Aggregator Pattern

**Usage**:
```csharp
// Subscribe to event
_parent.EventAggregator.Add("EventName", (args) => {
    // Handle event
});

// Trigger event
Parent.EventAggregator.Trigger("RowStateChanged", row);

// Async notify
await Parent.EventAggregator.NotifyAsync("CellSave", args);
```

**Common Events**:
```
"RowStateChanged"       → Row updated
"ContentStateChanged"   → Content refresh needed
"ToolbarStateChanged"   → Toolbar state changed
"HeaderStateChanged"    → Header refresh needed
"ActionBegin"          → Action started
"ActionComplete"       → Action finished
"CellSave"             → Cell about to save
"CellSaved"            → Cell saved
"BatchAdd"             → Row added to batch
"BatchDelete"          → Row deleted from batch
"BatchSave"            → Batch saved
"BatchCancel"          → Batch cancelled
```

---

## Common Integration Pattern

```csharp
// Hook into GridEvents
<SfGrid DataSource="Orders" AllowSelection="true">
    <GridEvents TValue="Orders" 
        OnActionBegin="OnActionBegin"
        OnCellSave="OnCellSave"
        CellSaved="OnCellSaved"
        OnBatchAdd="OnBatchAdd"
        OnBatchDelete="OnBatchDelete"
        OnBatchSave="OnBatchSave"
        OnBatchCancel="OnBatchCancel">
    </GridEvents>
    
    <GridEditSettings Mode="EditMode.Batch"
        AllowAdding="true"
        AllowEditing="true"
        AllowDeleting="true">
    </GridEditSettings>
    
    <!-- Columns... -->
</SfGrid>

@code {
    private async Task OnActionBegin(ActionEventArgs<Orders> args)
    {
        if (args.RequestType == Action.Add)
        {
            // UndoRedo: Record addition
        }
    }
    
    private async Task OnCellSave(CellSaveArgs<Orders> args)
    {
        // UndoRedo: Record cell change
        // args.Cancel = true to prevent save
    }
    
    private async Task OnCellSaved(CellSavedArgs<Orders> args)
    {
        // UndoRedo: Confirm cell saved
    }
    
    private async Task OnBatchSave(BeforeBatchSaveArgs<Orders> args)
    {
        // UndoRedo: Record batch commit
        // args.Cancel = true to prevent save
    }
}
```

---

## Dirty Tracking Queries

```csharp
// Get all dirty rows
var dirtyRows = Parent.Rows?.Where(r => r.IsDirty).ToList();

// Get all dirty cells in a row
var dirtyCells = row.Cells?.Where(c => c.IsDirty).ToList();

// Get modified data
var modifiedData = row.EditedData;  // Non-null if dirty

// Get row action
var action = row.Action;  // None, Added, Edited, or Deleted

// Check batch state
bool hasChanges = Parent.Rows?.Any(r => r.IsDirty) ?? false;

// Get batch changes
var batchChanges = EditModule?.GetBatchChanges();
```

---

## Performance Considerations

1. **Memory**: UndoRedoLimit default 100 (configurable)
2. **Cloning**: Deep clone row data for undo states
3. **Events**: Consider debouncing rapid cell edits
4. **Virtualization**: Reset virtual scroll on undo/redo
5. **Aggregates**: Refresh aggregates post-undo/redo

---

## Testing Checklist

- [ ] Undo single cell edit
- [ ] Redo single cell edit
- [ ] Undo row addition
- [ ] Redo row addition
- [ ] Undo row deletion
- [ ] Redo row deletion
- [ ] Undo batch save
- [ ] Redo batch save
- [ ] Undo with validation errors
- [ ] Keyboard Ctrl+Z / Ctrl+Y
- [ ] Stack limit behavior
- [ ] Focus restoration after undo
- [ ] Selection restoration after undo
- [ ] Aggregate refresh after undo
- [ ] Foreign key column undo
- [ ] Computed column undo
- [ ] Detail row editing undo
- [ ] Grouping + undo interaction
- [ ] Virtual scroll + undo interaction
- [ ] Frozen columns + undo interaction

---

## Common Pitfalls

1. **Not capturing state before modification** → Store deep clone
2. **Forgetting to restore all UI state** → Save focus, selection, scroll
3. **Not handling validation state** → Store EditContext state
4. **Ignoring foreign key columns** → Deep clone with resolved values
5. **Not refreshing aggregates** → Call ReactiveAggregateModule
6. **Not updating row indices** → Recalculate after add/delete
7. **Ignoring event cancellation** → Check args.Cancel in events
8. **Not handling empty undo/redo stacks** → Graceful degradation
9. **Modifying row data in place** → Always work with clones
10. **Forgetting platform-specific keys** → Check IsMacDevice for Cmd key

---

**Last Updated**: August 12, 2026  
**API Version**: Aligned with Blazor DataGrid v24+
