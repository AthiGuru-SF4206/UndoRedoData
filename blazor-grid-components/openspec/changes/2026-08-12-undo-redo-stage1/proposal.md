# Undo/Redo Feature - Stage 1: Keyboard Infrastructure - Proposal

## Problem Statement

Users performing batch editing operations in Syncfusion Blazor DataGrid often make mistakes while editing multiple cells. Currently, there is **no way to recover** from accidental changes without manually re-entering data or reloading the page and losing all batch changes.

**Current Friction**:
- User edits 5 cells, realizes one was wrong → must manually re-enter that cell
- User accidentally deletes a row → cannot recover without canceling entire batch
- No keyboard shortcuts (Ctrl+Z, Ctrl+Y) to reverse edits
- Users lose all work if they navigate away accidentally

## Solution Overview

Implement **Stage 1 of the Undo/Redo feature** with foundational infrastructure for **keyboard-driven undo/redo** in Batch Edit mode.

**Stage 1 Scope**: 
- ✅ Keyboard shortcut support (Ctrl+Z, Ctrl+Y, Ctrl+Shift+Z)
- ✅ Undo/Redo history stacks with configurable limits
- ✅ Public async API methods (`UndoAsync()`, `RedoAsync()`, `ClearUndoRedoAsync()`)
- ✅ Stack tracking properties (`UndoCount`, `RedoCount`, `IsUndoAvailable`, `IsRedoAvailable`)
- ✅ Configuration via `GridEditSettings.EnableUndoRedo` and `GridEditSettings.UndoRedoLimit`

**Stage 1 Does NOT include** (deferred to future stages):
- Toolbar buttons / UI controls
- Events (ActionUndoing, ActionUndone, etc.) — only internal logging
- Persistence across page reloads
- Redo-stack clearing after new action (handled but not exposed)

## Acceptance Criteria

### Functional Requirements

1. **Keyboard Shortcuts**
   - [ ] Ctrl+Z triggers undo of most recent action
   - [ ] Ctrl+Y triggers redo of most recent action
   - [ ] Ctrl+Shift+Z (alternative) triggers redo
   - [ ] Shortcuts work only when grid has focus
   - [ ] Shortcuts only work in Batch edit mode

2. **History Stack Management**
   - [ ] Undo stack records actions with proper state (old/new values)
   - [ ] Redo stack maintains actions cleared after new action is performed
   - [ ] Stack limit enforced (oldest actions removed when limit exceeded)
   - [ ] Default stack limit is 20 actions (configurable via UndoRedoLimit)
   - [ ] Memory-efficient stack using O(1) push/pop operations

3. **Action Recording**
   - [ ] Cell edits recorded with row index, column field, old value, new value
   - [ ] Row additions recorded with row data and position (Top/Bottom)
   - [ ] Row deletions recorded with row data and original index
   - [ ] Multiple actions properly sequenced in stack

4. **State Restoration (Undo)**
   - [ ] Undo cell edit restores original value and marks cell clean
   - [ ] Undo row addition removes row from grid
   - [ ] Undo row deletion restores row at original position
   - [ ] Multiple undo operations work sequentially
   - [ ] Undo when stack empty does nothing (no error)

5. **State Reapplication (Redo)**
   - [ ] Redo cell edit re-applies new value
   - [ ] Redo row addition restores row
   - [ ] Redo row deletion removes row again
   - [ ] Multiple redo operations work sequentially
   - [ ] Redo when stack empty does nothing (no error)

6. **Configuration & Lifecycle**
   - [ ] EnableUndoRedo defaults to false (opt-in)
   - [ ] UndoRedoLimit defaults to 20
   - [ ] EnableUndoRedo only works in `EditMode.Batch`
   - [ ] Silently disables if Mode is Normal or Dialog
   - [ ] Dynamic toggle of EnableUndoRedo clears stacks
   - [ ] Stack properties update correctly after each action

7. **API Methods**
   - [ ] `UndoAsync()` - undo most recent action
   - [ ] `RedoAsync()` - redo most recent undo
   - [ ] `UndoAllAsync()` - undo all actions to clean state
   - [ ] `RedoAllAsync()` - redo all undone actions
   - [ ] `ClearUndoRedoAsync()` - clear both stacks

8. **Property Accessors**
   - [ ] `UndoCount` - returns size of undo stack (read-only)
   - [ ] `RedoCount` - returns size of redo stack (read-only)
   - [ ] `IsUndoAvailable` - returns true if undo stack not empty
   - [ ] `IsRedoAvailable` - returns true if redo stack not empty

### Cross-Feature Interaction Guarantees

- [ ] Works with **Grouping** (grouped rows undo/redo correctly)
- [ ] Works with **Virtualization** (virtual scroll index maintained)
- [ ] Works with **Frozen Columns** (edits in frozen zones work)
- [ ] Works with **Paging** (page-level edits preserved)
- [ ] Works with **Filtering** (filtered rows undo/redo correctly)
- [ ] Works with **Infinite Scroll** (multi-page edits preserved)
- [ ] Works with **Aggregates** (footer aggregates refresh after undo/redo)
- [ ] Works with **Selection** (selection state preserved after undo/redo)
- [ ] Works with **Sorting** (sort order preserved after undo/redo)

### Non-Functional Requirements

- [ ] No breaking changes to existing APIs
- [ ] Zero performance impact when EnableUndoRedo=false (default)
- [ ] Stack operations complete in <5ms per action
- [ ] Memory bounded by UndoRedoLimit configuration
- [ ] Clean separation of concerns (UndoRedoManager<T> isolated)
- [ ] Code follows Syncfusion C# standards

## Success Metrics

1. **Functionality**: All keyboard shortcuts work as specified
2. **Reliability**: No regressions in batch editing or other features
3. **Performance**: Stack operations sub-millisecond with realistic limits (≤50 actions)
4. **UX**: Users can recover from mistakes easily via Ctrl+Z

## Stage Roadmap

| Stage | Scope | Timeline |
|-------|-------|----------|
| **1 (Current)** | Keyboard infrastructure, API methods, stack management | Done |
| **2** | Settings persistence (EnableUndoRedo property binding) | Next |
| **3** | Toolbar integration (Undo/Redo buttons) | Q3 2026 |
| **4** | Public events and end-to-end integration | Q4 2026 |

---

**Change Name**: `2026-08-12-undo-redo-stage1`  
**Schema**: spec-driven  
**Status**: Ready for design and implementation
