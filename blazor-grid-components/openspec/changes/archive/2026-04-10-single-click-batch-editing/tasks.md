# Single-Click Batch Editing - Implementation Tasks

## Overview
Implement the "Single Click to Edit Cell" feature in Batch editing mode. Feature is opt-in via `AllowEditOnSingleClick` property on `GridEditSettings`.

## Tasks

### Core API & Property Setup
- [x] Add `AllowEditOnSingleClick` property to `GridEditSettings` class
- [x] Add change detection for `AllowEditOnSingleClick` in `GridEditSettings.OnParametersSetAsync`
- [x] Serialize `AllowEditOnSingleClick` to JS options in `GetClientOption()` method

### .NET Interop Layer
- [x] Add `SingleClickEditCell` JSInvokable method to `GridJSInteropAdaptor`
- [x] Implement `SingleClickHandler` method in `Edit<T>` class
- [x] Add guards for primary key columns, non-editable cells, and edit mode validation

### JavaScript Integration
- [x] Register click listener in `sf-grid.ts` when `AllowEditOnSingleClick = true`
- [x] Implement cell click handler to resolve row/cell UIDs and invoke .NET
- [x] Ensure click listener doesn't trigger on non-data cells (headers, filter bar, group captions)
- [x] Handle dynamic option updates for listener attachment/detachment

### Cross-Feature Integration
- [x] Verify selection integration with `EditCell()` method (existing logic)
- [x] Verify keyboard navigation compatibility (Tab, Shift+Tab, F2, Escape, Arrow keys)
- [x] Verify validation flow for click-away cell save with `SaveCell()` and `ValidateNextCell()`
- [x] Test with grouping feature (caption rows, grouped columns, add-row positioning)

### Event & Behavior Verification
- [x] Verify `OnCellEdit` event fires with correct args for single-click activation
- [x] Verify `OnCellSave` and `CellSaved` events fire on click-away with proper args
- [x] Verify existing `OnBatchSave`, `OnBatchDelete`, `OnBatchAdd` events remain intact
- [x] Verify `OnActionBegin` and `OnActionComplete` fire around batch operations

### Data Operation Safety
- [ ] Test sorting applied after batch save doesn't break render state
- [ ] Test filtering applied after batch save auto-saves or discards dirty rows
- [ ] Test page change auto-saves open cell (or blocks on validation error)
- [ ] Test virtual scroll editing outside rendered window doesn't crash
- [ ] Test infinite scroll editing on page > 1 works correctly
- [ ] Test frozen columns: single-click edit opens in frozen and non-frozen panes
- [ ] Test row drag-drop handle doesn't trigger cell edit
- [ ] Test aggregate footer refreshes after cell save

### Regression Test Coverage
- [ ] Double-click still opens cell when `AllowEditOnSingleClick = false` (default)
- [ ] `AllowEditOnDblClick = false` disables double-click regardless of `AllowEditOnSingleClick`
- [ ] F2 key still opens cell in batch edit mode
- [ ] Required-field validation blocks click-away from invalid cell
- [ ] Custom `Validator` template works with single-click activation
- [ ] Row/Cell selection modes work correctly with single-click edit
- [ ] PersistSelection works with single-click edit
- [ ] Checkbox column not accidentally triggered
- [ ] Export reads committed data (dirty rows not exported until BatchSave)

### Documentation & Demo
- [x] Create or update demo component showing feature usage
- [x] Ensure demo covers validation, events, and selection scenarios
- [x] Add code comments to new methods for maintainability (if required)

## Schema: spec-driven
This change uses the spec-driven workflow which includes: proposal, specs, design, and tasks artifacts.
