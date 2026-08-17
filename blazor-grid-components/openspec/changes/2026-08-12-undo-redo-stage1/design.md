# Undo/Redo Feature - Stage 1: Keyboard Infrastructure - Design Document

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│  Presentation Layer (Blazor Component)              │
│  - GridEditSettings (EnableUndoRedo, UndoRedoLimit) │
│  - SfGrid properties (UndoCount, IsUndoAvailable)   │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│  Business Logic Layer (.NET)                        │
│  - UndoRedoManager<T> (core history management)     │
│  - UndoRedoAction<T> (action model)                 │
│  - Edit<T> (integration hooks for SaveCell, etc.)   │
│  - FocusHandler (keyboard shortcut routing)         │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│  Event & Interop Layer                              │
│  - GridJSInteropAdaptor (keyboard event entry)      │
│  - GridKeyboardEventArgs (key combination parsing)  │
│  - GridEvents (event hooks for action recording)    │
└─────────────────────────────────────────────────────┘
```

## Component Design

### 1. Core Manager: `UndoRedoManager<T>`

**Location**: `src/Internal/Actions/UndoRedoManager.cs` (NEW FILE)

**Responsibilities**:
- Maintain Undo and Redo stacks using `LinkedList<T>` (O(1) FIFO eviction)
- Record actions (CellEdit, RowAdd, RowDelete, Paste, AutoFill)
- Execute undo operations (restore old state, move action to Redo stack)
- Execute redo operations (reapply new state, move action to Undo stack)
- Enforce stack size limits
- Provide stack statistics (count, available flags)

**Key Methods**:

```csharp
public class UndoRedoManager<T>
{
    // Configuration
    public int MaxStackSize { get; set; } = 20;
    public bool IsEnabled { get; private set; }

    // Stacks
    private LinkedList<UndoRedoAction<T>> UndoStack { get; set; }
    private LinkedList<UndoRedoAction<T>> RedoStack { get; set; }

    // Statistics (read-only public properties)
    public int UndoCount => UndoStack.Count;
    public int RedoCount => RedoStack.Count;
    public bool IsUndoAvailable => UndoStack.Count > 0;
    public bool IsRedoAvailable => RedoStack.Count > 0;

    // Core Operations
    public void RecordAction(UndoRedoAction<T> action)
    {
        // Add action to undo stack, enforce limit, clear redo stack
    }

    public async Task UndoAsync()
    {
        // Pop from undo, restore state, push to redo
    }

    public async Task RedoAsync()
    {
        // Pop from redo, reapply state, push to undo
    }

    public async Task UndoAllAsync()
    {
        // Undo all actions sequentially
    }

    public async Task RedoAllAsync()
    {
        // Redo all actions sequentially
    }

    public void Clear()
    {
        // Clear both stacks, release memory
    }

    public void Enable(int stackLimit = 20)
    {
        IsEnabled = true;
        MaxStackSize = stackLimit;
    }

    public void Disable()
    {
        IsEnabled = false;
        Clear();  // Clear stacks immediately
    }
}
```

---

### 2. Action Model: `UndoRedoAction<T>`

**Location**: `src/Models/UndoRedoAction.cs` (NEW FILE)

**Represents a single undo-able action**:

```csharp
public enum UndoRedoActionType
{
    CellEdit,      // Single cell value change
    RowAdd,        // New row added
    RowDelete,     // Row deleted
    Paste,         // Multi-cell paste (atomic)
    AutoFill       // Fill-handle pattern (atomic)
}

public class UndoRedoAction<T>
{
    // Metadata
    public UndoRedoActionType ActionType { get; set; }
    public int SequenceNumber { get; set; }  // For debugging

    // Cell Edit Data
    public CellChange<T>? CellChange { get; set; }

    // Row Data
    public T? RowData { get; set; }
    public int? RowIndex { get; set; }
    public NewRowPosition? RowPosition { get; set; }  // Top/Bottom for RowAdd

    // State snapshots
    public List<CellChange<T>>? PreviousValues { get; set; }  // For multi-cell actions
    public List<T>? PreviousRows { get; set; }                 // For paste/multi-row actions
}

public class CellChange<T>
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string? FieldName { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public GridColumn? Column { get; set; }
}
```

---

### 3. Integration: Hooks in `Edit<T>`

**Location**: `src/Internal/Actions/Edit.cs` (MODIFIED)

**Hook Points**:

#### A. SaveCell() - Record Cell Edits
```csharp
// After line 520 (CellSaved event fired):
if (Parent.EditSettings?.EnableUndoRedo == true && 
    Parent.EditSettings?.Mode == EditMode.Batch)
{
    var cellChange = new CellChange<object>
    {
        RowIndex = OriginalRow.Index ?? -1,
        ColumnIndex = OriginalCell.ColumnIndex ?? -1,
        FieldName = OriginalCell.Column?.Field,
        OldValue = args.PreviousValue,
        NewValue = args.Value,
        Column = OriginalCell.Column
    };

    var action = new UndoRedoAction<object>
    {
        ActionType = UndoRedoActionType.CellEdit,
        CellChange = cellChange,
        Timestamp = DateTime.Now
    };

    Parent.UndoRedoManager?.RecordAction(action);
}
```

#### B. BulkAddRow() - Record Row Additions
```csharp
// After line 732 (row added to Parent.Rows):
if (Parent.EditSettings?.EnableUndoRedo == true)
{
    var action = new UndoRedoAction<object>
    {
        ActionType = UndoRedoActionType.RowAdd,
        RowData = CloneData,
        RowIndex = (Parent.Rows.Count - 1),
        RowPosition = Parent.EditSettings.NewRowPosition,
        Timestamp = DateTime.Now
    };

    Parent.UndoRedoManager?.RecordAction(action);
}
```

#### C. BulkDelete() - Record Row Deletions
```csharp
// After line 1000 (EditAction set to Deleted):
if (Parent.EditSettings?.EnableUndoRedo == true)
{
    var action = new UndoRedoAction<object>
    {
        ActionType = UndoRedoActionType.RowDelete,
        RowData = data,
        RowIndex = dataRow.Index ?? -1,
        Timestamp = DateTime.Now
    };

    Parent.UndoRedoManager?.RecordAction(action);
}
```

#### D. BatchClose() - Clear Redo Stack on Cancel
```csharp
// When batch cancelled, clear redo stack
Parent.UndoRedoManager?.ClearRedoStack();
```

---

### 4. Keyboard Integration: `FocusHandler<T>`

**Location**: `src/Internal/Actions/FocusHandler.cs` (MODIFIED)

**Hook Point**: `ProcessKeyCombination()` method (Line ~700)

```csharp
// Add before existing key handling:
if (keyCombination?.Equals("ctrl+z", StringComparison.OrdinalIgnoreCase) == true)
{
    if (_parent?.EditSettings?.EnableUndoRedo == true && 
        _parent?.EditSettings?.Mode == EditMode.Batch &&
        _parent?.IsGridFocused == true)
    {
        await _parent.UndoRedoManager?.UndoAsync().ConfigureAwait(true);
        e.PreventDefault();
        return;
    }
}
else if (keyCombination?.Equals("ctrl+y", StringComparison.OrdinalIgnoreCase) == true ||
         keyCombination?.Equals("ctrl+shift+z", StringComparison.OrdinalIgnoreCase) == true)
{
    if (_parent?.EditSettings?.EnableUndoRedo == true && 
        _parent?.EditSettings?.Mode == EditMode.Batch &&
        _parent?.IsGridFocused == true)
    {
        await _parent.UndoRedoManager?.RedoAsync().ConfigureAwait(true);
        e.PreventDefault();
        return;
    }
}
```

---

### 5. Configuration: `GridEditSettings`

**Location**: `src/GridEditSettings.cs` (MODIFIED)

```csharp
/// <summary>
/// Enables Undo/Redo functionality for batch editing operations.
/// Only works in EditMode.Batch. Default: false (opt-in).
/// </summary>
[Parameter]
public bool EnableUndoRedo { get; set; } = false;

/// <summary>
/// Maximum number of undo/redo steps to maintain in memory.
/// When exceeded, oldest actions are discarded. Default: 20.
/// Setting to 0 disables stack limit (not recommended for large datasets).
/// </summary>
[Parameter]
public int UndoRedoLimit { get; set; } = 20;

// In OnParametersSetAsync():
if (EnableUndoRedo != _enableUndoRedoPrevious)
{
    if (EnableUndoRedo && Parent?.EditSettings?.Mode == EditMode.Batch)
    {
        Parent.UndoRedoManager?.Enable(UndoRedoLimit);
    }
    else
    {
        Parent.UndoRedoManager?.Disable();
    }
    _enableUndoRedoPrevious = EnableUndoRedo;
}
```

---

### 6. Public API: `SfGrid<T>`

**Location**: `src/SfGrid.razor.cs` (MODIFIED)

```csharp
// Inject UndoRedoManager instance
internal UndoRedoManager<T>? UndoRedoManager { get; set; }

// Initialize in OnInitializedAsync():
if (UndoRedoManager == null)
{
    UndoRedoManager = new UndoRedoManager<T>();
}

// Public API Methods
public async Task UndoAsync()
{
    if (UndoRedoManager != null)
    {
        await UndoRedoManager.UndoAsync().ConfigureAwait(true);
    }
}

public async Task RedoAsync()
{
    if (UndoRedoManager != null)
    {
        await UndoRedoManager.RedoAsync().ConfigureAwait(true);
    }
}

public async Task UndoAllAsync()
{
    if (UndoRedoManager != null)
    {
        await UndoRedoManager.UndoAllAsync().ConfigureAwait(true);
    }
}

public async Task RedoAllAsync()
{
    if (UndoRedoManager != null)
    {
        await UndoRedoManager.RedoAllAsync().ConfigureAwait(true);
    }
}

public async Task ClearUndoRedoAsync()
{
    if (UndoRedoManager != null)
    {
        UndoRedoManager.Clear();
    }
}

// Read-only Properties
public int UndoCount => UndoRedoManager?.UndoCount ?? 0;
public int RedoCount => UndoRedoManager?.RedoCount ?? 0;
public bool IsUndoAvailable => UndoRedoManager?.IsUndoAvailable ?? false;
public bool IsRedoAvailable => UndoRedoManager?.IsRedoAvailable ?? false;
```

---

## Keyboard Shortcut Handling Flow

```
┌─────────────────────────────────────────┐
│  Browser KeyDown Event (Ctrl+Z / Ctrl+Y)│
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ JavaScript: sf-grid.ts (GridKeyDown)    │
│ - Capture keyCode + modifiers           │
│ - Serialize to KeyboardEventArgs        │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ GridJSInteropAdaptor.GridKeyDown()      │
│ - Deserialize JSON args                 │
│ - Delegate to FocusHandler              │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ FocusHandler.ProcessGridKeyDown()       │
│ - Parse keyCombination string           │
│ - Route to ProcessKeyCombination()      │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ FocusHandler.ProcessKeyCombination()    │
│ - Match "ctrl+z" or "ctrl+y"            │
│ - Check: IsGridFocused                  │
│ - Check: EnableUndoRedo=true            │
│ - Check: Mode=Batch                     │
└──────────────┬──────────────────────────┘
               │
          [IF ALL CHECKS PASS]
               │
               ▼
┌─────────────────────────────────────────┐
│ UndoRedoManager.UndoAsync()             │
│ UndoRedoManager.RedoAsync()             │
│ - Pop from stack                        │
│ - Restore state                         │
│ - Update UI                             │
└─────────────────────────────────────────┘
```

---

## Stack Operation Details

### Recording an Action (RecordAction)

```
1. If Redo stack not empty:
   └─→ Clear Redo stack (new action invalidates redos)

2. Create action object with:
   └─→ OldValue, NewValue (for cell edits)
   └─→ RowData, Index (for row operations)
   └─→ Timestamp (for audit trail)

3. Add to Undo stack:
   └─→ UndoStack.AddLast(action)

4. If stack size > MaxStackSize:
   ├─→ Get oldest action: UndoStack.First()
   └─→ Remove: UndoStack.RemoveFirst()
       └─→ Dispose of any unmanaged resources

5. Update stack statistics:
   └─→ Fire PropertyChanged for UndoCount, IsUndoAvailable
```

### Undo Operation (UndoAsync)

```
1. Check: UndoStack.Count > 0?
   └─→ If empty, return (no action)

2. Pop from Undo: action = UndoStack.Last()

3. Based on ActionType:
   ├─→ CellEdit:
   │   └─→ Find row by index
   │   └─→ Set cell value = OldValue
   │   └─→ Mark cell as clean
   ├─→ RowAdd:
   │   └─→ Find row by index
   │   └─→ Remove row from grid
   └─→ RowDelete:
       └─→ Find row by index
       └─→ Add row back to original position
       └─→ Restore row data

4. Add action to Redo: RedoStack.AddLast(action)

5. Remove from Undo: UndoStack.RemoveLast()

6. Trigger UI refresh:
   └─→ StateHasChanged()

7. Update properties:
   └─→ Fire PropertyChanged for counts/availability
```

### Redo Operation (RedoAsync)

```
1. Check: RedoStack.Count > 0?
   └─→ If empty, return (no action)

2. Pop from Redo: action = RedoStack.Last()

3. Based on ActionType:
   ├─→ CellEdit:
   │   └─→ Find row by index
   │   └─→ Set cell value = NewValue
   ├─→ RowAdd:
   │   └─→ Add row back to grid at original index
   └─→ RowDelete:
       └─→ Find row and mark as deleted

4. Add action to Undo: UndoStack.AddLast(action)

5. Remove from Redo: RedoStack.RemoveLast()

6. Trigger UI refresh:
   └─→ StateHasChanged()

7. Update properties:
   └─→ Fire PropertyChanged for counts/availability
```

---

## State Restoration Strategy

### For Cell Edits
```csharp
// Undo: Restore old value
var row = Parent.Rows.FirstOrDefault(r => r.Index == action.CellChange.RowIndex);
if (row != null)
{
    row.EditedData ??= Clone(row.Data);
    Parent.PropHelper.SetObject(
        action.CellChange.FieldName,
        action.CellChange.OldValue,
        row.EditedData
    );
    row.IsDirty = true;  // Still dirty (has change)
}
```

### For Row Additions
```csharp
// Undo: Remove the added row
var row = Parent.Rows.FirstOrDefault(r => r.Index == action.RowIndex);
if (row?.Action == EditAction.Added)
{
    Parent.Rows.Remove(row);
}

// Redo: Add it back
var newRow = new Row<T> { Data = action.RowData, EditedData = action.RowData };
if (action.RowPosition == NewRowPosition.Top)
    Parent.Rows.Insert(0, newRow);
else
    Parent.Rows.Add(newRow);
```

### For Row Deletions
```csharp
// Undo: Restore deleted row
var newRow = new Row<T> 
{ 
    Data = action.RowData, 
    EditedData = action.RowData,
    Action = EditAction.Deleted  // Keep as deleted until save
};
Parent.Rows.Insert(action.RowIndex ?? 0, newRow);

// Redo: Mark as deleted again
var row = Parent.Rows.FirstOrDefault(r => r.Index == action.RowIndex);
if (row != null)
    row.Action = EditAction.Deleted;
```

---

## Edge Cases & Safeguards

| Case | Handling |
|------|----------|
| **Undo when empty** | Silently do nothing; no error |
| **Redo when empty** | Silently do nothing; no error |
| **Row index mismatch** | Log warning; skip undo (data consistency issue) |
| **Column missing** | Log warning; skip operation |
| **Stack limit exceeded** | Remove oldest action using FIFO |
| **EnableUndoRedo toggle** | Clear stacks immediately on disable |
| **Mode change to Normal** | Disable UndoRedo, clear stacks |
| **Grid data reload** | Clear stacks (new data context) |
| **Memory pressure** | Reduce UndoRedoLimit dynamically |

---

## Performance Considerations

| Operation | Complexity | Target | Strategy |
|-----------|-----------|--------|----------|
| RecordAction | O(1) | <1ms | LinkedList push |
| UndoAsync | O(1) | <5ms | Direct state restore |
| RedoAsync | O(1) | <5ms | Direct state reapply |
| Clear | O(n) | <10ms | Bulk disposal |
| Stack limit check | O(1) | <1ms | Counter check |

**Memory Footprint**:
- Per action: ~500 bytes (cell data + metadata)
- Limit of 20 actions: ~10 KB
- Limit of 100 actions: ~50 KB

---

## Testing Strategy

### Unit Tests (UndoRedoManager<T>)
- Stack push/pop with various limits
- Action recording sequencing
- Undo/redo state transitions
- Memory cleanup on clear

### Integration Tests (Edit<T>)
- Cell edit recording → undo → redo
- Row add recording → undo → redo
- Row delete recording → undo → redo
- Multiple sequential operations

### Keyboard Tests (FocusHandler)
- Ctrl+Z triggers undo
- Ctrl+Y triggers redo
- Ctrl+Shift+Z triggers redo
- Shortcuts ignored when not in Batch mode
- Shortcuts ignored when grid not focused

### Regression Tests
- Batch editing works without EnableUndoRedo (default disabled)
- No memory leaks from large undo stacks
- Performance unaffected when feature disabled

---

**Document Version**: 1.0  
**Architecture Status**: Ready for implementation
