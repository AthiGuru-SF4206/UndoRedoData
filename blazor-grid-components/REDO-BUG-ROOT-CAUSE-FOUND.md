# REDO BUG: ROOT CAUSE IDENTIFIED ✓

## Summary
**The Redo is not working because `UpdateCell()` updates `CloneData` but NEVER updates `Row.Data`.**

When comparing IsDirty state, the code compares the updated CloneData against the unchanged Row.Data, which creates incorrect dirty flags.

---

## The Critical Code Section

**File:** `src/Internal/Actions/Edit.cs`  
**Method:** `UpdateCell()`  
**Lines:** 3091-3155

```csharp
internal async Task UpdateCell(double rowIndex, string field, object value)
{
    var Row = Parent.Rows?.Find(_ => _.Index == rowIndex);
    var Cell = Row?.Cells?.Find(_ => _.Column?.Field?.Equals(field, StringComparison.Ordinal) == true);
    
    if (Row != null && Cell != null)
    {
        CloneRowData(Row.EditedData! ?? Row.Data!);  // Line 3115: Clone from Row
        SetValue(value, field);                       // Line 3116: Update CloneData only!
        
        // BUG HERE: Row.Data is never updated!
        var originalCellValue = Parent.PropHelper?.GetObject(field, Row.Data!);
        var valueMatchesOriginal = GridUtils.CompareValues<object>(originalCellValue!, value!);
        Cell.IsDirty = !valueMatchesOriginal;
        
        Cell.Changes = true;
        
        if (Row.IsDirty)
        {
            Row.EditedData = CloneData!;  // Line 3136: Only set if dirty
        }
        else
        {
            Row.EditedData = null!;       // Line 3138: Clear if not dirty
        }
        
        HasBatchChanges = Parent.Rows?.Any(r => r.IsDirty) ?? false;
        Parent.SoftRefresh = true;
        Parent.EventAggregator.Trigger("ContentStateChanged", null!);
        Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
    }
}
```

---

## The Problem Explained

### Scenario: User edits a cell from "Old" → "New"

#### Step 1: Initial Edit & Save
```
Row.Data = { field: "Old" }
CloneData = { field: "Old" }  (cloned from Row.Data)
User types: "New"
CloneData = { field: "New" }
SaveCell() records:
  OldValue = "Old" ✓
  NewValue = "New" ✓
```

#### Step 2: User Presses Ctrl+Z (Undo)
```
UpdateCell(rowIndex, field, "Old") is called
  CloneData = { field: "New" }  (cloned from Row.Data or Row.EditedData)
  SetValue("Old", field)
  CloneData = { field: "Old" }  ← Updated ✓
  
  PROBLEM:
  Row.Data = { field: "New" }   ← NEVER UPDATED! ✗
  
  originalCellValue = Row.Data.field = "New"
  valueMatchesOriginal = CompareValues("New", "Old") = FALSE
  Cell.IsDirty = !FALSE = TRUE  ← WRONG! Should be FALSE after undo
```

**Result:** The undo visually restores the cell but marks it as still dirty (wrong green indicator).

#### Step 3: User Presses Ctrl+Y (Redo)
```
UpdateCell(rowIndex, field, "New") is called
  CloneData = { field: "Old" }  (cloned from Row.Data or Row.EditedData)
  SetValue("New", field)
  CloneData = { field: "New" }  ← Updated ✓
  
  STILL PROBLEM:
  Row.Data = { field: "New" }   ← Still never updated ✗
  
  originalCellValue = Row.Data.field = "New"
  valueMatchesOriginal = CompareValues("New", "New") = TRUE
  Cell.IsDirty = !TRUE = FALSE  ← WRONG! Should be TRUE after redo
```

**Result:** The redo applies the value but marks it as NOT dirty (wrong - no green indicator).

---

## Why Redo "Doesn't Work"

The user sees:
1. ✓ Edit "Old" → "New" → Cell shows green (dirty)
2. ✓ Ctrl+Z → Cell shows "Old" but still green (incorrect state)
3. ✗ Ctrl+Y → Cell shows "New" but NOT green (incorrect state, looks unchanged)

The cell VALUE is restored correctly by `SetValue()`, but the **UI state (dirty indicator) is wrong**, making it appear like nothing happened.

---

## The Fix

### Option A: Update Row.Data after SetValue()

```csharp
internal async Task UpdateCell(double rowIndex, string field, object value)
{
    var Row = Parent.Rows?.Find(_ => _.Index == rowIndex);
    var Cell = Row?.Cells?.Find(_ => _.Column?.Field?.Equals(field, StringComparison.Ordinal) == true);
    
    if (Row != null && Cell != null)
    {
        CloneRowData(Row.EditedData! ?? Row.Data!);
        SetValue(value, field);
        
        // FIX: Copy the updated CloneData back to Row.Data
        CloneUtils.CloneObjectProperties(CloneData!, Row.Data!);
        
        var originalCellValue = value;  // Now this is the actual current value
        var valueMatchesOriginal = GridUtils.CompareValues<object>(originalCellValue!, value!);
        Cell.IsDirty = !valueMatchesOriginal;
        
        // ... rest of method
    }
}
```

**But this breaks the normal edit flow!** The Row.Data should stay as the original, not be updated until Save.

### Option B: Store the True Original Value Separately (RECOMMENDED)

Add a field to track the actual original row data:

```csharp
private T? _originalRowDataBeforeEdits;  // Store before ANY edits in batch mode

// When starting edit:
_originalRowDataBeforeEdits = CloneUtils.Clone(OriginalRow.Data);

// In UpdateCell():
internal async Task UpdateCell(double rowIndex, string field, object value)
{
    var Row = Parent.Rows?.Find(_ => _.Index == rowIndex);
    var Cell = Row?.Cells?.Find(_ => _.Column?.Field?.Equals(field, StringComparison.Ordinal) == true);
    
    if (Row != null && Cell != null)
    {
        CloneRowData(Row.EditedData! ?? Row.Data!);
        SetValue(value, field);
        
        // FIX: Compare against the REAL original, not the potentially modified Row.Data
        var realOriginalValue = Parent.PropHelper?.GetObject(field, _originalRowDataBeforeEdits!);
        var valueMatchesOriginal = GridUtils.CompareValues<object>(realOriginalValue!, value!);
        Cell.IsDirty = !valueMatchesOriginal;
        
        // ... rest of method
    }
}
```

### Option C: Just Use the Value Being Applied (Simplest)

```csharp
// For undo: comparing RestoreTo("Old") against "Old" = true → not dirty ✓
// For redo: comparing RestoreTo("New") against "New" = true → dirty ✓

// Wait, this won't work either because we need to know what the original was...
```

---

## RECOMMENDATION

**Option B is the best solution** because:

1. ✅ It preserves the Row.Data as the "current" state (for normal edits)
2. ✅ It tracks the "true original" for comparison
3. ✅ Undo correctly marks cells as not dirty (matches original)
4. ✅ Redo correctly marks cells as dirty (differs from original)
5. ✅ No side effects on normal edit flow
6. ✅ Works with all undo/redo scenarios

---

## Implementation Steps

1. Add `_originalRowDataBeforeEdits` field to EditModule
2. Initialize it when starting first edit of a row in batch mode
3. Clear it when batch save or cancel
4. Use it in UpdateCell() for IsDirty comparison
5. Test: Edit → Undo → Redo should all show correct dirty state

---

## Test Case to Verify Fix

```csharp
// Start: Row = { Name: "John", Age: 25 }

// Edit 1: Change Name to "Jane"
// Expected: Cell shows green (dirty)
// Verify: Cell.IsDirty = true

// Ctrl+Z: Undo to "John"
// Expected: Cell shows "John" and NOT green
// Verify: Cell.IsDirty = false

// Ctrl+Y: Redo to "Jane"
// Expected: Cell shows "Jane" and green
// Verify: Cell.IsDirty = true

// Edit 2: Change Name to "Jack"
// Expected: Cell shows green (still dirty)
// Verify: Cell.IsDirty = true

// Ctrl+Z: Undo to "Jane"
// Expected: Cell shows "Jane" and green (because differs from original "John")
// Verify: Cell.IsDirty = true
```

---

## Files to Modify

- `src/Internal/Actions/Edit.cs`
  - Add field: `private T? _originalRowDataBeforeEdits;`
  - Modify method: `UpdateCell()` to use `_originalRowDataBeforeEdits` for IsDirty comparison
  - Modify method: `EditCell()` to initialize `_originalRowDataBeforeEdits`
  - Modify method: `BatchSave()` / `CloseEdit()` to clear `_originalRowDataBeforeEdits`

---

**Status:** Ready to implement  
**Complexity:** Medium (1-2 hours)  
**Risk:** Low (isolated to UpdateCell logic)  
**Testing:** High (requires comprehensive undo/redo test cases)
