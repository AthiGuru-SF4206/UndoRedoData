# Syncfusion Blazor DataGrid - Infrastructure Analysis for UndoRedo Integration

**Generated**: August 12, 2026  
**Workspace**: `d:\Blazor-Source-Project\UndoAndRedo\blazor-grid-components`

---

## Executive Summary

This document provides a **comprehensive, line-by-line analysis** of the Syncfusion Blazor DataGrid infrastructure required to implement UndoRedo functionality. It maps keyboard navigation, batch editing, event flows, and configuration structures to enable seamless undo/redo capability.

### Key Findings
- ✅ Robust keyboard event routing through `GridJSInteropAdaptor` → `FocusHandler`
- ✅ Clean dirty-tracking mechanism on cells/rows via `IsDirty` + `EditedData`
- ✅ Event-driven architecture with pre/post-action hooks via `GridEvents`
- ✅ Batch edit infrastructure with `EditAction` enum (Added/Edited/Deleted)
- ✅ `GridEditSettings` ready for configuration extension

---

## 1. KEYBOARD NAVIGATION ROUTING FLOW

```
JavaScript Layer (sfBlazor.Grid)
        ↓
        ↓ (keyboard event)
        ↓
GridJSInteropAdaptor<T>.GridKeyDown()
    Location: src/Internal/Base/GridJSInteropAdaptor.cs:585
    @JSInvokable decoration
    
        ↓
        ↓ Deserialize KeyboardEventArgs
        ↓
FocusHandler<T>.ProcessGridKeyDown()
    Location: src/Internal/Actions/FocusHandler.cs:589
    
        ↓
        ├─→ Template Column Focus (rowIndex != null && templateCellIndex != null)
        ├─→ Inline Edit Navigation (Tab/ShiftTab between cells)
        ├─→ Filter Bar/Add Form Focus
        └─→ ProcessKeyCombination()
                ↓
                ├─→ Tab/ShiftTab: EditCell or SaveCell()
                ├─→ F2: EditCell()
                ├─→ Escape: Cancel Edit
                ├─→ Enter: SaveCell()
                ├─→ Ctrl+Z: [UndoRedo Hook]
                └─→ Ctrl+Y: [UndoRedo Hook]
```

### Entry Point Analysis

**File**: `src/Internal/Base/GridJSInteropAdaptor.cs` (Lines 585-596)

```csharp
[JSInvokable]
public async ValueTask GridKeyDown(
    object args,                          // JSON KeyboardEventArgs
    bool value,                           // Unused
    bool isPagerFocused,                  // Pager has focus
    bool isToolbarFocused,                // Toolbar has focus
    int? cellIndex,                       // Current cell column index
    int? rowIndex = null,                 // Current cell row index
    int? templateCellIndex = null,        // Template cell column index
    bool focusColumnTemplate = false,     // Focus in template
    bool isMultiSelectPopUpOpened = false // Multi-select popup state
)
{
    // JSON → KeyboardEventArgs deserialization
    KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(
        args?.ToString()!, 
        _jsonSettings  // PropertyNameCaseInsensitive = true
    );
    
    if (Parent.FocusModule != null)
    {
        Parent.FocusModule.isMultiSelectPopUpOpened = isMultiSelectPopUpOpened;
        
        // Delegate to FocusHandler
        await Parent.FocusModule.ProcessGridKeyDown(
            action!,           // KeyboardEventArgs
            null!,             // BeforeCellFocus (optional)
            isPagerFocused,    // Boolean
            isToolbarFocused,  // Boolean
            cellIndex,         // int?
            rowIndex,          // int?
            templateCellIndex, // int?
            focusColumnTemplate // bool
        ).ConfigureAwait(true);
    }
}
```

---

## 2. KEYBOARD EVENT PROCESSING IN FOCUSHANDLER

### ProcessGridKeyDown() - Main Entry (Line 589)

**Signature**:
```csharp
internal async Task ProcessGridKeyDown(
    KeyboardEventArgs e,
    BeforeCellFocus? bf = null,
    bool isPagerFocused = true,
    bool isToolbarFocused = false,
    int? cellIndex = null,
    int? rowIndex = null,
    int? templateCellIndex = null,
    bool focusColumnTemplate = false
)
```

**Key Steps**:

1. **Extract key combination** (Line 591):
   ```csharp
   var keyCombination = e.GetKeyCombination(isMacDevice: _parent.IsMacDevice ?? false);
   ```
   Returns: `"Tab"`, `"ShiftTab"`, `"Enter"`, `"Escape"`, `"F2"`, `"ArrowUp"`, etc.

2. **Handle template column focus** (Lines 602-626):
   - If `focusColumnTemplate=true` and `templateCellIndex != null`
   - Routes focus from template back to main grid

3. **Handle inline edit Tab/ShiftTab** (Lines 629-649):
   - When in Normal edit mode (`EditSettings.Mode == EditMode.Normal`)
   - `IsEdit=true` and Tab/ShiftTab pressed
   - Navigates between editable cells in current row
   - Saves cell before moving to next

4. **Process key combination** (Line 757):
   ```csharp
   await ProcessKeyCombination(keyCombination, tPage, bf, isPagerFocused, e)
       .ConfigureAwait(true);
   ```

### ProcessKeyDown() - Cell-Level Handler (Line 230)

**Signature**:
```csharp
internal async Task ProcessKeyDown(
    KeyboardEventArgs e,
    Row<object> row,
    Cell<object> cell,
    bool isHeader = false
)
```

**Key Steps**:

1. **Extract key combination**:
   ```csharp
   string keyCombination = ...;
   ```

2. **Get action from settings** (Line 302):
   ```csharp
   string[] actions = _settings?.GetAction(keyCombination) ?? Array.Empty<string>();
   ```
   Maps keyCombination to grid actions (e.g., "CtrlA" → "SelectAll")

3. **Navigation handling** (Lines 310-450):
   - `"CtrlHome"`: First row, first cell
   - `"CtrlEnd"`: Last row, last cell
   - `"Home"`: First data cell in row
   - `"End"`: Last data cell in row
   - `"ArrowUp"` / `"ArrowDown"`: Move up/down
   - `"ArrowLeft"` / `"ArrowRight"`: Move left/right
   - `"Space"`: Toggle checkbox or select row
   - `"AltDown"`: Open column menu / filter dropdown

4. **Call MoveFocusCell()** (Line 456):
   ```csharp
   await MoveFocusCell(actions!, row!, cell!, e, keyCombination)
       .ConfigureAwait(true);
   ```
   Implements movement logic and focuses new cell

---

## 3. KEY DETECTION INFRASTRUCTURE

### Utils.cs Helper Methods (Lines 700+)

All methods extend `KeyboardEventArgs`:

```csharp
// Modifier combinations
e.IsCtrlA()        // Ctrl+A (Select All)
e.IsCtrlC()        // Ctrl+C (Copy)
e.IsCtrlP()        // Ctrl+P (Print)
e.IsCtrlZ()        // Ctrl+Z (UNDO TRIGGER) ← Hook here
e.IsCtrlY()        // Ctrl+Y (REDO TRIGGER) ← Hook here

// Function keys
e.IsF2()           // F2 (Edit)
e.IsDelete()       // Delete
e.IsEsc()          // Escape

// Navigation keys
e.IsHome()         // Home
e.IsCtrlHome()     // Ctrl+Home
e.IsEnd()          // End
e.IsCtrlEnd()      // Ctrl+End
e.IsPageUp()       // Page Up
e.IsPageDown()     // Page Down

// Arrow keys with modifiers
e.IsUpArrow()      // Arrow Up
e.IsShiftUp()      // Shift+Arrow Up
e.IsCtrlUp()       // Ctrl+Arrow Up
e.IsAltUp()        // Alt+Arrow Up
// ... similar for Down, Left, Right

// Tab navigation
e.IsTab()          // Tab
e.IsShiftTab()     // Shift+Tab

// Special shortcuts
e.IsAltW()         // Alt+W (Close)
e.IsMetaP()        // Cmd+P (Mac)
```

**Integration Point for UndoRedo**:
```csharp
// In ProcessKeyCombination() or ProcessKeyDown():
if (keyCombination == "CtrlZ")
{
    // Hook: Call UndoRedoManager.Undo()
    await Parent.UndoRedoManager?.UndoAsync().ConfigureAwait(true);
    return;
}
else if (keyCombination == "CtrlY")
{
    // Hook: Call UndoRedoManager.Redo()
    await Parent.UndoRedoManager?.RedoAsync().ConfigureAwait(true);
    return;
}
```

---

## 4. BATCH EDIT OPERATIONS FLOW

### SaveCell() - Core Edit Operation (Line 454)

**Location**: `src/Internal/Actions/Edit.cs`

**Signature**:
```csharp
internal async Task SaveCell(
    bool ForceSave = false,               // Skip validation
    bool isDelete = false,                // Deletion context
    bool isEscapeKey = false,             // Escape pressed
    bool focusLastGridCell = false,       // Focus last cell after save
    bool focusFirstCellOnShiftTab = false // Focus first cell after save
)
```

**State Transitions**:

```
BEFORE SaveCell():
  Cell.IsEdit = true
  OriginalRow.EditedData = CloneData
  
DURING SaveCell():
  1. Validate if !ForceSave
  2. Compare Value vs PreviousValue
  3. Fire OnCellSave event (CANCELLABLE)
     ↓ Can be intercepted by UndoRedoManager
  4. Update Cell.IsDirty
  5. Update OriginalRow.EditedData
  6. Fire CellSaved event (non-cancellable)
     ↓ UndoRedoManager records the change
  7. Calculate aggregates
  8. Navigate focus
  
AFTER SaveCell():
  Cell.IsEdit = false
  Cell.IsDirty = (oldValue != newValue)
  OriginalRow.IsDirty = (any cell in row is dirty)
  OriginalRow.EditedData = (non-null if dirty)
```

**Event Model - CellSaveArgs<T>** (Line 507-520):

```csharp
CellSaveArgs<T> args = new CellSaveArgs<T>()
{
    ColumnName = OriginalCell!.Column!.Field,      // Field name
    Value = EditedValue!,                          // New value
    PreviousValue = PreviousVal!,                  // Old value
    RowData = (T)OriginalRow.Data!,                // Original row
    Cancel = false,                                // Cancellation flag
    Data = (T)CloneData!,                          // Modified row clone
    IsForeignKey = OriginalCell.Column.IsForeignColumn(),
    Column = OriginalCell.Column,                  // Column metadata
    Parent = Parent
};

// Fire pre-save event (CANCELLABLE)
await SfBaseUtils.InvokeEvent<CellSaveArgs<T>>(
    Parent.GridEvents?.OnCellSave, 
    args
).ConfigureAwait(true);

if (args.Cancel)
{
    return;  // ← Exit point for cancellation
}

// ... save logic ...

// Fire post-save event (NON-CANCELLABLE)
await SfBaseUtils.InvokeEvent<CellSavedArgs<T>>(
    Parent.GridEvents?.CellSaved, 
    cellSavedArgs!
).ConfigureAwait(true);
```

**UndoRedo Hook Point**:
```csharp
// In GridEvents.OnCellSave handler:
await SfBaseUtils.InvokeEvent<CellSaveArgs<T>>(
    Parent.GridEvents?.OnCellSave,  // ← Hook here
    args
).ConfigureAwait(true);

// In GridEvents.CellSaved handler:
await SfBaseUtils.InvokeEvent<CellSavedArgs<T>>(
    Parent.GridEvents?.CellSaved,   // ← And here
    cellSavedArgs!
).ConfigureAwait(true);
```

---

### BulkAddRow() - Batch Row Addition (Line 697)

**Signature**:
```csharp
private async Task BulkAddRow(object data = null!)
```

**Flow**:

```
1. SaveCell()  ← Save current edit if active
2. GetModelGenerator(true)  ← Create empty row model
3. Fire OnBatchAdd event (BeforeBatchAddArgs) ← CANCELLABLE
   ├─ DefaultData: T (new row)
   ├─ PrimaryKey: string[] (primary key fields)
   └─ Index: int (0 by default)
4. Add to Parent.Rows
   ├─ If NewRowPosition.Top: Insert at index 0
   └─ If NewRowPosition.Bottom: Add to end
5. Set row state:
   ├─ Row.Action = EditAction.Added
   ├─ Row.EditedData = CloneData
   ├─ Row.IsDirty = true
   └─ Row.IsAddedTop/IsAddedBottom = true
6. EditCell() ← Enter edit on first editable cell
7. Trigger state changes
   ├─ "ToolbarStateChanged"
   ├─ "ContentStateChanged"
   └─ Select row
```

**Event Model - BeforeBatchAddArgs**:
```csharp
var args = new BeforeBatchAddArgs<T>()
{
    DefaultData = (T)CloneData!,
    PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync()
        .ConfigureAwait(true))?.ToArray()!,
    Cancel = false,
    EditContext = EditContext,
    Index = 0,
    Parent = Parent
};

await SfBaseUtils.InvokeEvent<BeforeBatchAddArgs<T>>(
    Parent.GridEvents?.OnBatchAdd, 
    args  // ← Can set Cancel=true
).ConfigureAwait(true);
```

**UndoRedo Hook Point**:
- Hook after row added to `Parent.Rows` with `EditAction.Added`
- Record: `{ action: "RowAdded", rowData: T, index: int, originalCount: int }`

---

### BulkDelete() - Batch Row Deletion (Line 958)

**Signature**:
```csharp
private async Task BulkDelete(string Field, object data, bool isDelete = false)
```

**Flow**:

```
1. SaveCell(true, isDelete) ← Force save current edit
2. Find row by primary key match
3. Fire OnBatchDelete event (BeforeBatchDeleteArgs) ← CANCELLABLE
   ├─ PrimaryKey: string[]
   ├─ RowIndex: int
   ├─ RowData: T (row to delete)
   └─ Cancel: bool
4. If !args.Cancel:
   a. Set row state:
      ├─ Row.Action = EditAction.Deleted
      ├─ Row.IsDirty = true
      └─ Row.Cells: ForEach(_.IsDirty = true)
   b. Update HasBatchChanges = true
   c. Trigger "ToolbarStateChanged"
   d. SelectRowAsync(nextIndex)
```

**Event Model - BeforeBatchDeleteArgs**:
```csharp
var args = new BeforeBatchDeleteArgs<T>()
{
    PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync()
        .ConfigureAwait(true))?.ToArray()!,
    RowIndex = (int)(dataRow.Index ?? -1),
    RowData = (T)data! ?? Parent.SelectedRecords[0],
    Cancel = false,
    Parent = Parent
};

await SfBaseUtils.InvokeEvent<BeforeBatchDeleteArgs<T>>(
    Parent.GridEvents?.OnBatchDelete, 
    args  // ← Can set Cancel=true
).ConfigureAwait(true);
```

**UndoRedo Hook Point**:
- Hook after `EditAction.Deleted` assignment
- Record: `{ action: "RowDeleted", rowData: T, index: int }`

---

### BatchSave() - Commit Changes (Line 764)

**Signature**:
```csharp
internal async Task BatchSave()
```

**Flow**:

```
1. SaveCell() ← Save current edit
2. GetBatchChanges() → BatchChanges<T>
   ├─ AddedRecords: List<T> (rows with EditAction.Added)
   ├─ ChangedRecords: List<T> (rows with EditAction.Edited)
   └─ DeletedRecords: List<T> (rows with EditAction.Deleted)
3. Fire OnBatchSave event (BeforeBatchSaveArgs) ← CANCELLABLE
   ├─ BatchChanges: (above)
   └─ Cancel: bool
4. If !args.Cancel:
   a. Call Parent.DataModule.SaveChanges(BatchChanges, PrimaryKey)
   b. On success:
      ├─ HasBatchChanges = false
      ├─ Clear all IsDirty flags
      ├─ Clear EditedData
      ├─ Clear IsAddedTop/IsAddedBottom
      └─ DataProcess() ← Reload/refresh
```

**Event Model - BeforeBatchSaveArgs**:
```csharp
var args = new BeforeBatchSaveArgs<T>()
{
    BatchChanges = BatchChanges,  // Contains Added/Changed/Deleted
    Cancel = false,
    Parent = Parent
};
```

**UndoRedo Hook Point**:
- Hook at `OnBatchSave` before `DataModule.SaveChanges()`
- Record: `{ action: "BatchSaved", changes: BatchChanges<T>, timestamp: DateTime }`

---

### BatchClose() - Discard Changes (Line 985)

**Signature**:
```csharp
internal async Task BatchClose(bool escapeKey = false)
```

**Flow**:

```
1. GetBatchChanges()
2. Fire OnBatchCancel event (BeforeBatchCancelArgs) ← CANCELLABLE
3. If !escapeKey:
   a. Remove all rows with IsAddedTop/IsAddedBottom
   b. ForEach(row in Rows):
      ├─ row.EditedData = null
      ├─ row.IsDirty = false
      ├─ row.Action = EditAction.None
      └─ row.Cells: ForEach(cell.IsDirty = false)
   c. HasBatchChanges = false
4. Trigger "ToolbarStateChanged"
```

**UndoRedo Hook Point**:
- UndoRedo doesn't typically need to record cancellations
- But should be notified to clear action recording

---

## 5. DIRTY TRACKING MECHANISM

### Dirty Flag States

```
Cell Level:
  Cell.IsDirty = false  → No edits
  Cell.IsDirty = true   → Value changed vs original

Row Level:
  Row.IsDirty = false  → All cells unchanged AND (Action == None OR Action == Deleted)
  Row.IsDirty = true   → Any cell dirty OR Action in (Added, Deleted, Edited)

EditedData:
  Row.EditedData = null     → IsDirty = false
  Row.EditedData = CloneData → IsDirty = true (contains modified values)
```

### Dirty Tracking Example

**From SaveCell() (Lines 524-532)**:

```csharp
// Compare new vs old
if (!OriginalRow.Cells.Any(_ => _.IsDirty))
{
    // No cells are dirty yet, so mark row based on comparison
    OriginalRow.IsDirty = GridUtils.CompareValues<object>(
        args.PreviousValue,  // Old value
        args.Value           // New value
    );
}

// Update EditedData based on IsDirty
OriginalRow.EditedData = OriginalRow.IsDirty ? CloneData! : null!;
```

**Row State Summary**:

```csharp
public class Row<object>
{
    public object Data { get; set; }                    // Original data
    public object EditedData { get; set; }              // Modified data (if IsDirty)
    public bool IsDirty { get; set; }                   // Has changes?
    public EditAction Action { get; set; }              // None/Added/Edited/Deleted
    public List<Cell<object>> Cells { get; set; }       // Cell collection
    // Each cell has: Cell.IsDirty, Cell.IsEdit, Cell.Column
}
```

---

## 6. EVENT-DRIVEN ARCHITECTURE

### GridEvents<TValue> Event Declarations

**Location**: `src/GridEvents.cs`

#### Action Events

**OnActionBegin** (Line 835):
```csharp
public EventCallback<ActionEventArgs<TValue>> OnActionBegin { get; set; }

// Fired BEFORE:
//  - Add: AddRecord()
//  - Edit: EditCell()
//  - Delete: DeleteRecord()
//  - Cancel: BatchClose()
//
// Properties:
//  - RequestType: Action.Add | Action.Edit | Action.Delete | Action.Cancel
//  - Action: string ("Add" | "Edit" | "Delete")
//  - Data: T (current row)
//  - Index: int (row index)
//  - Cancel: bool (can prevent action)
```

**OnActionComplete** (Line 862):
```csharp
public EventCallback<ActionEventArgs<TValue>> OnActionComplete { get; set; }

// Fired AFTER action completes
// Properties same as OnActionBegin
// NOTE: Cannot prevent action (already committed)
```

#### Cell Save Events

**OnCellSave** (Line 1189):
```csharp
public EventCallback<CellSaveArgs<TValue>> OnCellSave { get; set; }

// Fired BEFORE cell value is saved
// Properties:
//  - ColumnName: string (field name)
//  - Value: object (new value)
//  - PreviousValue: object (old value)
//  - RowData: TValue (original row)
//  - Data: TValue (modified row)
//  - Column: GridColumn (metadata)
//  - Cancel: bool (can prevent save)
```

**CellSaved** (Line 1216):
```csharp
public EventCallback<CellSavedArgs<TValue>> CellSaved { get; set; }

// Fired AFTER cell is saved successfully
// Properties same as OnCellSave (no Cancel property)
```

#### Batch Edit Events

```csharp
public EventCallback<BeforeBatchAddArgs<TValue>> OnBatchAdd { get; set; }
public EventCallback<BeforeBatchDeleteArgs<TValue>> OnBatchDelete { get; set; }
public EventCallback<BeforeBatchSaveArgs<TValue>> OnBatchSave { get; set; }
public EventCallback<BeforeBatchCancelArgs<TValue>> OnBatchCancel { get; set; }
```

---

## 7. EDITSETTINGS CONFIGURATION

### GridEditSettings Component

**Location**: `src/GridEditSettings.cs`

**Current Properties**:

```csharp
[Parameter] public bool AllowAdding { get; set; }
[Parameter] public bool AllowDeleting { get; set; }
[Parameter] public bool AllowEditing { get; set; }
[Parameter] public bool AllowEditOnDblClick { get; set; } = true
[Parameter] public bool AllowEditOnSingleClick { get; set; }
[Parameter] public bool AllowNextRowEdit { get; set; }

[Parameter] public EditMode Mode { get; set; } = EditMode.Normal
    // Normal, Dialog, Batch

[Parameter] public NewRowPosition NewRowPosition { get; set; } = NewRowPosition.Top
    // Top, Bottom

[Parameter] public bool ShowAddNewRow { get; set; }
[Parameter] public bool ShowConfirmDialog { get; set; } = true
[Parameter] public bool ShowDeleteConfirmDialog { get; set; }

[Parameter] public DialogSettings? Dialog { get; set; }
[Parameter] public RenderFragment<object>? Template { get; set; }
[Parameter] public RenderFragment<object>? HeaderTemplate { get; set; }
[Parameter] public RenderFragment<object>? FooterTemplate { get; set; }
[Parameter] public RenderFragment<object>? Validator { get; set; }
```

**Proposed Addition for UndoRedo**:

```csharp
/// <summary>
/// Enables Undo/Redo functionality for batch editing operations.
/// </summary>
[Parameter]
public bool EnableUndoRedo { get; set; } = false;

/// <summary>
/// Maximum number of undo/redo steps to maintain in memory.
/// Default is 100. Set to 0 for unlimited (not recommended for large datasets).
/// </summary>
[Parameter]
public int UndoRedoLimit { get; set; } = 100;

/// <summary>
/// Current size of undo stack (read-only).
/// </summary>
[Parameter]
public int UndoStackSize { get; internal set; } = 0;

/// <summary>
/// Current size of redo stack (read-only).
/// </summary>
[Parameter]
public int RedoStackSize { get; internal set; } = 0;
```

---

## 8. ACTION ROUTING ARCHITECTURE

### Keyboard Shortcut → Action Mapping

```csharp
// In Utils.cs or GridKeyboardSettings
public static Dictionary<string, string[]> KeyActionMap = new()
{
    { "F2", new[] { "EditCell" } },
    { "Delete", new[] { "Delete" } },
    { "Escape", new[] { "CancelEdit" } },
    { "Enter", new[] { "SaveCell" } },
    { "ShiftEnter", new[] { "SaveCell", "MoveUpCell" } },
    { "Tab", new[] { "SaveCell", "MoveRightCell" } },
    { "ShiftTab", new[] { "SaveCell", "MoveLeftCell" } },
    { "Insert", new[] { "AddRow" } },  // Batch mode
    { "ArrowUp", new[] { "MoveUpCell" } },
    { "ArrowDown", new[] { "MoveDownCell" } },
    { "ArrowLeft", new[] { "MoveLeftCell" } },
    { "ArrowRight", new[] { "MoveRightCell" } },
    { "CtrlZ", new[] { "Undo" } },      // ← UndoRedo
    { "CtrlY", new[] { "Redo" } },      // ← UndoRedo
    { "CtrlA", new[] { "SelectAll" } },
};
```

**Expected Hook Pattern**:

```csharp
// In FocusHandler.ProcessKeyCombination()
string[] actions = GetAction(keyCombination);

foreach (var action in actions)
{
    switch(action)
    {
        case "Undo":
            await Parent.UndoRedoManager?.UndoAsync();
            break;
        case "Redo":
            await Parent.UndoRedoManager?.RedoAsync();
            break;
        case "EditCell":
            await EditCell(row, cell);
            break;
        case "SaveCell":
            await SaveCell();
            break;
        // ... etc
    }
}
```

---

## 9. INTEGRATION POINTS SUMMARY

### Priority 1: Critical Hooks

| # | Location | Method | Line | Hook Type | Purpose |
|----|----------|--------|------|-----------|---------|
| 1 | Edit.cs | SaveCell() | 507-520 | Event | Record cell save (pre/post) |
| 2 | Edit.cs | BulkAddRow() | 720-732 | Event | Record row addition |
| 3 | Edit.cs | BulkDelete() | 980-1000 | Event | Record row deletion |
| 4 | Edit.cs | BatchSave() | 764-800 | Event | Commit batch transaction |
| 5 | FocusHandler.cs | ProcessKeyCombination() | ~700 | KeyCheck | Intercept Ctrl+Z / Ctrl+Y |

### Priority 2: Supporting Integration

| # | Location | Method | Line | Hook Type | Purpose |
|----|----------|--------|------|-----------|---------|
| 6 | Edit.cs | BatchClose() | 985 | Event | Discard batch changes |
| 7 | Edit.cs | EditCell() | ~350 | Entry | Record edit start |
| 8 | GridJSInteropAdaptor.cs | GridKeyDown() | 585 | JSInvokable | Keyboard event entry |
| 9 | FocusHandler.cs | ProcessGridKeyDown() | 589 | Route | Route to key handler |
| 10 | GridEvents.cs | GridEvents<T> | - | Events | Declare all event handlers |

### Priority 3: Enhancements

| # | Location | Component | Enhancement |
|----|----------|-----------|-------------|
| 11 | GridEditSettings.cs | GridEditSettings | Add EnableUndoRedo, UndoRedoLimit |
| 12 | SfGrid.razor.cs | SfGrid | Inject UndoRedoManager instance |
| 13 | Toolbar/Commands | Various | Add Undo/Redo commands to toolbar |
| 14 | Keyboard | Utils.cs | Add IsCtrlZ(), IsCtrlY() helpers |

---

## 10. STATE CAPTURE REQUIREMENTS

### What to Capture on Each Action

#### Cell Edit
```csharp
{
    action: "CellEdit",
    rowIndex: int,
    cellIndex: int,
    fieldName: string,
    oldValue: object,
    newValue: object,
    timestamp: DateTime,
    previousRows: Row<T>[],  // For undo
}
```

#### Row Add (Batch)
```csharp
{
    action: "RowAdd",
    rowData: T,
    rowIndex: int,
    position: "Top" | "Bottom",
    timestamp: DateTime,
    previousRowCount: int,
}
```

#### Row Delete (Batch)
```csharp
{
    action: "RowDelete",
    rowData: T,
    rowIndex: int,
    originalRow: Row<T>,  // For undo
    timestamp: DateTime,
}
```

#### Batch Save
```csharp
{
    action: "BatchSave",
    addedCount: int,
    changedCount: int,
    deletedCount: int,
    changes: BatchChanges<T>,
    timestamp: DateTime,
    isCommitted: bool,
}
```

---

## 11. POTENTIAL CONFLICTS & MITIGATION

### Conflict 1: Dirty Flag Overlap
**Issue**: Grid uses `IsDirty` for existing features  
**Mitigation**: UndoRedo layers on top without modifying core flags

### Conflict 2: EditAction Enum
**Issue**: Only 4 values (None/Added/Edited/Deleted)  
**Mitigation**: Create separate `UndoRedoAction` enum

### Conflict 3: Virtual Scroll
**Issue**: Index changes when rows added/deleted  
**Mitigation**: Reset `VirtualScrollModule` indices on undo/redo

### Conflict 4: Aggregates
**Issue**: Aggregates recalculate on dirty changes  
**Mitigation**: Trigger aggregate refresh after undo/redo

### Conflict 5: Selection
**Issue**: Selected rows may not exist after undo  
**Mitigation**: Cache selection state with each action

### Conflict 6: Focus
**Issue**: Focused cell may not exist after undo  
**Mitigation**: Cache focus state; restore if row still exists

---

## 12. FILE REFERENCE CHECKLIST

✅ **Keyboard Navigation**
- [x] `src/Internal/Actions/FocusHandler.cs` - ProcessKeyDown (L230), ProcessGridKeyDown (L589)
- [x] `src/Internal/Base/GridJSInteropAdaptor.cs` - GridKeyDown (L585)
- [x] `src/Internal/Base/Utils.cs` - Key detection helpers (L700+)

✅ **Batch Edit Operations**
- [x] `src/Internal/Actions/Edit.cs` - SaveCell (L454), BulkAddRow (L697), BulkDelete (L958), BatchSave (L764), BatchClose (L985)

✅ **Events & Models**
- [x] `src/GridEvents.cs` - Event declarations (L835, L862, L1189, L1216)
- [x] `src/EventModels/Grids.cs` - ActionEventArgs (L6123), CellSaveArgs (L1188), CellSavedArgs (L1204)

✅ **Configuration**
- [x] `src/GridEditSettings.cs` - Edit configuration (L1+)

✅ **Enumerations**
- [x] `src/Enumeration/GridsEnumerations.cs` - EditAction (L1153)

✅ **Main Grid**
- [x] `src/SfGrid.razor.cs` - Grid component (EditModule, FocusModule, EventAggregator)

---

## Conclusion

The Syncfusion Blazor DataGrid has a **robust, event-driven architecture** well-suited for UndoRedo integration:

1. ✅ Clean keyboard routing through JS interop
2. ✅ Clear dirty-tracking at cell/row level
3. ✅ Event hooks (pre/post) for all CRUD operations
4. ✅ Batch edit infrastructure with `EditAction` tracking
5. ✅ Configuration framework ready for extension

**Next Steps**:
1. Create `UndoRedoManager<T>` class
2. Add properties to `GridEditSettings`
3. Hook events in `Edit.cs` methods
4. Intercept keyboard shortcuts in `FocusHandler`
5. Implement undo/redo operations with state restoration
6. Add toolbar commands and keyboard shortcuts

---

**Document Version**: 1.0  
**Generated**: August 12, 2026  
**Analysis Scope**: Comprehensive infrastructure mapping
