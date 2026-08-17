# Undo/Redo Feature - Stage 1: Implementation Tasks

## Overview
Implement keyboard-driven Undo/Redo infrastructure for Batch Edit mode. This task list follows the spec-driven workflow and includes all necessary files, hooks, and configuration.

---

## Task Phases

### Phase 1: Core Infrastructure (Foundation)

#### Task 1.1: Create UndoRedoAction Model
- [ ] Create file: `src/Models/UndoRedoAction.cs`
- [ ] Define `UndoRedoActionType` enum (CellEdit, RowAdd, RowDelete, Paste, AutoFill)
- [ ] Define `UndoRedoAction<T>` class with properties:
  - ActionType, SequenceNumber
  - CellChange (for cell edits)
  - RowData, RowIndex, RowPosition (for row operations)
  - PreviousValues, PreviousRows (for multi-cell/multi-row actions)
- [ ] Define `CellChange<T>` class with properties:
  - RowIndex, ColumnIndex, FieldName, OldValue, NewValue, Column
- [ ] Add XML documentation comments
- [ ] **VALIDATION NOTE:** Removed Timestamp field (validated against EJ2 production: not used in standard impl)

#### Task 1.2: Create UndoRedoManager Class
- [ ] Create file: `src/Internal/Actions/UndoRedoManager.cs`
- [ ] Implement generic class `UndoRedoManager<T>`
- [ ] Implement stacks using `LinkedList<UndoRedoAction<T>>` (not Stack<T> for FIFO eviction)
- [ ] Implement properties:
  - MaxStackSize (default 20)
  - IsEnabled
  - UndoCount (read-only)
  - RedoCount (read-only)
  - IsUndoAvailable (read-only)
  - IsRedoAvailable (read-only)
- [ ] Implement core methods:
  - `RecordAction(UndoRedoAction<T> action)` - Add to undo stack, clear redo, enforce limit
  - `UndoAsync()` - Pop undo, restore state, push to redo
  - `RedoAsync()` - Pop redo, reapply state, push to undo
  - `UndoAllAsync()` - Undo all actions sequentially
  - `RedoAllAsync()` - Redo all actions sequentially
  - `ClearRedoStack()` - Clear redo stack only (used after cancel)
  - `Clear()` - Clear both stacks
  - `Enable(int stackLimit)` - Enable with limit
  - `Disable()` - Disable and clear
- [ ] Add XML documentation comments
- [ ] Add internal logging (Debug output for state transitions)

---

### Phase 2: Keyboard Integration (Event Routing)

#### Task 2.1: Update FocusHandler KeyHandling
- [ ] Open file: `src/Internal/Actions/FocusHandler.cs`
- [ ] Locate `ProcessKeyCombination()` method (approximately line 700)
- [ ] Add keyboard shortcut handlers BEFORE existing key handling:
  - `if (keyCombination?.Equals("ctrl+z", StringComparison.OrdinalIgnoreCase))`
  - `else if (keyCombination?.Equals("ctrl+y", ...) || keyCombination?.Equals("ctrl+shift+z", ...))`
- [ ] For each shortcut, check guards:
  - `EnableUndoRedo == true`
  - `Mode == EditMode.Batch`
  - `IsGridFocused == true`
- [ ] Call `await _parent.UndoRedoManager?.UndoAsync()` or `RedoAsync()`
- [ ] Call `e.PreventDefault()` and `return` to prevent browser default behavior
- [ ] Add inline comments explaining guards

#### Task 2.2: Add Key Detection Helpers (Optional)
- [ ] Open file: `src/Internal/Base/Utils.cs`
- [ ] Locate existing key helper methods (e.g., `IsCtrlA()`, `IsCtrlC()`)
- [ ] Add helper methods (following existing pattern):
  - `public static bool IsCtrlZ(KeyboardEventArgs e)` 
  - `public static bool IsCtrlY(KeyboardEventArgs e)`
  - `public static bool IsCtrlShiftZ(KeyboardEventArgs e)`
- [ ] Use in FocusHandler as alternative to string comparison (optional, for consistency)

---

### Phase 3: Edit Integration (Action Recording)

#### Task 3.1: Hook Cell Save (SaveCell)
- [ ] Open file: `src/Internal/Actions/Edit.cs`
- [ ] Locate `SaveCell()` method (Line ~454)
- [ ] Locate where `CellSaved` event is fired (Line ~517-520)
- [ ] After `CellSaved` event, add:
  ```
  if (Parent.EditSettings?.EnableUndoRedo == true && 
      Parent.EditSettings?.Mode == EditMode.Batch &&
      Parent.UndoRedoManager != null)
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
          CellChange = cellChange
      };

      Parent.UndoRedoManager.RecordAction(action);
  }
  ```
- [ ] Add inline comments

#### Task 3.2: Hook Row Addition (BulkAddRow)
- [ ] Open file: `src/Internal/Actions/Edit.cs`
- [ ] Locate `BulkAddRow()` method (Line ~697)
- [ ] Find where row is added to `Parent.Rows` (Line ~730-732)
- [ ] After row addition, add:
  ```
  if (Parent.EditSettings?.EnableUndoRedo == true &&
      Parent.UndoRedoManager != null)
  {
      var action = new UndoRedoAction<object>
      {
          ActionType = UndoRedoActionType.RowAdd,
          RowData = CloneData,
          RowIndex = (Parent.Rows?.Count - 1) ?? -1,
          RowPosition = Parent.EditSettings.NewRowPosition
      };

      Parent.UndoRedoManager.RecordAction(action);
  }
  ```
- [ ] Add inline comments

#### Task 3.3: Hook Row Deletion (BulkDelete)
- [ ] Open file: `src/Internal/Actions/Edit.cs`
- [ ] Locate `BulkDelete()` method (Line ~958)
- [ ] Find where `row.Action = EditAction.Deleted` is assigned (Line ~1000)
- [ ] After `EditAction.Deleted` assignment, add:
  ```
  if (Parent.EditSettings?.EnableUndoRedo == true &&
      Parent.UndoRedoManager != null)
  {
      var action = new UndoRedoAction<object>
      {
          ActionType = UndoRedoActionType.RowDelete,
          RowData = data,
          RowIndex = dataRow.Index ?? -1
      };

      Parent.UndoRedoManager.RecordAction(action);
  }
  ```
- [ ] Add inline comments

#### Task 3.4: Redo Stack Clear on Batch Cancel
- [ ] Open file: `src/Internal/Actions/Edit.cs`
- [ ] Locate `BatchClose()` method (Line ~985)
- [ ] At beginning of method, before any cleanup, add:
  ```
  if (Parent.EditSettings?.EnableUndoRedo == true &&
      Parent.UndoRedoManager != null)
  {
      Parent.UndoRedoManager.ClearRedoStack();
  }
  ```
- [ ] Add comment explaining: "Clear redo stack on batch cancel (new actions invalidate redos)"

---

### Phase 4: Configuration (GridEditSettings)

#### Task 4.1: Add Properties to GridEditSettings
- [ ] Open file: `src/GridEditSettings.cs`
- [ ] Locate existing `[Parameter]` properties (around line 10-50)
- [ ] Add new properties:
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
  /// </summary>
  [Parameter]
  public int UndoRedoLimit { get; set; } = 20;
  ```
- [ ] Add private fields to track previous values:
  ```csharp
  private bool _enableUndoRedoPrevious = false;
  private int _undoRedoLimitPrevious = 20;
  ```
- [ ] Add XML documentation comments

#### Task 4.2: Update GridEditSettings.OnParametersSetAsync()
- [ ] Open file: `src/GridEditSettings.cs`
- [ ] Locate `OnParametersSetAsync()` method
- [ ] At end of method, before `base.OnParametersSetAsync()` call, add:
  ```csharp
  // Handle UndoRedo enable/disable changes
  if (EnableUndoRedo != _enableUndoRedoPrevious ||
      UndoRedoLimit != _undoRedoLimitPrevious)
  {
      if (Parent?.UndoRedoManager != null)
      {
          if (EnableUndoRedo && Parent?.EditSettings?.Mode == EditMode.Batch)
          {
              Parent.UndoRedoManager.Enable(UndoRedoLimit);
          }
          else
          {
              Parent.UndoRedoManager.Disable();
          }
      }
      _enableUndoRedoPrevious = EnableUndoRedo;
      _undoRedoLimitPrevious = UndoRedoLimit;
  }
  ```
- [ ] Add inline comments

---

### Phase 5: SfGrid Public API (Component Integration)

#### Task 5.1: Add UndoRedoManager to SfGrid
- [ ] Open file: `src/SfGrid.razor.cs`
- [ ] Locate property declarations (around line 50-100)
- [ ] Add property:
  ```csharp
  /// <summary>
  /// Internal manager for Undo/Redo operations. 
  /// Accessed via public API methods and properties.
  /// </summary>
  internal UndoRedoManager<T>? UndoRedoManager { get; set; }
  ```

#### Task 5.2: Initialize UndoRedoManager in OnInitializedAsync
- [ ] Open file: `src/SfGrid.razor.cs`
- [ ] Locate `OnInitializedAsync()` method
- [ ] Add initialization at start of method:
  ```csharp
  // Initialize UndoRedo manager
  if (UndoRedoManager == null)
  {
      UndoRedoManager = new UndoRedoManager<T>();
  }
  ```
- [ ] Add comment

#### Task 5.3: Add Public API Methods
- [ ] Open file: `src/SfGrid.razor.cs`
- [ ] Locate public method section (end of class)
- [ ] Add public methods:
  ```csharp
  /// <summary>
  /// Undo the most recent edit operation.
  /// </summary>
  public async Task UndoAsync()
  {
      if (UndoRedoManager != null)
      {
          await UndoRedoManager.UndoAsync().ConfigureAwait(true);
      }
  }

  /// <summary>
  /// Redo the most recently undone operation.
  /// </summary>
  public async Task RedoAsync()
  {
      if (UndoRedoManager != null)
      {
          await UndoRedoManager.RedoAsync().ConfigureAwait(true);
      }
  }

  /// <summary>
  /// Undo all recorded operations to reach a clean state.
  /// </summary>
  public async Task UndoAllAsync()
  {
      if (UndoRedoManager != null)
      {
          await UndoRedoManager.UndoAllAsync().ConfigureAwait(true);
      }
  }

  /// <summary>
  /// Redo all undone operations.
  /// </summary>
  public async Task RedoAllAsync()
  {
      if (UndoRedoManager != null)
      {
          await UndoRedoManager.RedoAllAsync().ConfigureAwait(true);
      }
  }

  /// <summary>
  /// Clear both undo and redo stacks, resetting to clean state.
  /// </summary>
  public async Task ClearUndoRedoAsync()
  {
      if (UndoRedoManager != null)
      {
          UndoRedoManager.Clear();
      }
  }
  ```

#### Task 5.4: Add Public Properties
- [ ] Open file: `src/SfGrid.razor.cs`
- [ ] In public property section, add:
  ```csharp
  /// <summary>
  /// Returns the number of actions that can be undone.
  /// </summary>
  public int UndoCount => UndoRedoManager?.UndoCount ?? 0;

  /// <summary>
  /// Returns the number of actions that can be redone.
  /// </summary>
  public int RedoCount => UndoRedoManager?.RedoCount ?? 0;

  /// <summary>
  /// Returns true if there are actions that can be undone.
  /// </summary>
  public bool IsUndoAvailable => UndoRedoManager?.IsUndoAvailable ?? false;

  /// <summary>
  /// Returns true if there are actions that can be redone.
  /// </summary>
  public bool IsRedoAvailable => UndoRedoManager?.IsRedoAvailable ?? false;
  ```

---

### Phase 6: Undo/Redo State Restoration Logic

#### Task 6.1: Implement UndoAsync State Restoration
- [ ] Open file: `src/Internal/Actions/UndoRedoManager.cs`
- [ ] In `UndoAsync()` method, implement:
  - Check if `UndoStack.Count > 0`, return if empty
  - Pop action: `var action = UndoStack.Last()` then `UndoStack.RemoveLast()`
  - Route based on `action.ActionType`:
    - **CellEdit**: Restore old value to cell, mark clean
    - **RowAdd**: Remove row from grid
    - **RowDelete**: Restore row to grid at original index
  - Push to Redo: `RedoStack.AddLast(action)`
  - Trigger UI refresh: `StateHasChanged()` (via parent reference)
  - Update stack counters

#### Task 6.2: Implement RedoAsync State Restoration
- [ ] Open file: `src/Internal/Actions/UndoRedoManager.cs`
- [ ] In `RedoAsync()` method, implement:
  - Check if `RedoStack.Count > 0`, return if empty
  - Pop action: `var action = RedoStack.Last()` then `RedoStack.RemoveLast()`
  - Route based on `action.ActionType`:
    - **CellEdit**: Restore new value to cell
    - **RowAdd**: Add row back to grid at original position
    - **RowDelete**: Mark row as deleted
  - Push to Undo: `UndoStack.AddLast(action)`
  - Trigger UI refresh
  - Update stack counters

#### Task 6.3: Implement RecordAction
- [ ] Open file: `src/Internal/Actions/UndoRedoManager.cs`
- [ ] In `RecordAction()` method, implement:
  - Clear Redo stack: `RedoStack.Clear()`
  - Add to Undo: `UndoStack.AddLast(action)`
  - Enforce limit:
    - If `UndoStack.Count > MaxStackSize`
    - Remove oldest: `UndoStack.RemoveFirst()`
  - Update counters
  - Log action (Debug output)

---

### Phase 7: Validation & Safety Guards

#### Task 7.1: Add Guard Clauses
- [ ] In all state restoration code (UndoAsync, RedoAsync):
  - Verify row still exists: `Parent.Rows.FirstOrDefault(r => r.Index == action.RowIndex)`
  - Verify column still exists: `Parent.Columns.FirstOrDefault(c => c.Index == action.CellChange.ColumnIndex)`
  - Log warning if not found
  - Skip restoration if not found
- [ ] Add try-catch around each operation for error resilience

#### Task 7.2: Add Edit Mode Validation
- [ ] In keyboard handler (FocusHandler):
  - Check `Mode == EditMode.Batch` before executing undo/redo
  - Silently ignore if not in Batch mode
- [ ] In GridEditSettings:
  - Silently disable UndoRedo if Mode changes to Normal or Dialog

---

### Phase 8: Cross-Feature Safety

#### Task 8.1: Virtual Scroll Compatibility
- [ ] In `UndoAsync`/`RedoAsync`, after state restoration:
  - If rows added/removed, check if virtual scroll needs index update
  - Call `Parent.VirtualScrollModule?.RefreshVirtualRows()` if needed
  - Add comment: "Virtual scroll may need index recalculation"

#### Task 8.2: Aggregate Refresh
- [ ] In `UndoAsync`/`RedoAsync`, after row operations:
  - If aggregates enabled: `Parent.AggregateModule?.RefreshAggregates()`
  - Add comment: "Aggregates need recalculation after row changes"

#### Task 8.3: Selection State
- [ ] In state restoration, handle selection:
  - After row add/delete, update selection if needed
  - Don't change selected state (leave as-is)
  - Add comment: "Selection preserved across undo/redo"

---

### Phase 9: Documentation & Demo

#### Task 9.1: Add Inline Code Comments
- [ ] Review all new code for clarity
- [ ] Add XML documentation to all public members
- [ ] Add inline comments for complex logic

#### Task 9.2: Create or Update Demo
- [ ] Create demo file: `demos/UndoRedoDemo.razor` (or update existing if present)
- [ ] Demonstrate:
  - Enable/disable UndoRedo
  - Cell edits with undo/redo
  - Row add/delete with undo/redo
  - Stack status display (UndoCount, IsUndoAvailable)
  - Keyboard shortcuts (Ctrl+Z, Ctrl+Y)

---

### Phase 10: Compile & Verify

#### Task 10.1: Compile Solution
- [ ] Build solution: `dotnet build`
- [ ] Verify no errors or warnings from new code
- [ ] Fix any compiler errors

#### Task 10.2: Basic Functionality Test
- [ ] Test Ctrl+Z undo (manual)
- [ ] Test Ctrl+Y redo (manual)
- [ ] Test stack limit enforcement
- [ ] Test keyboard shortcuts ignored when not in Batch mode
- [ ] Verify no regressions in existing batch editing

---

## Dependencies & Sequencing

```
Phase 1 (UndoRedoAction + UndoRedoManager)
    ↓
Phase 2 (Keyboard Integration in FocusHandler)
    ↓
Phase 3 (Edit Integration hooks)
    ↓
Phase 4 (GridEditSettings configuration)
    ↓
Phase 5 (SfGrid public API)
    ↓
Phase 6 (State Restoration Logic)
    ↓
Phase 7-10 (Validation, Testing, Demo)
```

**All phases are independent after Phase 1 completes** — can work in parallel on different areas.

---

## Acceptance Criteria Checklist

### Functional
- [ ] Ctrl+Z undo works in Batch mode with grid focus
- [ ] Ctrl+Y / Ctrl+Shift+Z redo works in Batch mode with grid focus
- [ ] Cell edits undo/redo correctly (value restoration)
- [ ] Row additions undo/redo correctly (add/remove from grid)
- [ ] Row deletions undo/redo correctly (restore/delete)
- [ ] Multiple sequential operations work
- [ ] Empty stack operations (undo/redo when empty) do nothing
- [ ] Stack limit enforced (oldest actions removed)
- [ ] EnableUndoRedo property works (enable/disable)
- [ ] UndoRedoLimit property works (custom limits)

### Non-Functional
- [ ] No breaking changes to existing APIs
- [ ] Zero perf impact when `EnableUndoRedo=false` (default)
- [ ] Stack operations <5ms
- [ ] Memory bounded by UndoRedoLimit
- [ ] Clean code with documentation
- [ ] No compiler warnings

### Cross-Feature
- [ ] Works with Grouping
- [ ] Works with Virtualization
- [ ] Works with Frozen Columns
- [ ] Works with Paging
- [ ] Works with Filtering
- [ ] No regressions in batch edit features

---

## Schema: spec-driven

This change uses the **spec-driven workflow** which includes:
- proposal.md (this document's companion)
- design.md (architecture details)
- tasks.md (implementation checklist)
- Exploration.md (infrastructure findings)

---

**Last Updated**: August 12, 2026  
**Status**: Ready for implementation  
**Total Tasks**: 40+ implementation items across 10 phases
