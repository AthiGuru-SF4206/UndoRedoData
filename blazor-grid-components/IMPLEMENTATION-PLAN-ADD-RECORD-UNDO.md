# Implementation Plan: Fix Add Record Undo Behavior

**Date**: August 14, 2026  
**Issue**: When adding a new record in batch edit mode and then undoing, cells revert to defaults instead of removing the entire row  
**Solution**: Suppress CellEdit undo actions for newly added rows

---

## Issue Summary

### Current Behavior (BROKEN)
```
1. Click Add → Row appears with defaults (0, empty, 0)
2. Edit cells → Row changes to (100, "John", 50)
3. Press Ctrl+Z (Undo) → Row reverts to (0, empty, 0) ❌
```

### Expected Behavior (AFTER FIX)
```
1. Click Add → Row appears with defaults (0, empty, 0)
2. Edit cells → Row changes to (100, "John", 50)
3. Press Ctrl+Z (Undo) → Row completely removed ✅
```

---

## Root Cause Analysis

In `SaveCell()` method (Edit.cs, line ~508):

```csharp
var PreviousVal = Parent.PropHelper?.GetObject(..., OriginalRow!.EditedData ?? OriginalRow!.Data);
```

**For newly added rows**:
- `Row.EditedData` is set to default data (same as Row.Data)
- First cell edit gets: `OldValue = "0"` (default), `NewValue = "100"` (edited)
- Undo restores to "0" instead of removing row

---

## Implementation Strategy

### Change 1: Suppress CellEdit Recording for Newly Added Rows

**File**: `src/Internal/Actions/Edit.cs`  
**Method**: `SaveCell()`  
**Location**: Line ~610-615 (after CellSavedArgs creation)

**Current Code**:
```csharp
// Record cell edit action for undo/redo
if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.EditSettings?.Mode == EditMode.Batch &&
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    cellSavedArgs != null)
{
    var cellChange = new CellChange<T> { ... };
    var action = new UndoRedoAction<T> { ... };
    Parent.UndoRedoManager?.RecordAction(action);
}
```

**New Code**:
```csharp
// Record cell edit action for undo/redo
// IMPORTANT: Skip recording CellEdit for newly added rows
// Newly added rows should only have RowAdd action, so undo removes entire row
bool isNewlyAddedRow = OriginalRow?.Action == EditAction.Added;

if (Parent.EditSettings?.EnableUndoRedo == true &&
    Parent.EditSettings?.Mode == EditMode.Batch &&
    !isNewlyAddedRow &&  // ← NEW: Skip for newly added rows
    Parent.UndoRedoManager != null &&
    Parent.UndoRedoManager.IsEnabled &&
    cellSavedArgs != null)
{
    var cellChange = new CellChange<T> { ... };
    var action = new UndoRedoAction<T> { ... };
    Parent.UndoRedoManager?.RecordAction(action);
}
```

### Rationale for This Approach

1. **Cleaner Undo History**: One Undo action (RowAdd) instead of multiple (RowAdd + N×CellEdit)
2. **Better UX**: One undo completely removes the unwanted row
3. **Matches EJ2 Behavior**: Aligns with JavaScript implementation
4. **Preserves EditedData**: Even though we don't record CellEdit, EditedData is still updated
   - This ensures: Add → Edit → Delete → Undo correctly restores edited values

---

## Impact Analysis

### ✅ What This Fixes
- Add record → Edit cells → Undo now works correctly
- Row is removed completely (not reverted to defaults)
- Matches user expectation and EJ2 behavior

### ✅ No Regressions
- **Regular cell edits** (non-added rows): Unchanged - still record CellEdit actions
- **Delete undo**: Unchanged - uses RowDelete action (separate)
- **Edit → Delete → Undo**: Still works correctly (EditedData is preserved)
- **Batch save**: Unchanged - all changes still saved correctly

### ⚠️ Behavior Changes (INTENTIONAL)
- **New Behavior**: Newly added rows cannot have granular CellEdit undo
  - Before: Add → Edit → Undo reverts cell to default
  - After: Add → Edit → Undo removes entire row
  - **This is the CORRECT behavior**

---

## Testing Requirements

### Test Case 1: Simple Add + Edit + Undo
```csharp
1. Click Add button
2. Edit first cell (e.g., 0 → 100)
3. Press Ctrl+Z
✅ Expected: Row completely removed
```

### Test Case 2: Add + Multiple Edits + Undo
```csharp
1. Click Add button
2. Edit 3 cells (defaults → new values)
3. Press Ctrl+Z (once)
✅ Expected: Row completely removed
```

### Test Case 3: Existing Row Edit + Undo (REGRESSION TEST)
```csharp
1. Existing row with value "vinet"
2. Edit cell to "vinets"
3. Press Ctrl+Z
✅ Expected: Cell reverts to "vinet"
✅ Row NOT removed
```

### Test Case 4: Add + Edit + Delete + Undo (REGRESSION TEST)
```csharp
1. Click Add button
2. Edit cells (defaults → new values)
3. Delete row
4. Press Ctrl+Z
✅ Expected: Row restored with edited values (not defaults)
```

### Test Case 5: Toolbar Undo Button
```csharp
1. Add row → Edit cells
2. Click toolbar Undo button
✅ Expected: Row removed
```

### Test Case 6: Keyboard Shortcut
```csharp
1. Add row → Edit cells
2. Press Ctrl+Z
✅ Expected: Row removed
```

---

## Code Changes Summary

| File | Method | Line | Change | Impact |
|------|--------|------|--------|--------|
| `Edit.cs` | `SaveCell()` | ~610 | Add `isNewlyAddedRow` check | Suppress CellEdit for added rows |

---

## Verification Checklist

- [ ] Code compiles without errors
- [ ] All existing tests pass
- [ ] New scenario tests pass (add + edit + undo)
- [ ] Regression tests pass (existing row edits still work)
- [ ] Delete → Undo still restores edited values
- [ ] Toolbar undo button works
- [ ] Keyboard Ctrl+Z works
- [ ] No memory leaks in undo/redo stacks
- [ ] Performance not degraded

---

## Rollback Plan

If issues arise:
1. Remove the `!isNewlyAddedRow` condition
2. Revert to recording all CellEdit actions
3. Original behavior restored (with the old bug)

---

## Future Enhancements

1. Consider adding a configuration option to allow granular undo
2. Add UI indicator showing how many undo levels are available
3. Add logging/diagnostics for undo/redo operations
