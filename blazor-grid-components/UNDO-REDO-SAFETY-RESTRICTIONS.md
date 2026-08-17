# Undo/Redo Safety Restrictions & Code Change Guidelines

## Overview
This document outlines **strict restrictions** on existing code modifications made for the undo/redo feature to prevent unintended side effects.

---

## 🚫 Restricted Changes in Existing Code

### 1. **Edit.cs - SaveCell() Method (Lines 503-509)**

#### What Changed
```csharp
// ❌ OLD (Line 503-506): Overwrote PreviousVal incorrectly
var PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow!.Data!);
if (OriginalRow != null && OriginalRow.EditedData != null)
{
    PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow.EditedData); // BUG
}

// ✅ NEW (Line 508-509): Uses EditedData ?? Data correctly
var PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, 
    OriginalRow!.EditedData ?? OriginalRow!.Data);
```

#### RESTRICTION: This affects `CellSavedArgs.PreviousValue`
**CRITICAL**: Any code that listens to the `CellSaved` event and uses `CellSavedArgs.PreviousValue` will now see DIFFERENT VALUES.

```csharp
// Example: Event subscriber code
public async Task OnCellSaved(CellSavedArgs<OrderData> args)
{
    // args.PreviousValue will now reflect the intermediate edit value
    // Previously: Always got the original unedited value
    // Now: Gets the value from the previous edit (or original if first edit)
    
    // IMPACT: Any audit logging, change tracking, validation logic here
    // might behave differently!
}
```

#### If You Need to Revert This Change
**DO NOT revert without approval**, because:
- The old logic was a bug (undo/redo was broken)
- Many tests likely depend on the new behavior
- Event subscribers might have been updated

#### VALIDATION CHECKLIST
- [ ] Search codebase for `CellSavedArgs` event handlers
- [ ] Test all event handlers with multi-edit sequences
- [ ] Verify change tracking / audit logs work correctly
- [ ] Unit test: Edit cell 3 times, verify PreviousValue at each step

---

### 2. **Edit.cs - DeleteRecord() Method (Lines 670-715)**

#### What Changed
```csharp
// ❌ OLD (Line 642): Only used SelectionModule
var deletedRow = Parent.SelectionModule?.SelectedRow();

// ✅ NEW (Lines 672-715): Complex lookup with 2 strategies
// Strategy 1: Try to find row by data parameter + primary key matching
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

#### RESTRICTION: This changes the deletion mechanism
**WARNING**: Different code paths are now active. Potential issues:

1. **If primary keys are NOT configured**:
   - Strategy 1 fails silently
   - Falls back to SelectionModule
   - Behavior should be same as before, but is harder to debug

2. **If data parameter is null**:
   - Strategy 1 skipped
   - Falls back to SelectionModule
   - Behavior should be same as before

3. **If primary key matching finds wrong row** (duplicate keys):
   - Will delete WRONG row
   - Worst case scenario

#### If You Need to Revert This Change
**DO NOT revert without approval**, because:
- Old code couldn't find rows after selection was cleared
- New code works for undo/redo scenarios
- Tests were updated for new behavior

#### VALIDATION CHECKLIST
- [ ] Test DeleteRecord WITHOUT primary keys configured
- [ ] Test DeleteRecord with NULL data parameter
- [ ] Test DeleteRecord with multiple rows having same display value
- [ ] Test DeleteRecord when selection is cleared mid-operation
- [ ] Verify fallback to SelectionModule works correctly
- [ ] Unit test: DeleteRecord in all 3 strategy paths

---

### 3. **Edit.cs - UpdateCell() Method (Line 3023)**

#### What Changed
```csharp
// ❌ OLD
internal async Task UpdateCell(double rowIndex, string field, object value)

// ✅ NEW - Added optional parameter
internal async Task UpdateCell(double rowIndex, string field, object value, bool isUndoRedoAction = false)
```

And the implementation:
```csharp
// NEW LOGIC (Lines 3047-3048)
var sourceData = isUndoRedoAction ? Row.Data! : (Row.EditedData! ?? Row.Data!);
CloneRowData(sourceData);
```

#### RESTRICTION: This is a behavior-altering flag
**CRITICAL**: When `isUndoRedoAction = true`, clones from ORIGINAL data (`Row.Data`) instead of EDITED data.

```csharp
// Scenario: User edits cell A→B→C, then presses Undo
// With isUndoRedoAction = true:  CloneRowData uses original row (A)
// With isUndoRedoAction = false: CloneRowData uses edited row (B or C)
// This creates DIFFERENT cloned data!
```

#### If You Need to Call UpdateCell
**IMPORTANT**: 
- For normal editing: Call WITHOUT the flag (defaults to false)
- For undo/redo: Call WITH flag = true
- NEVER forget the flag when restoring undo values

#### If You Need to Revert This Change
**DO NOT revert without approval**, because:
- Undo/redo will completely break
- All undo tests will fail
- Cell restoration will restore wrong values

#### VALIDATION CHECKLIST
- [ ] Verify ALL UpdateCell calls from UndoRedoManager pass `isUndoRedoAction: true`
- [ ] Verify ALL normal UpdateCell calls (if any) do NOT pass the flag
- [ ] Unit test: UpdateCell with flag=true vs flag=false produce different results
- [ ] Code review: Search entire codebase for new UpdateCell calls

---

## ✅ Safe Changes (No Restrictions)

These changes are additive and don't affect existing code:

1. **RecordCellEditAction()** in SaveCell() - Lines 590-610
   - Guarded by `!isNewlyAddedRow`
   - Doesn't affect existing logic
   - ✅ Safe

2. **AddRecord() - RecordRowAddAction()** - Lines 914-924
   - New code path
   - Doesn't modify existing AddRecord behavior
   - ✅ Safe

3. **BatchDeleteRows() - RecordRowDeleteAction()** - Lines 1098-1115
   - Added after existing deletion code
   - Only records action, doesn't change deletion
   - ✅ Safe

4. **BatchClose() - ClearRedoStack()** - Lines 1128-1138
   - New code path on batch close/escape
   - Doesn't affect existing batch close behavior
   - ✅ Safe

---

## 🛡️ Safeguards in Place

### Feature Flag Enforcement
```csharp
// GridEditSettings.cs
public bool EnableUndoRedo { get; set; } = false; // ✅ Defaults OFF
```

Undo/redo logic only executes when:
1. EnableUndoRedo = true
2. EditMode = Batch
3. UndoRedoManager is enabled

### Guard Methods
```csharp
// UndoRedoManager.cs
internal bool ShouldRecordUndoRedoAction(EditMode expectedMode = EditMode.Batch)
{
    if (Parent?.EditSettings?.EnableUndoRedo != true) return false;
    if (expectedMode == EditMode.Batch && Parent.EditSettings?.Mode != EditMode.Batch) return false;
    if (isEnabled == false) return false;
    return true;
}
```

### Backward Compatibility
- All new parameters have defaults
- Feature is OFF by default
- Existing code paths unchanged when feature disabled

---

## 📋 Test Requirements Before Deployment

### Unit Tests (MUST HAVE)
```csharp
[TestClass]
public class UndoRedoIntegrationTests
{
    // Risk Area #1: PreviousVal in SaveCell
    [Test] public void SaveCell_MultiplEdits_PreviousValueCorrect() { }
    [Test] public void CellSaved_EventHandler_ReceivesCorrectPreviousValue() { }
    
    // Risk Area #2: DeleteRecord lookup
    [Test] public void DeleteRecord_WithoutPrimaryKeys_UsesFallback() { }
    [Test] public void DeleteRecord_WithPrimaryKeyMatch_FindsCorrectRow() { }
    [Test] public void DeleteRecord_WithNullData_UsesFallback() { }
    
    // Risk Area #3: UpdateCell flag
    [Test] public void UpdateCell_WithUndoRedoTrue_ClonesFromOriginal() { }
    [Test] public void UpdateCell_WithUndoRedoFalse_ClonesFromEdited() { }
}
```

### Integration Tests (SHOULD HAVE)
```csharp
[Test] public void Undo_MultipleEdits_RestoresCorrectly() { }
[Test] public void Undo_RowDelete_RestoresWithEdits() { }
[Test] public void Undo_RowAdd_RemovesRow() { }
[Test] public void Redo_AfterUndo_RestoresCorrectly() { }
[Test] public void Undo_WithVirtualization_HandlesRowIndices() { }
[Test] public void Undo_WithGrouping_RestoresCorrectLogicalPosition() { }
```

### Edge Case Tests (NICE TO HAVE)
```csharp
[Test] public void DeleteRecord_SelectionCleared_StillFindsRow() { }
[Test] public void UpdateCell_WithNullData_HandlesGracefully() { }
[Test] public void UndoRedo_StackOverflow_LimitedByMaxStackSize() { }
```

---

## 🚨 Critical Issues to Watch

### Issue #1: DeleteRecord Selection Cleared
**Scenario**: User deletes row while selection was cleared
**Old Behavior**: Might fail silently
**New Behavior**: Uses primary key lookup, should work
**Monitor**: Test this specific scenario

### Issue #2: PreviousValue Change
**Scenario**: Event handler expects PreviousValue = original value
**Old Behavior**: Always returned original (even if wrong)
**New Behavior**: Returns intermediate edit value
**Monitor**: Audit all CellSaved subscribers

### Issue #3: UpdateCell Flag Forgotten
**Scenario**: New code calls UpdateCell without isUndoRedoAction flag
**Old Behavior**: Would work but clone wrong data
**New Behavior**: Would restore wrong value
**Monitor**: Code review all UpdateCell calls, add static analyzer rule

---

## 🔒 Freeze Zones - DO NOT MODIFY

The following methods MUST NOT be modified for undo/redo without architecture review:

1. **Edit.cs - SaveCell()** (Lines 500-610)
   - Too many integration points
   - Changes here affect all cell editing

2. **Edit.cs - DeleteRecord()** (Lines 660-720)
   - Affects all row deletions
   - Changes to row lookup logic are high-risk

3. **Edit.cs - UpdateCell()** (Lines 3020-3070)
   - Used for ALL cell updates including undo/redo
   - Flag parameter is critical

---

## 📞 Escalation Path

If you need to modify these restricted areas:

1. **Post** your proposed change to architecture review
2. **Include** test cases for all scenarios
3. **Justify** why existing restrictions are insufficient
4. **Get approval** from **both** Grid Architecture + Feature Lead
5. **Update** this document with new restrictions
6. **Add** new unit tests before deployment

---

## Summary

✅ **What's Safe**: New additive code for undo/redo recording

⚠️ **What's Restricted**: 3 modifications to existing logic in SaveCell, DeleteRecord, UpdateCell

🚫 **What's Forbidden**: 
- Reverting the 3 restricted changes without approval
- Calling UpdateCell without understanding the isUndoRedoAction flag
- Using DeleteRecord without primary keys configured AND tested
- Modifying PreviousVal logic without checking event subscribers

🎯 **Bottom Line**: 
The undo/redo feature is well-integrated but has sharp edges. Treat these 3 areas as **read-only** unless you have explicit architecture approval and comprehensive test coverage.

