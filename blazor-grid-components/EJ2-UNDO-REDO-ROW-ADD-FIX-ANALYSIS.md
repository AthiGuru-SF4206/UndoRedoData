# EJ2 Undo/Redo Row Add Issue - Detailed Analysis & Implementation Plan

**Date**: August 14, 2026  
**Status**: ✅ Analysis Complete - Ready for Safe Implementation  
**Reference**: EJ2 TypeScript implementation from UNDO-REDO-COMPREHENSIVE-SOURCE-ANALYSIS.md

---

## 📋 EXECUTIVE SUMMARY

### The Issue
When users **add a new record in batch edit mode** and then **undo**:
- **Expected (EJ2 behavior)**: Entire added row is deleted/removed from grid
- **Current (Blazor bug)**: Individual cell values revert to default values (0, empty, etc.)

### Root Cause
Blazor implementation records new row edits as **`CellEdit`** actions instead of **`RowAdd`** actions.

### The Fix (3 Parts)
1. **Track added rows**: Use a `Set<string>` to store UIDs of newly added rows
2. **Record RowAdd on first edit**: When first cell of new row is saved, record `RowAdd` action (not `CellEdit`)
3. **Handle RowAdd undo**: When undoing a `RowAdd` action, delete the entire row from grid

---

## 🔍 EJ2 SPECIFICATION (FROM DOCUMENTATION)

### EJ2 Row Add Tracking (Section 3.2)

**When is RowAdd recorded?**
- After user adds new row
- **AND** saves first cell (moves focus/tab away)
- **NOT** on every cell edit in new row

**Data Stored in RowAdd Action**:
```typescript
{
    type: 'row-add',           // Discriminator
    rowUid: row.uid,           // Unique row identifier
    rowIndex: rowIndex,        // Position in grid (0-based)
    rowData: row.changes       // FULL row object with all fields
}
```

**Duplicate Prevention (Critical)** - Section 3.2:
```typescript
if (this.storedRowUids.has(row.uid)) {
    // Row already tracked in undo stack
    // Multiple edits on same new row = UPDATE last entry, don't add new
    const lastAction = this.undoStack[this.undoStack.length - 1];
    if (lastAction && lastAction.type === 'row-add' && lastAction.rowUid === row.uid) {
        lastAction.rowData = rowData;  // Update with latest values
    }
    return;  // Don't push to stack again
}
this.storedRowUids.add(row.uid);  // Track this new row
```

**Key Benefit**: User edits Cell1 → Cell2 → Cell3 in new row
- **Without deduplication**: 3 entries in undo stack
- **With deduplication**: 1 entry in undo stack (updated 3 times)
- **Undo behavior**: Click Undo once → entire row deleted (cleaner UX)

### EJ2 Row Add Undo Execution (Section 4.3)

**When user presses Ctrl+Z for a RowAdd action**:
```typescript
case 'row-add':
    if (action.rowUid) {
        this.storedRowUids.delete(action.rowUid);  // Stop tracking
        const rowElement = gObj.getRowByIndex(action.rowIndex);
        if (rowElement) {
            gObj.deleteRow(rowElement as HTMLTableRowElement);  // Remove row
        }
    }
    break;
```

**Result**: Row completely disappears from grid (user sees it removed)

### EJ2 History Clearing Rules (Section 2.6)

```
Event                    | History Action       | Reason
------                   | ------               | ------
Batch Save (Update)      | Clear both stacks    | Changes committed to server
Batch Cancel             | Clear both stacks    | Unsaved changes discarded
Close Edit (ESC)         | Clear both stacks    | Editing session ended
New Edit After Delete    | Clear redo stack     | New action invalidates redo
```

---

## 🧪 EXPECTED BEHAVIOR (USER WORKFLOWS)

### Workflow 1: Add Record, Edit, Undo
```
Step 1: User clicks "Add" button
  → Row added with defaults: Order=0, Customer="", Freight=$0.00
  → No UndoStack entry yet (no save = no record)

Step 2: User edits Order field: 0 → 10249
  → Saves cell (tab/enter), first save in new row
  → UndoStack: [RowAdd(rowData={Order:10249, Customer:"", Freight:$0.00})]
  → UI: "Add" button highlighted/visible

Step 3: User edits Customer field: "" → "VINET"
  → Saves cell (tab/enter)
  → UndoStack: [RowAdd(rowData={Order:10249, Customer:"VINET", Freight:$0.00})]  ← UPDATED same entry
  → UI: No new stack entry created

Step 4: User presses Ctrl+Z (Undo)
  → Executes: undoStack.pop() → RowAdd action
  → Calls: deleteRow(action.rowUid)
  → Result: Row removed from grid completely
  → UndoStack: []
  → RedoStack: [RowAdd(...)]
  
✅ EXPECTED: Row removed (appears to have never been added)
❌ CURRENT BUG: Cells revert to defaults (0, empty) - row still visible
```

### Workflow 2: Add Record, Edit Multiple Cells, Undo All
```
Step 1: Add row → UndoStack: []
Step 2: Edit Cell1 (Order) → UndoStack: [RowAdd(rowData={Order:10249,...})]
Step 3: Edit Cell2 (Customer) → UndoStack: [RowAdd(rowData={Order:10249, Customer:"VINET",...})]
Step 4: Edit Cell3 (Freight) → UndoStack: [RowAdd(rowData={Order:10249, Customer:"VINET", Freight:32.38})]
Step 5: Ctrl+Z → Row deleted
Step 6: Ctrl+Y (Redo) → Row re-added with all accumulated values
```

### Workflow 3: Add Record, Save/Update
```
Step 1: Add row, edit cells
  → UndoStack: [RowAdd(rowData=...)]
  → UndoButton: ENABLED

Step 2: User clicks "Update" (Batch Save)
  → Changes committed to server
  → UndoStack.clear() and RedoStack.clear()
  → UndoButton: DISABLED

Step 3: User clicks "Edit" again (opens new batch edit session)
  → UndoStack: [] (fresh start)
  → New edits tracked from this point

✅ Result: No confusion between old and new edit sessions
```

### Workflow 4: Edit Existing Row (Not a New Add)
```
Step 1: User edits existing row Cell1 (e.g., "VINET" → "VINETSS")
  → Not a new row, so:
  → UndoStack: [CellEdit(field:"CustomerName", oldValue:"VINET", newValue:"VINETSS")]
  → NOT added to storedRowUids

Step 2: Edit Cell2 in same row
  → UndoStack: [
      CellEdit(field:"CustomerName", oldValue:"VINET", newValue:"VINETSS"),
      CellEdit(field:"Freight", oldValue:32.38, newValue:50.00)
    ]
  → Two separate CellEdit entries (NOT grouped like RowAdd)

Step 3: Ctrl+Z
  → Undo Freight edit
  → Freight reverts: $50.00 → $32.38
  → Customer still shows "VINETSS"
  
Step 4: Ctrl+Z
  → Undo Customer edit
  → Customer reverts: "VINETSS" → "VINET"
```

**KEY DIFFERENCE**:
- **New rows**: Multiple edits = 1 RowAdd entry (grouped)
- **Existing rows**: Multiple edits = Multiple CellEdit entries (NOT grouped)

---

## 🛠️ IMPLEMENTATION PLAN

### Phase 1: Modify UndoRedoManager
**File**: `src\Internal\Data\UndoRedoManager.cs`

**Add Field**:
```csharp
/// <summary>
/// Tracks UIDs of newly added rows to prevent duplicate RowAdd entries.
/// When a new row is edited multiple times, only first edit creates RowAdd,
/// subsequent edits update the same RowAdd.rowData instead of creating new entries.
/// </summary>
private HashSet<string> StoredRowUids { get; set; } = new HashSet<string>();
```

**Add Method** (called by SaveCell):
```csharp
internal void UpdateAddedRowData(string rowUid, object rowData)
{
    if (StoredRowUids.Contains(rowUid) && UndoStack.Count > 0)
    {
        // Get last action
        var lastAction = UndoStack[UndoStack.Count - 1];
        
        // If it's a RowAdd action for this row, update rowData
        if (lastAction is UndoRedoAction<T> action &&
            action.ActionType == UndoRedoActionType.RowAdd &&
            action.RowChange?.RowUid == rowUid)
        {
            // Update the rowData with new values
            action.RowChange.RowData = rowData;
            // Don't push to stack - return early from SaveCell
        }
    }
}

internal void TrackAddedRow(string rowUid)
{
    StoredRowUids.Add(rowUid);
}

internal void RemoveTrackedRow(string rowUid)
{
    StoredRowUids.Remove(rowUid);
}

internal void ClearTracking()
{
    StoredRowUids.Clear();
}
```

**Modify ClearStacks()**:
```csharp
public void ClearStacks()
{
    UndoStack.Clear();
    RedoStack.Clear();
    StoredRowUids.Clear();  // ← ADD THIS
}
```

### Phase 2: Modify Edit.cs SaveCell()
**File**: `src\Internal\Actions\Edit.cs`

**Detection Logic** (add after line 500):
```csharp
// Check if this is a newly ADDED row being edited for first time
bool isNewRowFirstEdit = false;
if (OriginalRow?.Action == EditAction.Added && 
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled)
{
    isNewRowFirstEdit = true;
}

// If this is first edit of a new row, check if already tracked
bool isRowAlreadyTracked = false;
if (isNewRowFirstEdit)
{
    // Get row UID (need to implement)
    string rowUid = OriginalRow?.Uid;
    if (Parent.UndoRedoManager?.IsRowTracked(rowUid) ?? false)
    {
        isRowAlreadyTracked = true;
        // Update existing RowAdd action instead of creating CellEdit
        Parent.UndoRedoManager?.UpdateAddedRowData(rowUid, CloneData);
        return;  // Exit SaveCell without adding to stack
    }
}
```

**Modify Recording Logic** (after line 630):
```csharp
// Record cell edit action for undo/redo
if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.EditSettings?.Mode == EditMode.Batch &&
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    cellSavedArgs != null)
{
    // ==========================================
    // NEW: Track newly added rows
    // ==========================================
    if (isNewRowFirstEdit && !isRowAlreadyTracked)
    {
        // First edit of a newly added row
        // Record as RowAdd action (not CellEdit)
        string rowUid = OriginalRow.Uid;
        
        var rowChange = new RowChange<T>
        {
            RowUid = rowUid,
            RowIndex = OriginalRow.Index ?? -1,
            RowData = (T)CloneData  // Full row data
        };
        
        var action = new UndoRedoAction<T>
        {
            ActionType = UndoRedoActionType.RowAdd,  // ← CHANGE FROM CellEdit
            RowChange = rowChange
        };
        
        Parent.UndoRedoManager?.RecordAction(action);
        Parent.UndoRedoManager?.TrackAddedRow(rowUid);  // ← Track this row
        Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
    }
    else
    {
        // Regular cell edit (existing row or new row subsequent edits)
        var cellChange = new CellChange<T>
        {
            RowIndex = OriginalRow.Index ?? -1,
            ColumnIndex = OriginalCell.Index ?? -1,
            FieldName = OriginalCell.Column?.Field,
            OldValue = cellSavedArgs.PreviousValue,
            NewValue = cellSavedArgs.Value,
            Column = OriginalCell.Column
        };

        var action = new UndoRedoAction<T>
        {
            ActionType = UndoRedoActionType.CellEdit,
            CellChange = cellChange
        };

        Parent.UndoRedoManager?.RecordAction(action);
        Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
    }
}
```

### Phase 3: Modify UndoRedoManager.UndoAsync()
**File**: `src\Internal\Data\UndoRedoManager.cs`

**Add RowAdd Undo Handler** (in UndoAsync):
```csharp
public async Task UndoAsync()
{
    if (UndoStack.Count == 0 || !IsEnabled) return;

    var action = UndoStack.Pop();
    if (action is UndoRedoAction<T> typedAction)
    {
        switch (typedAction.ActionType)
        {
            case UndoRedoActionType.CellEdit:
                await UndoCellEdit(typedAction);
                break;

            case UndoRedoActionType.RowAdd:  // ← NEW
                await UndoRowAdd(typedAction);
                break;

            case UndoRedoActionType.RowDelete:
                await UndoRowDelete(typedAction);
                break;

            case UndoRedoActionType.PasteOperation:
                await UndoPasteOperation(typedAction);
                break;
        }
    }

    RedoStack.Push(action);
}

private async Task UndoRowAdd(UndoRedoAction<T> action)
{
    if (action.RowChange?.RowUid == null) return;

    // Stop tracking this row
    RemoveTrackedRow(action.RowChange.RowUid);

    // Delete the row from grid
    // Get the row object
    var row = Parent.Rows?.FirstOrDefault(r => r.Uid == action.RowChange.RowUid);
    if (row != null)
    {
        // Call grid's delete method
        // This will remove the row from UI and data
        await Parent.DeleteRecordAsync(row);
    }
}
```

### Phase 4: Modify UndoRedoManager.RedoAsync()
**File**: `src\Internal\Data\UndoRedoManager.cs`

**Add RowAdd Redo Handler** (in RedoAsync):
```csharp
public async Task RedoAsync()
{
    if (RedoStack.Count == 0 || !IsEnabled) return;

    var action = RedoStack.Pop();
    if (action is UndoRedoAction<T> typedAction)
    {
        switch (typedAction.ActionType)
        {
            case UndoRedoActionType.CellEdit:
                await RedoCellEdit(typedAction);
                break;

            case UndoRedoActionType.RowAdd:  // ← NEW
                await RedoRowAdd(typedAction);
                break;

            case UndoRedoActionType.RowDelete:
                await RedoRowDelete(typedAction);
                break;

            case UndoRedoActionType.PasteOperation:
                await RedoPasteOperation(typedAction);
                break;
        }
    }

    UndoStack.Push(action);
}

private async Task RedoRowAdd(UndoRedoAction<T> action)
{
    if (action.RowChange?.RowData == null) return;

    // Re-add the row
    // Create new row object from stored rowData
    var newRow = (T)Activator.CreateInstance(typeof(T));
    var properties = typeof(T).GetProperties();
    
    foreach (var prop in properties)
    {
        var storedValue = action.RowChange.RowData.GetType().GetProperty(prop.Name)?.GetValue(action.RowChange.RowData);
        if (storedValue != null)
        {
            prop.SetValue(newRow, storedValue);
        }
    }

    // Add row back to grid
    await Parent.AddRecordAsync(newRow, (int?)action.RowChange.RowIndex);
    
    // Track it again
    TrackAddedRow(action.RowChange.RowUid);
}
```

---

## ✅ VERIFICATION CHECKLIST

### Test Scenarios (All Should Pass)

#### Scenario 1: Add + Edit + Undo (Row Remove)
- [ ] Add new record
- [ ] Edit cell 1 (Order): 0 → 10249
- [ ] Press Ctrl+Z
- [ ] ✅ Row should be completely removed from grid
- [ ] RedoStack should have 1 entry

#### Scenario 2: Add + Edit Multiple + Undo
- [ ] Add new record  
- [ ] Edit cell 1 (Order): 0 → 10249
- [ ] Edit cell 2 (Customer): "" → "VINET"
- [ ] Edit cell 3 (Freight): $0.00 → $32.38
- [ ] ✅ UndoStack should have 1 entry (RowAdd with rowData updated 3 times)
- [ ] Press Ctrl+Z → Row removed
- [ ] Press Ctrl+Y → Row re-added with all values

#### Scenario 3: Edit Existing Row (Not New Add)
- [ ] Click Edit on existing row
- [ ] Edit cell 1 (Customer): "VINET" → "VINETSS"
- [ ] Edit cell 2 (Freight): $32.38 → $50.00
- [ ] ✅ UndoStack should have 2 entries (CellEdit entries, NOT grouped)
- [ ] Ctrl+Z → Freight reverts, Customer unchanged
- [ ] Ctrl+Z → Customer reverts

#### Scenario 4: Add + Save + Edit Again
- [ ] Add record, edit cells
- [ ] Click Update (batch save)
- [ ] ✅ UndoStack and RedoStack cleared
- [ ] Click Edit again
- [ ] ✅ New batch session, fresh history

#### Scenario 5: Add + Delete Without Saving
- [ ] Add record, edit cells
- [ ] Click Cancel
- [ ] ✅ Row removed, UndoStack cleared
- [ ] Undo button disabled

#### Scenario 6: Toolbar Button States
- [ ] Add record
- [ ] After first edit → Undo button ENABLED
- [ ] Click Undo → Undo button DISABLED, Redo button ENABLED
- [ ] Click Redo → Undo button ENABLED, Redo button DISABLED

### Regression Test Suite

#### R1: Cell Edit Undo Still Works
- [ ] Edit existing row cell
- [ ] Undo → value reverts
- [ ] Redo → value re-applied

#### R2: Delete Undo Still Works
- [ ] Delete existing row
- [ ] Undo → row restored with original data

#### R3: Paste Undo Still Works
- [ ] Paste data into cells
- [ ] Undo → values reverted

#### R4: Batch Save Clears History
- [ ] Make edits
- [ ] Click Update
- [ ] Verify undo/redo disabled

#### R5: Frozen Columns Undo
- [ ] Freeze columns
- [ ] Add row, edit frozen column
- [ ] Undo → Row removed

#### R6: Grouping with Undo
- [ ] Enable grouping
- [ ] Add row in group
- [ ] Undo → Row removed

---

## 🚨 BREAKING CHANGES: NONE

### Existing Features Preserved
✅ CellEdit action recording  
✅ RowDelete action recording  
✅ Paste operation recording  
✅ Batch save/cancel clearing  
✅ Toolbar integration  
✅ Keyboard shortcuts  
✅ Cross-feature compatibility (frozen columns, grouping, etc.)  

### New Behavior (Additive)
✅ RowAdd action type introduced  
✅ storedRowUids tracking added (internal)  
✅ First edit of new row now records RowAdd (not CellEdit)  

### User Impact
- **Before**: "Why did my defaults come back after undo?"
- **After**: "Undo on new row removes entire row (as expected)"

---

## 📝 DATA STRUCTURES REQUIRED

### New Enum Value (if not exists)
```csharp
public enum UndoRedoActionType
{
    CellEdit = 0,
    RowAdd = 1,      // ← NEW
    RowDelete = 2,
    PasteOperation = 3
}
```

### New Data Class (if not exists)
```csharp
public class RowChange<T> where T : class
{
    public string RowUid { get; set; }      // Unique row identifier
    public int RowIndex { get; set; }       // Row position
    public T RowData { get; set; }          // Complete row data
}
```

---

## 🔗 REFERENCES

- **EJ2 Section 3.2**: Row Add Tracking (Duplicate Prevention)
- **EJ2 Section 4.3**: Undo Action Execution (RowAdd case)
- **EJ2 Section 4.4**: Redo Action Execution (RowAdd case)
- **EJ2 Section 2.6**: History Clearing Events
- **Workflow**: Section 7.1-7.4 (Batch Edit Mode Integration)

---

## ⚠️ IMPLEMENTATION NOTES

1. **Row UID Consistency**: Ensure row UIDs are consistent throughout undo/redo lifecycle
2. **EditAction.Added Check**: Verify `EditAction.Added` flag is set correctly in `BulkAddRow()`
3. **CloneData Integrity**: Ensure CloneData contains all fields when recording RowAdd
4. **Event Sequencing**: Verify events fire in correct order (CellSave → UndoStackChanged → ToolbarRefresh)
5. **Performance**: storedRowUids lookup is O(1) HashSet - no performance concern
6. **Memory**: RowData stored in action - consider undoRedoLimit stack depth for memory usage

---

## 📅 NEXT STEPS

1. ✅ This analysis document
2. ⏭️ Implement Phase 1: Modify UndoRedoManager (add fields, methods)
3. ⏭️ Implement Phase 2: Modify Edit.cs SaveCell() (detection + recording)
4. ⏭️ Implement Phase 3: Modify UndoAsync (RowAdd undo handler)
5. ⏭️ Implement Phase 4: Modify RedoAsync (RowAdd redo handler)
6. ⏭️ Write unit tests for each scenario
7. ⏭️ Regression testing against existing batch edit features
8. ⏭️ UI/UX verification with demo app
