# Undo/Redo Feature Analysis - Blazor DataGrid Batch Editing Integration

**Date:** August 12, 2026  
**Scope:** Complete implementation roadmap for Undo/Redo keyboard navigation and history management  
**Status:** Planning phase - ready for design walkthrough

---

## 1. EXECUTIVE SUMMARY

The Syncfusion Blazor DataGrid already has 80% of the infrastructure needed for Undo/Redo in Batch Edit mode:

✅ **READY:** Cell dirty tracking, value capture (before/after), edit action classification, keyboard routing
❌ **MISSING:** History stacks, undo/redo manager, Ctrl+Z/Y handlers, public APIs

**Implementation Approach:**
1. Add keyboard handlers in `FocusHandler.ProcessKeyDown()` for Ctrl+Z/Y/Ctrl+Shift+Z
2. Create `UndoRedoManager<T>` class to manage action stacks (Undo & Redo)
3. Create `Action<T>` model to serialize edits (CellEdit, RowAdd, RowDelete, Paste, AutoFill)
4. Hook into `Edit.cs` SaveCell/BulkAddRow/BulkDelete to record actions
5. Expose public APIs: `UndoAsync()`, `RedoAsync()`, `UndoAllAsync()`, `RedoAllAsync()`, `ClearUndoRedoAsync()`
6. Add stack properties: `UndoCount`, `RedoCount`, `IsUndoAvailable`, `IsRedoAvailable`
7. Raise events: `ActionUndoing`, `ActionUndone`, `ActionRedoing`, `ActionRedone` (with cancellation support)

---

## 2. BATCH EDITING ARCHITECTURE - CURRENT STATE

### 2.1 Value Capture Mechanism

**Before Values** (Original, stored in `Row<T>.Data`):
```csharp
// In Edit.cs SaveCell()
var PreviousVal = Parent.PropHelper?.GetObject(
    OriginalCell!.Column!.Field, 
    OriginalRow!.Data  // ← Original value
);
```

**After Values** (Modified, stored in `Row<T>.EditedData`):
```csharp
var EditedValue = Parent.PropHelper?.GetObject(
    OriginalCell!.Column!.Field, 
    OriginalRow.EditedData  // ← New value
);
```

**Dirty Detection:**
```csharp
OriginalCell.IsDirty = GridUtils.CompareValues<object>(
    PreviousVal,     // Original
    EditedValue      // Modified
);
```

**Key Event with Before/After Values:**
```csharp
// In Edit.cs - CellSaveArgs fired BEFORE save
public class CellSaveArgs<T>
{
    public string ColumnName { get; set; }
    public object? PreviousValue { get; set; }  // ← TRACK BEFORE
    public object? Value { get; set; }          // ← TRACK AFTER
    public T? RowData { get; set; }
    public T? Data { get; set; }
    public GridColumn? Column { get; set; }
    public bool Cancel { get; set; }
}
```

### 2.2 Row State Tracking

**Row Model:**
```csharp
public class Row<T>
{
    public T? Data { get; set; }           // Original values
    public T? EditedData { get; set; }     // Change delta
    public bool IsDirty { get; set; }      // Has changes?
    public EditAction Action { get; set; } // Added/Edited/Deleted/None
}
```

**EditAction Enum:**
```csharp
public enum EditAction
{
    None,      // No modification
    Edited,    // Existing row modified (for tracking changes)
    Deleted,   // Row marked for deletion
    Added,     // New row added
}
```

### 2.3 How to Access All Pending Changes

```csharp
// Added rows
var addedRows = Parent.Rows?.FindAll(
    row => row.Action == EditAction.Added && row.IsAddedTop
);

// Edited rows
var editedRows = Parent.Rows?.FindAll(
    row => row.EditedData != null 
        && row.Action != EditAction.Deleted 
        && row.Action != EditAction.Added
);

// Deleted rows
var deletedRows = Parent.Rows?.FindAll(
    row => row.Action == EditAction.Deleted
);

// Check if any pending changes
bool hasDirty = await Parent.IsDirtyAsync();
```

---

## 3. KEYBOARD NAVIGATION INFRASTRUCTURE - CURRENT STATE

### 3.1 Keyboard Event Flow

```
Browser KeyDown Event
    ↓
JavaScript: GridKeyDown() event handler
    ↓
JSON serialization to KeyboardEventArgs
    ↓
GridJSInteropAdaptor.cs → GridKeyDown() [JSInvokable method]
    ↓
FocusHandler.ProcessKeyDown(KeyboardEventArgs)
    ↓
Key combination detection & routing
    ↓
Action handler (navigation, copy, paste, etc.)
```

### 3.2 Key Routing Files & Classes

| File | Class/Method | Purpose |
|------|-------------|---------|
| `src/Internal/Base/GridJSInteropAdaptor.cs:585` | `GridKeyDown()` | JS interop entry point |
| `src/Internal/Actions/FocusHandler.cs` | `ProcessKeyDown()` | Key combination routing |
| `src/Internal/Base/Utils.cs:570-700` | `IsCtrlA()`, `IsCtrlC()`, `IsCtrlP()` | Key detection helpers |
| `src/Internal/Base/GridKeyboardEventArgs.cs` | `GetKeyCombination()` | Parse key + modifiers |

### 3.3 Existing Key Detection Helpers (Utils.cs)

```csharp
public static bool IsCtrlA(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyA";

public static bool IsCtrlC(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyC";

public static bool IsCtrlV(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyV";

public static bool IsCtrlP(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyP";

// ❌ MISSING - Need to add:
public static bool IsCtrlZ(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyZ";

public static bool IsCtrlY(KeyboardEventArgs e) => 
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyY";

public static bool IsCtrlShiftZ(KeyboardEventArgs e) => 
    e.CtrlKey && e.ShiftKey && e.Code == "KeyZ";
```

### 3.4 Key Combination Parsing

```csharp
// In FocusHandler.ProcessKeyDown()
var keyCombination = e.GetKeyCombination(
    isMacDevice: _parent!.IsMacDevice ?? false
);

// Examples:
// "Tab", "ShiftTab", "ArrowUp", "ArrowDown", "Enter", "Delete", "Escape"
// Also supports: "ctrl+key" format

// Pattern to follow:
bool isCtrlZ = keyCombination?.Equals("ctrl+z", StringComparison.OrdinalIgnoreCase) == true;
bool isCtrlY = keyCombination?.Equals("ctrl+y", StringComparison.OrdinalIgnoreCase) == true;
bool isCtrlShiftZ = keyCombination?.Equals("ctrl+shift+z", StringComparison.OrdinalIgnoreCase) == true;
```

### 3.5 Grid Focus Requirements

Keyboard shortcuts ONLY work when:
- Grid has focus (not when external input focused)
- Grid is enabled (`AllowEditing="true"` and not disabled)
- Edit mode is **Batch** (not Normal/Dialog)
- Undo/Redo is enabled (`GridEditSettings.EnableUndoRedo="true"`)

**Check grid focus:**
```csharp
// In FocusHandler or SfGrid
bool isGridFocused = Parent.IsGridFocused; // Property available
bool isEditingEnabled = Parent.AllowEditing;
bool isBatchMode = Parent.EditSettings.Mode == EditMode.Batch;
bool isUndoRedoEnabled = Parent.EditSettings.EnableUndoRedo;
```

---

## 4. EXISTING EVENTS FOR INTEGRATION

### 4.1 Cell-Level Events

**BEFORE Save Event:**
```csharp
// Fired in Edit.cs before SaveCell() completes
public EventCallback<CellSaveArgs<T>>? OnCellSave { get; set; }

public class CellSaveArgs<T>
{
    public string ColumnName { get; set; }
    public object? PreviousValue { get; set; }  // ← HOOK HERE FOR BEFORE
    public object? Value { get; set; }          // ← HOOK HERE FOR AFTER
    public T? RowData { get; set; }
    public T? Data { get; set; }
    public GridColumn? Column { get; set; }
    public bool Cancel { get; set; }            // Can cancel the save
}
```

**AFTER Save Event:**
```csharp
public EventCallback<CellSavedArgs<T>>? CellSaved { get; set; }

public class CellSavedArgs<T>
{
    public string ColumnName { get; set; }
    public object? Data { get; set; }
    public T? RowData { get; set; }
}
```

### 4.2 Row-Level Events

```csharp
public EventCallback<ActionEventArgs<T>>? OnActionBegin { get; set; }
public EventCallback<ActionEventArgs<T>>? OnActionComplete { get; set; }

public class ActionEventArgs<T>
{
    public ActionType RequestType { get; set; } // Save, Cancel, Add, Delete, etc.
    public List<T>? Data { get; set; }
}
```

---

## 5. ACTION MODEL FOR UNDO/REDO

### 5.1 Action Types to Track

```csharp
public enum UndoRedoActionType
{
    CellEdit,      // Single cell value change
    RowAdd,        // Row added
    RowDelete,     // Row deleted
    Paste,         // Multi-cell paste (atomic)
    AutoFill,      // Fill-handle pattern (atomic)
}
```

### 5.2 Action Data Structure

```csharp
public class UndoRedoAction<T>
{
    // Metadata
    public string ActionId { get; set; }                  // GUID or incremental
    public UndoRedoActionType ActionType { get; set; }   // What happened?
    public DateTime Timestamp { get; set; }
    
    // For CellEdit
    public int? RowIndex { get; set; }
    public string? ColumnField { get; set; }
    public object? PreviousValue { get; set; }           // Before
    public object? NewValue { get; set; }                // After
    
    // For RowAdd/RowDelete
    public T? RowData { get; set; }                      // Full row snapshot
    
    // For Paste/AutoFill (bulk operations)
    public List<CellChange<T>>? CellChanges { get; set; } // All affected cells
    
    // Associated column info
    public GridColumn? Column { get; set; }
}

public class CellChange<T>
{
    public int RowIndex { get; set; }
    public string ColumnField { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}
```

### 5.3 Validation on Undo/Redo

**User Preference:** "How EJ2 handles it" - validation should be bypassed on undo (data was valid originally)

```csharp
// Undo: Bypass validation (restoring to known-good state)
bool bypassValidation = true;  // Skip column.ValidationRules
ApplyCellValue(previousValue, bypassValidation: true);

// Redo: Enforce validation (new operation, must pass checks)
bool bypassValidation = false; // Check column.ValidationRules
var isValid = ApplyCellValue(newValue, bypassValidation: false);
if (!isValid)
{
    // Raise ActionRedoingFailed event
    // Leave action in Redo stack
    return false;
}
```

---

## 6. HISTORY STACKS IMPLEMENTATION

### 6.1 Stack Manager Class

```csharp
public class UndoRedoManager<T>
{
    private Stack<UndoRedoAction<T>> UndoStack { get; set; }
    private Stack<UndoRedoAction<T>> RedoStack { get; set; }
    private int MaxStackSize { get; set; } = 20; // Default
    
    public int UndoCount => UndoStack.Count;
    public int RedoCount => RedoStack.Count;
    public bool IsUndoAvailable => UndoCount > 0;
    public bool IsRedoAvailable => RedoCount > 0;
    
    // Core operations
    public void Push(UndoRedoAction<T> action);           // Add to Undo stack
    public UndoRedoAction<T>? Pop();                     // Remove from Undo stack
    public UndoRedoAction<T>? PeekUndo();                // View top action (don't remove)
    public void MoveToRedo(UndoRedoAction<T> action);    // Move Undo → Redo
    public void ClearRedo();                             // Clear Redo stack (new edit invalidates redo)
    public void Clear();                                 // Clear both stacks
}
```

### 6.2 Stack Limits

```csharp
// In GridEditSettings.cs
public int UndoRedoLimit { get; set; } = 20;  // Default

// When pushing action:
if (UndoStack.Count >= UndoRedoLimit)
{
    // Dequeue (remove) oldest action from bottom
    var oldest = UndoStack.DequeueFromBottom();
    // This is O(n) for standard Stack - consider using LinkedList<T>
}
UndoStack.Push(newAction);
```

### 6.3 Optimized Stack Implementation

```csharp
// Use LinkedList<T> for efficient FIFO eviction
private LinkedList<UndoRedoAction<T>> UndoStack 
    = new LinkedList<UndoRedoAction<T>>();

// When limit exceeded:
if (UndoStack.Count >= MaxStackSize)
{
    UndoStack.RemoveFirst();  // O(1) operation
}
UndoStack.AddLast(newAction);  // O(1) operation
```

---

## 7. INTEGRATION HOOKS IN EDIT.CS

### 7.1 Cell Edit Hook

**File:** `src/Internal/Actions/Edit.cs` → `SaveCell()` method

```csharp
public async Task SaveCell(...)
{
    // ... existing code ...
    
    var PreviousVal = Parent.PropHelper?.GetObject(
        OriginalCell!.Column!.Field, 
        OriginalRow!.Data
    );
    
    // NEW: Record before-state
    var beforeState = new UndoRedoAction<T>
    {
        ActionId = Guid.NewGuid().ToString(),
        ActionType = UndoRedoActionType.CellEdit,
        RowIndex = RowIndex,
        ColumnField = OriginalCell.Column.Field,
        PreviousValue = PreviousVal,
        // NewValue = will be captured on success
        Column = OriginalCell.Column,
        Timestamp = DateTime.Now,
    };
    
    // ... Save the cell ...
    var EditedValue = Parent.PropHelper?.GetObject(
        OriginalCell!.Column!.Field, 
        OriginalRow.EditedData
    );
    
    // NEW: Record after-state and push to Undo stack
    beforeState.NewValue = EditedValue;
    if (GridEditSettings.EnableUndoRedo)
    {
        Parent.UndoRedoManager.Push(beforeState);
        Parent.UndoRedoManager.ClearRedo();  // New edit invalidates redo
        // Raise events if needed
    }
}
```

### 7.2 Row Add Hook

**File:** `src/Internal/Actions/Edit.cs` → `BulkAddRow()` method

```csharp
public async Task BulkAddRow(...)
{
    var newRow = new Row<T> { Data = newData, EditedData = newData };
    
    // NEW: Record row addition
    if (GridEditSettings.EnableUndoRedo)
    {
        var action = new UndoRedoAction<T>
        {
            ActionId = Guid.NewGuid().ToString(),
            ActionType = UndoRedoActionType.RowAdd,
            RowData = newData,
            Timestamp = DateTime.Now,
        };
        Parent.UndoRedoManager.Push(action);
        Parent.UndoRedoManager.ClearRedo();
    }
    
    // ... Add row to grid ...
}
```

### 7.3 Row Delete Hook

**File:** `src/Internal/Actions/Edit.cs` → `BulkDelete()` method

```csharp
public async Task BulkDelete(...)
{
    var rowToDelete = Parent.Rows?[index];
    
    // NEW: Record row deletion (snapshot the data)
    if (GridEditSettings.EnableUndoRedo)
    {
        var action = new UndoRedoAction<T>
        {
            ActionId = Guid.NewGuid().ToString(),
            ActionType = UndoRedoActionType.RowDelete,
            RowData = rowToDelete?.Data,  // Save snapshot
            RowIndex = index,
            Timestamp = DateTime.Now,
        };
        Parent.UndoRedoManager.Push(action);
        Parent.UndoRedoManager.ClearRedo();
    }
    
    // ... Delete row ...
}
```

### 7.4 Batch Save Hook (Clear History)

**File:** `src/Internal/Actions/Edit.cs` → `BulkSave()` method

```csharp
public async Task BulkSave(...)
{
    // ... Save all changes to server/datasource ...
    
    // NEW: Clear history after successful save
    if (GridEditSettings.EnableUndoRedo)
    {
        Parent.UndoRedoManager.Clear();
        // Raise event indicating history cleared
    }
}
```

### 7.5 Batch Cancel Hook (Clear History)

**File:** `src/Internal/Actions/Edit.cs` → `CancelBatch()` or equivalent

```csharp
public async Task CancelBatch(...)
{
    // ... Revert all pending changes ...
    
    // NEW: Clear history after cancel
    if (GridEditSettings.EnableUndoRedo)
    {
        Parent.UndoRedoManager.Clear();
    }
}
```

---

## 8. KEYBOARD HANDLER IMPLEMENTATION

### 8.1 Add Key Detection Helpers (Utils.cs)

```csharp
// Add to src/Internal/Base/Utils.cs around line 700

public static bool IsCtrlZ(KeyboardEventArgs e) =>
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyZ";

public static bool IsCtrlY(KeyboardEventArgs e) =>
    e.CtrlKey && !e.ShiftKey && e.Code == "KeyY";

public static bool IsCtrlShiftZ(KeyboardEventArgs e) =>
    e.CtrlKey && e.ShiftKey && e.Code == "KeyZ";
```

### 8.2 Add Keyboard Handlers (FocusHandler.cs)

**File:** `src/Internal/Actions/FocusHandler.cs` → `ProcessKeyDown()` method

```csharp
public async Task ProcessKeyDown(KeyboardEventArgs e, ...)
{
    // ... existing key handlers ...
    
    // NEW: Undo/Redo handlers
    if (!Parent!.EditSettings!.EnableUndoRedo || 
        Parent.EditSettings.Mode != EditMode.Batch)
    {
        // Skip if not enabled or not in batch mode
        return;
    }
    
    var keyCombination = e.GetKeyCombination(
        isMacDevice: _parent!.IsMacDevice ?? false
    );
    
    // Check for Ctrl+Z (Undo)
    if (GridUtils.IsCtrlZ(e))
    {
        e.PreventDefault();  // Prevent browser default
        if (Parent.UndoRedoManager.IsUndoAvailable)
        {
            await Parent.UndoAsync();
        }
        return;
    }
    
    // Check for Ctrl+Y (Redo)
    if (GridUtils.IsCtrlY(e))
    {
        e.PreventDefault();
        if (Parent.UndoRedoManager.IsRedoAvailable)
        {
            await Parent.RedoAsync();
        }
        return;
    }
    
    // Check for Ctrl+Shift+Z (Redo alternative)
    if (GridUtils.IsCtrlShiftZ(e))
    {
        e.PreventDefault();
        if (Parent.UndoRedoManager.IsRedoAvailable)
        {
            await Parent.RedoAsync();
        }
        return;
    }
}
```

---

## 9. PUBLIC API METHODS (SfGrid.cs)

### 9.1 Core Undo/Redo Methods

```csharp
// In SfGrid<T>.cs or SfGrid.Methods.cs

/// <summary>
/// Undo the most recent action if available.
/// </summary>
public async Task UndoAsync()
{
    if (!EditSettings?.EnableUndoRedo ?? true || 
        EditSettings?.Mode != EditMode.Batch)
    {
        return;
    }
    
    if (!UndoRedoManager.IsUndoAvailable)
    {
        return;
    }
    
    var action = UndoRedoManager.PeekUndo();
    
    // Raise ActionUndoing event (can be cancelled)
    var undoingArgs = new ActionUndoingArgs<T>
    {
        ActionId = action.ActionId,
        ActionType = action.ActionType,
        // ... other metadata ...
        Cancel = false,
    };
    
    if (OnActionUndoing.HasDelegate)
    {
        await OnActionUndoing.InvokeAsync(undoingArgs);
    }
    
    if (undoingArgs.Cancel)
    {
        return;  // Undo cancelled by event handler
    }
    
    // Execute undo
    await ApplyUndo(action);
    
    // Move to redo stack
    UndoRedoManager.Pop();
    UndoRedoManager.MoveToRedo(action);
    
    // Raise ActionUndone event
    if (OnActionUndone.HasDelegate)
    {
        var undoneArgs = new ActionUndoneArgs<T>
        {
            ActionId = action.ActionId,
            ActionType = action.ActionType,
            // ... metadata ...
        };
        await OnActionUndone.InvokeAsync(undoneArgs);
    }
    
    // Update UI (redraw affected rows)
    await Refresh();
}

public async Task RedoAsync()
{
    if (!EditSettings?.EnableUndoRedo ?? true || 
        EditSettings?.Mode != EditMode.Batch)
    {
        return;
    }
    
    if (!UndoRedoManager.IsRedoAvailable)
    {
        return;
    }
    
    var action = UndoRedoManager.PeekRedo();
    
    // Raise ActionRedoing event (can be cancelled)
    var redoingArgs = new ActionRedoingArgs<T>
    {
        ActionId = action.ActionId,
        // ...
        Cancel = false,
    };
    
    if (OnActionRedoing.HasDelegate)
    {
        await OnActionRedoing.InvokeAsync(redoingArgs);
    }
    
    if (redoingArgs.Cancel)
    {
        return;
    }
    
    // Execute redo with validation enforced
    await ApplyRedo(action, bypassValidation: false);
    
    // Move to undo stack
    UndoRedoManager.PopRedo();
    UndoRedoManager.MoveToUndo(action);
    
    // Raise ActionRedone event
    if (OnActionRedone.HasDelegate)
    {
        await OnActionRedone.InvokeAsync(new ActionRedoneArgs<T>
        {
            ActionId = action.ActionId,
            // ...
        });
    }
    
    await Refresh();
}

public async Task UndoAllAsync()
{
    while (UndoRedoManager.IsUndoAvailable)
    {
        await UndoAsync();
    }
}

public async Task RedoAllAsync()
{
    while (UndoRedoManager.IsRedoAvailable)
    {
        await RedoAsync();
    }
}

public async Task ClearUndoRedoAsync()
{
    UndoRedoManager.Clear();
    // Optional: refresh UI to update button states
}
```

### 9.2 Apply Undo Helper Method

```csharp
private async Task ApplyUndo(UndoRedoAction<T> action)
{
    switch (action.ActionType)
    {
        case UndoRedoActionType.CellEdit:
            // Restore previous value to the cell
            var row = Rows?[action.RowIndex];
            if (row != null)
            {
                Parent.PropHelper?.SetObject(
                    action.ColumnField,
                    action.PreviousValue,
                    row.EditedData,
                    bypassValidation: true  // Skip validation
                );
                row.IsDirty = false;  // Reset dirty flag
            }
            break;
            
        case UndoRedoActionType.RowAdd:
            // Remove the added row
            var indexToRemove = Rows?.FindIndex(r => r.Data?.Equals(action.RowData) == true);
            if (indexToRemove >= 0)
            {
                Rows?.RemoveAt(indexToRemove);
            }
            break;
            
        case UndoRedoActionType.RowDelete:
            // Restore the deleted row
            Rows?.Insert(action.RowIndex, new Row<T> 
            { 
                Data = action.RowData, 
                EditedData = null 
            });
            break;
            
        case UndoRedoActionType.Paste:
            // Restore all cells in the paste operation
            foreach (var change in action.CellChanges ?? [])
            {
                var row = Rows?[change.RowIndex];
                if (row != null)
                {
                    Parent.PropHelper?.SetObject(
                        change.ColumnField,
                        change.OldValue,
                        row.EditedData,
                        bypassValidation: true
                    );
                }
            }
            break;
    }
}

private async Task ApplyRedo(UndoRedoAction<T> action, bool bypassValidation)
{
    // Similar to ApplyUndo but:
    // - For CellEdit: restore NewValue instead of PreviousValue
    // - Enforce validation if bypassValidation=false
    // - Raise validation error event if redo fails
}
```

---

## 10. STACK PROPERTIES (SfGrid.Properties.cs)

```csharp
/// <summary>
/// Gets the number of undo actions available.
/// </summary>
public int UndoCount => UndoRedoManager?.UndoCount ?? 0;

/// <summary>
/// Gets the number of redo actions available.
/// </summary>
public int RedoCount => UndoRedoManager?.RedoCount ?? 0;

/// <summary>
/// Gets a value indicating whether undo is available.
/// </summary>
public bool IsUndoAvailable => UndoRedoManager?.IsUndoAvailable ?? false;

/// <summary>
/// Gets a value indicating whether redo is available.
/// </summary>
public bool IsRedoAvailable => UndoRedoManager?.IsRedoAvailable ?? false;
```

---

## 11. EVENT DEFINITIONS (GridEvents.cs)

```csharp
// NEW events to add to GridEvents.cs

/// <summary>
/// Triggered before an undo operation. Can be cancelled.
/// </summary>
public EventCallback<ActionUndoingArgs<T>>? OnActionUndoing { get; set; }

/// <summary>
/// Triggered after an undo operation completes.
/// </summary>
public EventCallback<ActionUndoneArgs<T>>? OnActionUndone { get; set; }

/// <summary>
/// Triggered before a redo operation. Can be cancelled.
/// </summary>
public EventCallback<ActionRedoingArgs<T>>? OnActionRedoing { get; set; }

/// <summary>
/// Triggered after a redo operation completes.
/// </summary>
public EventCallback<ActionRedoneArgs<T>>? OnActionRedone { get; set; }
```

---

## 12. EVENT ARGUMENT MODELS (EventModels/Grids.cs)

```csharp
public class ActionUndoingArgs<T>
{
    public string ActionId { get; set; }
    public UndoRedoActionType ActionType { get; set; }
    public int? RowIndex { get; set; }
    public string? ColumnField { get; set; }
    public object? PreviousValue { get; set; }
    public object? NewValue { get; set; }
    public bool Cancel { get; set; }
}

public class ActionUndoneArgs<T>
{
    public string ActionId { get; set; }
    public UndoRedoActionType ActionType { get; set; }
    public int? RowIndex { get; set; }
    public List<int>? AffectedRowIndices { get; set; }
}

public class ActionRedoingArgs<T>
{
    public string ActionId { get; set; }
    public UndoRedoActionType ActionType { get; set; }
    public bool Cancel { get; set; }
}

public class ActionRedoneArgs<T>
{
    public string ActionId { get; set; }
    public UndoRedoActionType ActionType { get; set; }
}
```

---

## 13. CONFIGURATION IN GRIDEDITSETTINGS.CS

```csharp
/// <summary>
/// Enables or disables the Undo/Redo feature. Only supported in Batch Edit mode.
/// Default is false.
/// </summary>
public bool EnableUndoRedo { get; set; } = false;

/// <summary>
/// Sets the maximum number of actions to maintain in Undo/Redo stacks.
/// Default is 20. Oldest actions are discarded when limit exceeded.
/// </summary>
public int UndoRedoLimit { get; set; } = 20;
```

---

## 14. CROSS-FEATURE INTERACTION GUARANTEES

### 14.1 Sorting + Undo/Redo

```
Current Sort: Name (A-Z)
User edits: John → Zoe (name changes)
User presses Ctrl+Z
Expected: Name reverts to John, row moves back to original position per sort
```

**Implementation:** After ApplyUndo(), re-sort by active sort columns

### 14.2 Filtering + Undo/Redo

```
Current Filter: City = "NYC"
User edits: City from "NYC" → "LA" (now doesn't match filter)
User presses Ctrl+Z
Expected: City reverts to "NYC", row stays visible (within filter)
```

**Implementation:** After ApplyUndo(), re-apply filter predicate

### 14.3 Grouping + Undo/Redo

```
Grouped By: Department
User edits: Department from "Sales" → "HR"
User presses Ctrl+Z
Expected: Department reverts, row returns to original group
```

**Implementation:** After ApplyUndo(), re-group data

### 14.4 Paging + Undo/Redo

```
Page: 2 (rows 20-30 visible)
User edits: Row 25
User goes to Page 1
User presses Ctrl+Z on Page 1
Expected: Edit on Page 2 is undone, if user navigates back to Page 2, change is undone
```

**Implementation:** Undo operates on underlying data, not UI view

### 14.5 FrozenColumns + Undo/Redo

```
Frozen: ID, Name
User edits: Name in frozen column
User presses Ctrl+Z
Expected: Value reverted, frozen column layout preserved
```

**Implementation:** Column freeze state independent of undo/redo

---

## 15. VALIDATION STRATEGY

### 15.1 Undo Validation Handling

```
User enters "Invalid Value" that passes current validation rules (soft validation)
Validation rules change at runtime (become stricter)
User presses Ctrl+Z
Expected: Value reverts to previous (bypassing new validation rules)
Rationale: Data was originally valid, we're restoring to a known-good state
```

**Implementation:**
```csharp
// In ApplyUndo()
Parent.PropHelper?.SetObject(
    fieldName,
    previousValue,
    row.EditedData,
    bypassValidation: true  // ← BYPASS validation on undo
);
```

### 15.2 Redo Validation Handling

```
User edits: "Value A" → "Value B"
User presses Ctrl+Z (now "Value A" again, "Value B" in Redo stack)
Validation rules change (Value B now invalid)
User presses Ctrl+Y
Expected: Redo fails, event raised, action remains in Redo stack
```

**Implementation:**
```csharp
// In ApplyRedo()
var isValid = Parent.PropHelper?.ValidateValue(
    newValue,
    column.ValidationRules
);

if (!isValid)
{
    // Raise RedoValidationFailed event
    return false;  // Redo cancelled
}
```

---

## 16. MEMORY MANAGEMENT

### 16.1 Stack Size Configuration

```csharp
// Default
UndoRedoLimit = 20  // Each action ≈ 40-100 bytes

// Recommendations for large datasets:
// - 100K+ rows: UndoRedoLimit = 5-10
// - 10M+ rows: UndoRedoLimit = 1-3
// - Normal use: UndoRedoLimit = 20-50
```

### 16.2 Memory Cleanup

```csharp
// On Batch Save
BulkSave() → UndoRedoManager.Clear()

// On Batch Cancel
CancelBatch() → UndoRedoManager.Clear()

// On Grid Refresh
Refresh() → UndoRedoManager.Clear()

// On Data Reload
DataSource = newSource → UndoRedoManager.Clear()

// Manual cleanup
await grid.ClearUndoRedoAsync()
```

### 16.3 No Persistence

- Stacks cleared on page refresh
- No localStorage serialization by default
- Workaround: Serialize stacks manually if needed

---

## 17. FILE CHANGES SUMMARY

| File | Change Type | Purpose |
|------|------------|---------|
| `src/Internal/Base/Utils.cs` | ADD | Add `IsCtrlZ()`, `IsCtrlY()`, `IsCtrlShiftZ()` helpers |
| `src/Internal/Actions/FocusHandler.cs` | MODIFY | Add Ctrl+Z/Y/Ctrl+Shift+Z handler logic |
| `src/Internal/Actions/Edit.cs` | MODIFY | Hook into SaveCell, BulkAddRow, BulkDelete, BulkSave |
| `src/GridEditSettings.cs` | ADD | Add `EnableUndoRedo` and `UndoRedoLimit` properties |
| `src/Internal/Models/UndoRedoManager.cs` | CREATE | Stack management (LinkedList-based) |
| `src/Internal/Models/UndoRedoAction.cs` | CREATE | Action model with metadata |
| `src/SfGrid.Methods.cs` | ADD | Public APIs: UndoAsync, RedoAsync, UndoAllAsync, etc. |
| `src/SfGrid.Properties.cs` | ADD | Properties: UndoCount, RedoCount, IsUndoAvailable, IsRedoAvailable |
| `src/GridEvents.cs` | ADD | Events: OnActionUndoing, OnActionUndone, OnActionRedoing, OnActionRedone |
| `src/EventModels/Grids.cs` | ADD | Event argument classes |
| `src/Enumeration/GridsEnumerations.cs` | ADD | `UndoRedoActionType` enum |

---

## 18. IMPLEMENTATION CHECKLIST

- [ ] Add key detection helpers (Utils.cs)
- [ ] Create UndoRedoManager<T> class
- [ ] Create UndoRedoAction<T> model
- [ ] Add EnableUndoRedo & UndoRedoLimit to GridEditSettings
- [ ] Add keyboard handlers in FocusHandler.ProcessKeyDown
- [ ] Hook Edit.SaveCell to record actions
- [ ] Hook Edit.BulkAddRow to record additions
- [ ] Hook Edit.BulkDelete to record deletions
- [ ] Hook BulkSave to clear stacks
- [ ] Add public API methods (UndoAsync, RedoAsync, etc.)
- [ ] Add stack properties (UndoCount, RedoCount, etc.)
- [ ] Define events (OnActionUndoing, OnActionUndone, etc.)
- [ ] Create event argument models
- [ ] Implement cross-feature interactions (sorting, filtering, grouping, paging, frozen columns)
- [ ] Unit tests for stack operations
- [ ] Unit tests for keyboard shortcuts
- [ ] Unit tests for batch edit scenarios
- [ ] Integration tests with sorting/filtering/grouping
- [ ] Documentation & examples

---

## 19. NOTES & DECISIONS

1. **Keyboard Shortcuts:** Following standard browser conventions (Ctrl+Z for Undo, Ctrl+Y for Redo)
2. **Validation Handling:** Bypass on Undo (data integrity), enforce on Redo (consistency)
3. **Stack Eviction:** FIFO (oldest actions first) when limit exceeded
4. **Atomicity:** Paste/AutoFill grouped as single actions (not individual cells)
5. **Redo Stack Clearing:** Automatically cleared on new edit (standard undo/redo UX)
6. **History Clearing:** Auto-cleared on save/cancel/refresh (prevents stale undo data)
7. **Persistence:** No built-in persistence; stacks cleared on browser refresh
8. **Focus Requirements:** Undo/Redo only work when grid focused and in Batch mode
9. **EJ2 Parity:** Validation handling matches EJ2 DataGrid implementation

---

## 20. DELIVERABLES

**Phase 1: Core Infrastructure**
- UndoRedoManager & UndoRedoAction models
- Keyboard detection helpers
- FocusHandler keyboard routing

**Phase 2: Edit Integration**
- Hook into SaveCell, BulkAddRow, BulkDelete
- Stack recording for all action types
- History cleanup on save/cancel

**Phase 3: Public API & Events**
- UndoAsync, RedoAsync methods
- Stack properties
- Event definitions and argument models

**Phase 4: Cross-Feature Testing**
- Integration with sorting, filtering, grouping, paging, frozen columns
- Validation behavior validation
- Memory management verification

---

**Status:** Ready for implementation walkthrough with architecture team ✅
