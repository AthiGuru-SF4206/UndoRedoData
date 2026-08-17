# Single-Click Batch Editing - Proposal

## Problem Statement

Currently, in Syncfusion Blazor DataGrid's Batch editing mode, users must **double-click** a cell to enter edit state. This is inconsistent with many modern data grid UX patterns where single-click enters edit mode, reducing interaction overhead and improving user experience.

**Current Friction**: Users expect single-click to edit, leading to user confusion and extra clicks.

## Solution Overview

Introduce an opt-in API property `AllowEditOnSingleClick` on `GridEditSettings` that, when enabled in Batch mode, allows a cell to enter edit state on a **single mouse click** rather than requiring a double-click.

The feature:
- Is **opt-in** (default: false) → zero impact on existing grids
- Works **only in Batch edit mode** → other modes unaffected
- Maintains **full backward compatibility** with double-click and keyboard activation (F2, Tab)
- Reuses existing save/validation/event infrastructure → low risk, high reliability
- Integrates safely with all grid features (grouping, virtualization, frozen columns, etc.)

## Acceptance Criteria

### Functional Requirements

1. **Property Definition**
   - [ ] `GridEditSettings.AllowEditOnSingleClick` bool parameter added (default: false)
   - [ ] Property serialized to JavaScript options
   - [ ] Property change detection updates JS listener attachment/detachment

2. **Edit Activation**
   - [ ] Single click on data cell opens cell in edit mode (if `AllowEditOnSingleClick=true` and `Mode=Batch`)
   - [ ] Non-data cells (headers, filter bar, group captions) do not trigger edit
   - [ ] Primary key cells on existing rows remain non-editable

3. **Cell-to-Cell Navigation**
   - [ ] Clicking a different cell auto-saves current cell and opens new cell
   - [ ] Validation errors block click-away; current cell remains open
   - [ ] Previous cell transitions to dirty state if changed

4. **Event Behavior**
   - [ ] `OnCellEdit` fires with correct args when single-click activates
   - [ ] `OnCellSave` / `CellSaved` fire on click-away save
   - [ ] `OnBatchSave` / `OnBatchDelete` / `OnBatchAdd` all work unchanged
   - [ ] No new events introduced; all existing events fire identically

5. **Keyboard & Selection**
   - [ ] Tab/Shift+Tab navigation unchanged; auto-save still works
   - [ ] F2 key still opens cell
   - [ ] Escape key cancels edit
   - [ ] Row selected on single-click (unless `PersistSelection=true`)
   - [ ] Selection modes (Row/Cell/Both) all work correctly

6. **Validation & Error Handling**
   - [ ] Required-field validation blocks click-away
   - [ ] Custom validators respected
   - [ ] Validation tooltips shown
   - [ ] `OnCellEdit` cancellation prevents edit activation

### Cross-Feature Integration

- [ ] Works with **Grouping** (grouped data rows editable; captions not)
- [ ] Works with **Virtualization** (virtual scroll to edited row)
- [ ] Works with **Frozen Columns** (edit in frozen and movable panes)
- [ ] Works with **Paging** (page change auto-saves open cell)
- [ ] Works with **Filtering** (filter changes re-render correctly)
- [ ] Works with **Sorting** (sort after save re-renders correctly)
- [ ] Works with **Infinite Scroll** (editing on any page works)
- [ ] Works with **Aggregates** (footer aggregates refresh after save)
- [ ] Works with **Row Drag-Drop** (reorder handles not affected)
- [ ] Works with **Selection** (row/cell selection modes compatible)

### Non-Functional Requirements

- [ ] No breaking changes to existing APIs
- [ ] Double-click still works when `AllowEditOnSingleClick=false` (default)
- [ ] Performance: listener only attached when feature enabled
- [ ] Code quality: uses existing Edit<T> and SaveCell() methods (no code duplication)
- [ ] Browser compatibility: works on all supported browsers

## Success Metrics

1. **Feature Completeness**: All acceptance criteria met without regressions
2. **User Experience**: Users can edit cells with single click in Batch mode
3. **Reliability**: All existing features co-exist safely
4. **Documentation**: Demo component and code comments provided

## Out of Scope

- Triple-click behavior
- Right-click context menu edit activation
- Touch/mobile single-tap edit (may be future enhancement)
- Programmatic single-click trigger (API method)
- Feature for Normal or Dialog edit modes (Batch only)

---

**Status**: PROPOSAL COMPLETE  
**Owner**: DataGrid Team  
**Date**: 2026-04-16
