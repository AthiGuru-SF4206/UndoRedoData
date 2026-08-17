# PR: BLAZ-73262 — Command column cells not selected in Flow cell selection with Ctrl+Shift

**Branch:** `bugfix/blaz-73262-flow-selection-command-column`  
**Target:** `develop`  
**Merge strategy:** Squash merge

---

### Bug / Feature Description

Command column cells in intermediate rows are not selected when using Flow cell selection mode with Ctrl+Shift range selection in the Syncfusion Blazor DataGrid.

Task: https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/73262  
Ticket: https://es-testingportal.bolddesk.com/agent/tickets/73262

---

### Root Cause (Bug fixes only)

**File:** `Internal/Actions/Selection.cs`  
**Method:** `IterateCells`  
**Branch:** `else if (!isStartRow && !isEndRow)` — the middle-row path for Flow cell selection mode

The guard condition that determines which cells to select in middle rows (rows that are neither the start nor end row of a range selection) was:

```csharp
_cell?.IsDataCell == true || _cell?.Column?.Type.Equals(ColumnType.CheckBox) == true
```

Command column cells are generated with `CellType.CommandColumn` by `RowModelGenerator` (lines 211, 225) and `GroupModelGenerator` (line 651). Their `IsDataCell` property is never set to `true`, and they are not `ColumnType.CheckBox`. As a result, they were silently excluded from the selection loop for all middle rows.

The `isStartRow` and `isEndRow` branches have **no `IsDataCell` filter** — they select cells based solely on index bounds — which is why only middle-row command cells were affected.

---

### Solution Description

Added `|| _cell?.CellType == CellType.CommandColumn` to the middle-row guard condition in `IterateCells`:

```csharp
// Before
if ((_cell?.IsDataCell == true || _cell?.Column?.Type.Equals(ColumnType.CheckBox) == true)
    && _row?.Cells?.Where(x => x.IsSelected).Count() != _row?.Cells?.Count)

// After
if ((_cell?.IsDataCell == true
    || _cell?.Column?.Type.Equals(ColumnType.CheckBox) == true
    || _cell?.CellType == CellType.CommandColumn)
    && _row?.Cells?.Where(x => x.IsSelected).Count() != _row?.Cells?.Count)
```

This explicitly allows `CellType.CommandColumn` cells to participate in Flow mode range selection for middle rows, consistent with the behavior already present for start and end rows. All other cell type exclusions (Indent, Detail, RowDrag) are unaffected because none of them have `CellType.CommandColumn`.

---

### AI Log Details (if Code Studio was used)

**Root Cause Identification:**  
`IterateCells` in `Selection.cs` — the `!isStartRow && !isEndRow` path for Flow mode silently excludes command column cells because the guard condition only allows `IsDataCell == true` or `ColumnType.CheckBox`. Command column cells have `CellType.CommandColumn` and `IsDataCell = false` as set by `RowModelGenerator`.

**Why This is Wrong:**  
The start and end row branches in the same method apply no `IsDataCell` filter — they include all cells within the index range, including command column cells. The inconsistency causes command column cells to be selected correctly in only the first and last row of a multi-row range, while being silently skipped in every middle row.

**The Fix:**  
Extend the middle-row guard to also include `CellType.CommandColumn`. This is a minimal, targeted change that aligns middle-row behaviour with start/end row behaviour without affecting any other cell type exclusion logic.

---

### Code Studio Usage (Mandatory)

* Code Studio used in this PR?
    - [x] Yes
    - [ ] No
* Primary use (choose one):
    - [ ] Generate new code
    - [ ] Refactor/improve existing code
    - [x] Tests
    - [x] Bug fix / debugging help
    - [ ] Docs / comments
    - [ ] Review assistance
* Outcome:
    - [x] Saved time
    - [ ] Neutral
    - [ ] Cost time

---

### Impact Assessment

* [x] Low  — Single feature, minimal user impact
* [ ] Medium — Multiple features, moderate user impact
* [ ] High  — Critical functionality, significant user impact

> Scope is limited to `IterateCells` in `Selection.cs`. Only the Flow mode middle-row cell enumeration path is changed. No other selection path (Box mode, start row, end row, row-only mode, Both mode) is touched.

---

### Areas Tested

* [x] Tested using standard test cases
* [ ] Tested against feature matrix
* [ ] NA

> BUnit test file added: `Bunit/CR Issues/BLAZ-73262.razor` — 3 fixtures covering:
> 1. Core bug: Ctrl+Shift click range — command column cells selected in middle rows
> 2. Public API: `SelectCellsByRangeAsync` — command column cells included for all rows
> 3. Non-regression: Flow mode without command column — data cells still selected correctly in middle rows

---

### Breaking Changes

* [ ] Yes (Tag `breaking-issue`, provide migration guidance)
* [x] No

---

### Regression Testing

* [x] Verified fix does not reintroduce previous bugs
* [x] Checked edge cases and error scenarios

> Verified the following combinations are unaffected:
> - Flow mode — single-row selection (start row == end row path, no change)
> - Flow mode — two-row selection (start row and end row only, no middle rows)
> - Box mode — entirely separate `IsCellBox()` branch, no change
> - CheckBox column selection — still explicitly allowed by its own condition
> - Indent / Detail / RowDrag cells — none have `CellType.CommandColumn`, remain excluded
> - Row-only selection mode — `IterateCells` is not reached in row mode
> - Both (row + cell) mode — `IterateCells` path unchanged

---

### Action to Prevent Recurrence

* [x] Added/updated unit tests (BUnit)
* [ ] Added Playwright automation
* [ ] Other (specify):
* [ ] NA

> `Bunit/CR Issues/BLAZ-73262.razor` added with 3 fixtures as described above.

---

### Cross-Platform Verification

* [x] Blazor Server
* [x] Blazor WebAssembly
* [ ] NA

---

### Related Issues

* [ ] Resolved in EJ2 (PR link: ___)
* [ ] Created task for EJ2 (Task link: ___)
* [ ] Needs attention in other components
* [x] NA

---

### API Changes

* [ ] New API added (API Review task link: ___)
* [ ] Existing API renamed/modified (API Review task link: ___)
* [x] No API changes

---

### Performance Verification

* [x] Verified no memory leaks introduced
* [x] Verified no performance degradation
* [ ] Not applicable

> Change is a single boolean condition addition inside a cell iteration loop. No new allocations, no new async paths, no render triggers added.

---

### Files Changed

| File | Type | Description |
|------|------|-------------|
| `Internal/Actions/Selection.cs` | Bug fix | Added `\|\| _cell?.CellType == CellType.CommandColumn` to middle-row guard in `IterateCells` |
| `Bunit/CR Issues/BLAZ-73262.razor` | Test | 3 BUnit fixtures covering core fix, public API path, and non-regression |

---

### Reviewer Checklist

* [ ] Code Studio usage information reviewed
* [ ] Code changes follow component guidelines
* [ ] All provided information reviewed and verified
* [ ] Solution addresses the root cause effectively
