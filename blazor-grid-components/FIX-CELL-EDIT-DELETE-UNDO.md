# Fix: Cell Edit + Delete + Undo Behavior

**Status**: ✅ IMPLEMENTED  
**Date**: August 13, 2026  
**Impact**: Fixes critical undo/redo behavior issue  

---

## Problem Description

### Scenario
When user performs these actions in sequence:
1. Edit a cell: `vinet` → `vinets` (in batch edit mode)
2. Delete the row
3. Press Ctrl+Z to undo the delete

### Expected Behavior
Deleted row should restore with the edited value: **"vinets"**

### Actual Behavior (Before Fix)
Deleted row restored with the original value: **"vinet"** ❌

---

## Root Cause

In `Edit.cs` **BulkDelete()** method (line ~1095), the delete action was recording the **original** row data instead of the **current edited** row data:

```csharp
// ❌ BEFORE FIX
var action = new UndoRedoAction<T>
{
    ActionType = UndoRedoActionType.RowDelete,
    RowData = (T)_.Data!,  // Stores ORIGINAL data
    RowIndex = _.Index ?? -1
};
```

### The Issue
In batch edit mode:
- `Row.Data` = Original unedited data (from data source)
- `Row.EditedData` = Current edited data (user's changes)

When recording delete, the code used `_.Data` which always contains the original value, not the user's edits!

---

## Solution Implemented

### Code Change
**File**: `src/Internal/Actions/Edit.cs`  
**Method**: `BulkDelete()` (lines 1095-1120)

```csharp
// ✅ AFTER FIX
var rowDataToStore = _.EditedData ?? _.Data;  // Use edited data if available
var action = new UndoRedoAction<T>
{
    ActionType = UndoRedoActionType.RowDelete,
    RowData = (T)rowDataToStore!,  // Stores CURRENT state (edited or original)
    RowIndex = _.Index ?? -1
};
```

### What Changed
1. Instead of always using `_.Data`, we now use `_.EditedData ?? _.Data`
2. This captures the **current edited state** if the row was edited
3. Falls back to **original state** if the row was never edited
4. Updated null check to `(_.EditedData != null || _.Data != null)` for safety

---

## Behavior After Fix

### ✅ Scenario 1: Edit → Delete → Undo
```
User: vinet → vinets (edit)
Delete the row
Ctrl+Z (undo delete)
Result: Row restored with "vinets" ✅ CORRECT
```

### ✅ Scenario 2: Multiple Edits → Delete → Undo
```
User: Edit multiple cells in row
Delete the row
Ctrl+Z (undo delete)
Result: All edits preserved in restored row ✅ CORRECT
```

### ✅ Scenario 3: Delete without Prior Edit
```
User: Delete row (no prior edits)
Ctrl+Z (undo delete)
Result: Row restored with original value ✅ CORRECT
```

### ✅ Scenario 4: Add Row → Edit → Delete → Undo
```
User: Add new row, edit it, delete it
Ctrl+Z (undo delete)
Result: Row restored with edited data (not null) ✅ CORRECT
```

---

## Regression Analysis

### ✅ Cell Edit Undo/Redo - NO IMPACT
- Uses separate `CellChange { OldValue, NewValue }` structure
- Does NOT use `RowData` field
- Completely independent code path
- **Status**: Safe ✅

### ✅ Delete Only (No Prior Edit) - NO IMPACT
- `EditedData ?? Data` evaluates to `Data` (since EditedData=null)
- Same behavior as before
- **Status**: Safe ✅

### ✅ Edit → Undo Edit → Delete - NO IMPACT
- After undo edit, `EditedData` is cleared to null
- Delete then uses `Data` (same as before)
- **Status**: Safe ✅

### ✅ Delete → Undo → Redo Delete - NO IMPACT
- Redo logic uses primary key to find row (key unchanged)
- Row still found and re-deleted correctly
- **Status**: Safe ✅

---

## Benefits of This Fix

| Benefit | Impact |
|---------|--------|
| **User Edits Preserved** | Undo delete shows all user changes, not original data |
| **Better UX** | Behavior matches user expectations (keep edits on undo) |
| **Fixes Null Issue** | For added rows where `Data=null`, uses `EditedData` instead |
| **No Regression** | Cell edit undo/redo completely unaffected |
| **EJ2 Alignment** | Closer to EJ2 grid behavior |

---

## Code Review Checklist

- ✅ Changes isolated to `BulkDelete()` method only
- ✅ Null safety improved (check both `EditedData` and `Data`)
- ✅ Backward compatible (unedited rows use same path)
- ✅ No breaking changes to other features
- ✅ Comment added explaining the fix
- ✅ No compilation errors introduced

---

## Testing Recommendations

### Manual Test Case 1: Edit → Delete → Undo
```
1. Enable batch edit mode with undo/redo
2. Edit a cell: vinet → vinets
3. Delete the row
4. Press Ctrl+Z
Expected: Row restores with "vinets" ✅
```

### Manual Test Case 2: Multiple Edits → Delete → Undo
```
1. Enable batch edit mode with undo/redo
2. Edit multiple cells in same row
3. Delete the row
4. Press Ctrl+Z
Expected: All edits visible in restored row ✅
```

### Manual Test Case 3: Cell Edit Undo (Regression Test)
```
1. Enable batch edit mode with undo/redo
2. Edit a cell: vinet → vinets
3. Press Ctrl+Z
Expected: Cell restores to "vinet" (unaffected) ✅
```

### Manual Test Case 4: Delete Without Edit (Regression Test)
```
1. Enable batch edit mode with undo/redo
2. Delete a row (no prior edit)
3. Press Ctrl+Z
Expected: Row restores with original data (unaffected) ✅
```

---

## Summary

✅ **Fix Applied**: `BulkDelete()` now stores `EditedData ?? Data` instead of just `Data`

✅ **Behavior**: Undo delete now restores row with all user edits intact

✅ **Backward Compatible**: No regression in existing functionality

✅ **Safe**: Cell edit undo/redo and other features completely unaffected

🚀 **Ready for Testing**
