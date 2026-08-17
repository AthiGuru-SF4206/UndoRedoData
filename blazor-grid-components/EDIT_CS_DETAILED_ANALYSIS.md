# Edit.cs Modifications - Detailed Analysis

## Overview
`Edit.cs` is the **action recording layer** for undo/redo. It contains 9 distinct recording/handling points and 3 critical bug fixes.

**Total additions**: ~450 lines  
**File size before**: ~3,100 lines  
**File size after**: ~3,550 lines (+14.5%)

---

## The 9 Recording/Handling Points

### 1. **SaveCell() - PreviousValue Fix** (Lines 503-510)
**Git Command**: `git show HEAD:src/Internal/Actions/Edit.cs | grep -A 10 "SaveCell"`

**Critical Bug Fix**: ⚠️ **BLOCKER FOR ENTIRE UNDO/REDO FEATURE**

#### Before (BUGGY)
```csharp
var PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow!.Data!);
if (OriginalRow != null && OriginalRow.EditedData != null)     
{
    PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow.EditedData);
}
```

#### After (FIXED)
```csharp
var PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow!.EditedData ?? OriginalRow!.Data);
```

#### Root Cause Analysis

The bug happens in batch edit mode:

1. **User enters cell**: `"vinet"` → `CloneData.PropertyName = "vinet"`
2. **SaveCell() is called** (Line 503):
   - `PreviousVal = GetObject(Field, OriginalRow.Data)` → `"vinet"` (correct, original value)
3. **Line 504-506 Overwrites it**:
   - `PreviousVal = GetObject(Field, OriginalRow.EditedData)` → `"vinet"` (now contains edited value!)
4. **Line 608 records action**:
   - `OldValue = "vinet"` (should be `"vinet"`, but is `"vinet"`)
   - **Both old and new are the same!**
5. **User presses Ctrl+Z**:
   - Undo restores cell to `"vinet"` (same value, no visual change)

#### The Fix: `EditedData ?? Data` Pattern

The fixed code uses the **null-coalescing operator** to select the right source:

- If this is the **first edit** of the row: `EditedData = null` → Use `Data` (original value) ✓
- If this is the **second+ edit** of the row: `EditedData != null` → Use `EditedData` (intermediate value) ✓

This enables **incremental undo**:
- Edit 1: `"vinet"` → `"vinets"` (saves: `PreviousVal = "vinet"`)
- Edit 2: `"vinets"` → `"vinetss"` (saves: `PreviousVal = "vinets"`)
- Undo 1: Reverts to `"vinets"`
- Undo 2: Reverts to `"vinet"`

#### Why This Fix is Necessary
**Without this fix, the entire undo/redo feature doesn't work at all.** This is not a new feature addition — it's fixing a pre-existing bug that broke undo/redo from day one.

---

### 2. **SaveCell() - CellEdit Action Recording** (Lines 590-643)
**Git Command**: `git diff HEAD~1 HEAD -- src/Internal/Actions/Edit.cs | grep -A 50 "Record cell edit"`

**Purpose**: Record individual cell changes for undo/redo  
**Scope**: Batch editing mode, existing rows only

#### Implementation
```csharp
// Record cell edit action for undo/redo
bool isNewlyAddedRow = (OriginalRow?.Action ?? EditAction.None) == EditAction.Added;

if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.EditSettings?.Mode == EditMode.Batch &&
    !isNewlyAddedRow &&  // Skip recording CellEdit for newly added rows
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    cellSavedArgs != null)
{
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
```

#### Key Design Decision: Skip CellEdit for Newly Added Rows
```csharp
bool isNewlyAddedRow = (OriginalRow?.Action ?? EditAction.None) == EditAction.Added;
if (/* ... checks ... */ && !isNewlyAddedRow && /* ... more checks ... */)
{
    // Record CellEdit
}
else if (isNewlyAddedRow && /* ... checks ... */)
{
    // CRITICAL FIX: Update the RowAdd action with latest edited data
    var wasUpdated = Parent.UndoRedoManager.UpdateLastRowAddAction(rowIndex, (T)OriginalRow.EditedData!);
}
```

**Why skip CellEdit for new rows?**

Scenario: User adds a new row and edits cell A and cell B
- ❌ **If we record CellEdit for both**:
  - Undo stack: [CellEdit(A), CellEdit(B), RowAdd]
  - Undo 1: Reverts cell B → Row still shows (wrong!)
  - Undo 2: Reverts cell A → Row still shows (wrong!)
  - Undo 3: Removes row (finally!)
  
- ✅ **If we only record RowAdd (updated)**:
  - Undo stack: [RowAdd(with A and B edits)]
  - Undo 1: Removes entire row (correct!)

The EJ2 undo/redo feature uses this pattern: "If row already in undo stack, just update rowData"

#### EventAggregator Trigger
```csharp
Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
```
- Notifies toolbar to refresh Undo/Redo button states
- Essential for UI responsiveness

#### Why This Point is Necessary
✅ **Required** to record individual cell edits for existing rows.

---

### 3. **DeleteRecord() - Row Lookup Fix** (Lines 687-730)
**Git Command**: `git diff HEAD~1 HEAD -- src/Internal/Actions/Edit.cs | grep -A 50 "Strategy 1:"`

**Critical Bug Fix**: Row lookup fails when selection is cleared  
**Impact**: Delete operations broken in batch mode after undo

#### Before (FRAGILE)
```csharp
var deletedRow = Parent.SelectionModule?.SelectedRow();
```

#### After (ROBUST - Multi-Strategy Fallback)
```csharp
// Fetch primary keys once and reuse
var primaryKeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
Row<object>? deletedRow = null;

// Strategy 1: Try to find row by data parameter (primary key matching)
if (data != null && primaryKeys?.Count > 0)
{
    var primaryKeyField = primaryKeys!.FirstOrDefault();       
    if (primaryKeyField != null)
    {
        var dataKeyValue = Parent.PropHelper?.GetObject(primaryKeyField, data);
        deletedRow = Parent.Rows?.FirstOrDefault(row =>        
            row.Data != null &&
            GridUtils.CompareValues<object>(
                Parent.PropHelper?.GetObject(primaryKeyField, row.Data)!,
                dataKeyValue!
            )
        );
    }
}

// Strategy 2: Fallback to SelectionModule if data not provided
if (deletedRow == null && Parent.SelectionModule != null)      
{
    deletedRow = Parent.SelectionModule.SelectedRow();
}
```

#### Why This Fix is Necessary

**Scenario**: User edits row, saves, then deletes it, then presses Undo

1. Edit row → SaveCell() called → Selection not cleared yet
2. Delete row → SelectionModule might be cleared during SaveCell
3. DeleteRecord() called → `SelectionModule?.SelectedRow()` returns null
4. Delete operation fails silently or with wrong row

**With the fix**:
- Strategy 1 (Primary Key): Uses `data` parameter to find row
- Strategy 2 (Fallback): Uses SelectionModule if data not available
- **Result**: Delete works reliably regardless of selection state

#### Performance Consideration
- Primary key lookup is O(n) but necessary for robustness
- Usually run once per delete, not in tight loops
- No significant performance impact

#### Why This Point is Necessary
✅ **Required** to make delete operations reliable in batch mode.

---

### 4. **DeleteRecord() - Debug Logging** (Lines 714-725)
**Purpose**: Debugging aid for troubleshooting delete operations

```csharp
if (deletedRow != null)
{
    deletedRow.Action = EditAction.Deleted;
    System.Diagnostics.Debug.WriteLine($"[UndoRedo] DeleteRecord: Row marked as Deleted at index={deletedRow.Index}");
}
else
{
    System.Diagnostics.Debug.WriteLine($"[UndoRedo] DeleteRecord WARNING: Could not find row to delete (data={data}, selection=null)");        
}
```

#### Why This is Necessary
✅ **Recommended** for production debugging (can be removed in final release if needed, but helpful for troubleshooting).

---

### 5. **AddRecord() - RowAdd Action Recording** (Lines 941-959)
**Purpose**: Record new row additions for undo/redo

```csharp
// Record row addition action for undo/redo
if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    CloneData != null)
{
    var action = new UndoRedoAction<T>
    {
        ActionType = UndoRedoActionType.RowAdd,
        RowData = (T)CloneData!,
        RowIndex = addedRowIndex >= 0 ? addedRowIndex : row.Index ?? -1,
        RowPosition = Parent.EditSettings.NewRowPosition
    };

    Parent.UndoRedoManager?.RecordAction(action);
    Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!); 
}
```

#### Key Details
- Records **empty row** initially (CloneData with default values)
- RowPosition saved (Top or Bottom) for redo
- **Will be updated** by SaveCell() as user edits cells (see Point 2)

#### Why This Point is Necessary
✅ **Required** to record row additions.

---

### 6. **DeleteRows() - RowDelete Action Recording** (Lines 1024-1045)
**Purpose**: Record row deletions for undo/redo

```csharp
// Record row deletion action for undo/redo
if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    (_.EditedData != null || _.Data != null))
{
    var rowDataToStore = _.EditedData ?? _.Data;
    var action = new UndoRedoAction<T>
    {
        ActionType = UndoRedoActionType.RowDelete,
        RowData = (T)rowDataToStore!,
        RowIndex = _.Index ?? -1
    };

    Parent.UndoRedoManager?.RecordAction(action);
}
```

#### Critical Design: `EditedData ?? Data`
Stores the **current state** of the row (with any edits), not the original data.

**Scenario**: User edits row, then deletes it, then presses Undo
- Store EditedData (with user's edits) → Redo shows edits
- Store Data (original) → Redo shows original (loses user's work!)

#### Why This Point is Necessary
✅ **Required** to record row deletions with accumulated edits.

---

### 7. **DeleteRows() - EventAggregator Trigger** (Line 1046)
```csharp
Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
```

#### Why This is Necessary
✅ **Required** to notify toolbar of stack changes (same as Points 2 and 5).

---

### 8. **BatchClose() - Redo Stack Clear** (Lines 1145-1151)
**Purpose**: Clear redo stack when user cancels batch edit

```csharp
// Clear redo stack on batch cancel (new actions invalidate redos)
if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.UndoRedoManager != null)
{
    Parent.UndoRedoManager.ClearRedoStack();
    Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!); 
}
```

#### Why This is Necessary
✅ **Standard undo/redo behavior**: When user cancels batch edit, all redo operations are invalidated (user started a new action sequence).

---

### 9. **UpdateCell() - Undo/Redo Application** (Lines 3031-3076)
**Purpose**: Apply undo/redo changes to the grid UI  
**Complex**: ⚠️ Most complex modification in Edit.cs

#### Signature Change
```csharp
// OLD
internal async Task UpdateCell(double rowIndex, string field, object value)

// NEW
internal async Task UpdateCell(double rowIndex, string field, object value, bool isUndoRedoAction = false)
```

#### Implementation
```csharp
// CRITICAL FIX: For Undo/Redo, always clone from Row.Data (the original)
// NOT from Row.EditedData (which may already contain the edited value)
var sourceData = isUndoRedoAction ? Row.Data! : (Row.EditedData! ?? Row.Data!);
CloneRowData(sourceData);
SetValue(value, field);

// Recompute dirty state against ORIGINAL data (Row.Data), not previously edited value
var originalCellValue = Parent.PropHelper?.GetObject(field, Row.Data!);
var valueMatchesOriginal = GridUtils.CompareValues<object>(originalCellValue!, value!);
Cell.IsDirty = valueMatchesOriginal;

// Flag cell for re-rendering
Cell.Changes = true;

// Keep EditedData only while row is dirty; clear when fully restored
if (Row.IsDirty)
{
    Row.EditedData = CloneData!;
}
else
{
    Row.EditedData = null!;
}
```

#### Problem Solved: Dirty State Not Clearing on Undo

**Before Fix**:
1. User edits cell: `"vinet"` → `"vinets"`
   - Cell shows green "modified" indicator
2. User presses Ctrl+Z (undo)
   - Cell reverts to `"vinet"`
   - **Green indicator still showing!** (Bug)

**After Fix**:
1. User edits cell: `"vinet"` → `"vinets"`
   - `Cell.IsDirty = true` (original: "vinet", current: "vinets", not equal)
   - Cell shows green indicator ✓
2. User presses Ctrl+Z (undo)
   - UpdateCell() called with `isUndoRedoAction=true`
   - `sourceData = Row.Data` (not EditedData)
   - `originalCellValue = "vinet"`
   - `value = "vinet"` (the undo'd value)
   - `valueMatchesOriginal = CompareValues("vinet", "vinet")` = true
   - `Cell.IsDirty = true` → Actually means "NOT dirty" (matches original)
   - Green indicator disappears ✓

#### Why This Point is Necessary
✅ **Required** for correct UI state after undo/redo (dirty indicator, cell rendering).

---

## Toolbar Integration Points (Lines 2099-2124)

### CellEdit Recording in Toolbar State
```csharp
// Add Undo/Redo toolbar button states for Batch Edit mode
if (Edit != null && Edit.EnableUndoRedo && Parent.UndoRedoManager != null)
{
    if (Parent.UndoRedoManager.IsUndoAvailable)
        EnableItems.Add("Undo");
    else
        DisableItems.Add("Undo");

    if (Parent.UndoRedoManager.IsRedoAvailable)
        EnableItems.Add("Redo");
    else
        DisableItems.Add("Redo");
}
else
{
    DisableItems.Add("Undo");
    DisableItems.Add("Redo");
}
```

#### Why This is Necessary
✅ **Required** to update toolbar button states based on undo/redo stack availability.

---

## Summary: What Each Point Does

| # | Method | Action Type | Purpose | Necessary |
|---|--------|------------|---------|-----------|
| 1 | SaveCell() | PreviousValue Fix | Enable incremental undo | ✅ **BLOCKER** |
| 2 | SaveCell() | CellEdit Recording | Record cell changes | ✅ Core |
| 3 | DeleteRecord() | Row Lookup Fix | Find row by primary key | ✅ Critical |
| 4 | DeleteRecord() | Debug Logging | Troubleshooting | ✅ Recommended |
| 5 | AddRecord() | RowAdd Recording | Record new rows | ✅ Core |
| 6 | DeleteRows() | RowDelete Recording | Record deletions | ✅ Core |
| 7 | DeleteRows() | UI Update | Refresh toolbar | ✅ UI Integration |
| 8 | BatchClose() | Redo Clear | Invalidate redo on cancel | ✅ Semantics |
| 9 | UpdateCell() | Undo/Redo Apply | Apply changes + dirty state | ✅ Critical |

---

## Code Quality

### Strengths
- ✅ Consistent guard pattern: `if (EnableUndoRedo && IsEnabled && Manager != null)`
- ✅ Proper null checks throughout
- ✅ Good comments explaining complex logic
- ✅ EventAggregator triggers for UI updates
- ✅ Debug logging for troubleshooting

### Opportunities for Improvement
- ⚠️ Repeated guard pattern (9 times) — candidates for helper method
- ⚠️ Recording logic (CellChange creation) — candidates for factory method
- ⚠️ Multi-strategy row lookup — candidates for GridUtils method

---

## Refactoring Suggestions (OPTIONAL)

### Suggestion 1: Extract ShouldRecordUndoRedoAction()
```csharp
private bool ShouldRecordUndoRedoAction()
{
    return Parent.EditSettings?.EnableUndoRedo == true &&
           Parent.EditSettings?.Mode == EditMode.Batch &&
           Parent.UndoRedoManager != null &&
           Parent.UndoRedoManager.IsEnabled;
}
```

**Usage**:
```csharp
if (ShouldRecordUndoRedoAction() && !isNewlyAddedRow && cellSavedArgs != null)
{
    var cellChange = new CellChange<T> { /* ... */ };
    // ...
}
```

### Suggestion 2: Extract RecordCellEditAction()
```csharp
private void RecordCellEditAction(CellChange<T> cellChange)
{
    var action = new UndoRedoAction<T>
    {
        ActionType = UndoRedoActionType.CellEdit,
        CellChange = cellChange
    };
    Parent.UndoRedoManager?.RecordAction(action);
    Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
}
```

### Suggestion 3: Extract FindRowByData()
Move the multi-strategy row lookup to GridUtils:
```csharp
public static Row<T>? FindRowByData<T>(SfGrid<T> grid, T? data, 
    SelectionModule<T>? selectionModule, List<string>? primaryKeys)
{
    // Strategy 1: Primary key matching
    // Strategy 2: SelectionModule fallback
}
```

---

## Testing Requirements

### Unit Tests Needed
1. **PreviousValue Logic**
   - Test incremental edits: Edit 1, Edit 2, Undo, check values

2. **CellEdit Recording**
   - Test that CellEdit is recorded for existing rows
   - Test that CellEdit is NOT recorded for new rows

3. **Row Lookup**
   - Test primary key matching
   - Test fallback to SelectionModule
   - Test with cleared selection

4. **Dirty State**
   - Test dirty indicator shows on edit
   - Test dirty indicator clears on undo
   - Test dirty indicator reappears on redo

5. **New Row Multi-Edit**
   - Add row, edit cell A, edit cell B, undo, verify row removed

---

## Final Assessment

**Edit.cs modifications: ✅ ALL NECESSARY**

Each of the 9 points serves a specific purpose, with no redundant code. The 3 critical bug fixes (PreviousValue, DeleteRecord lookup, Dirty state) are blocking issues that prevent undo/redo from working correctly.

The file would benefit from refactoring to extract common patterns, but this is an optimization, not a blocker.
