# Undo & Redo Support in Blazor DataGrid Batch Editing

## Introduction

Undo and Redo support enables users to safely modify data in Batch Edit mode and recover from mistakes without losing pending changes.

The feature maintains an edit-history stack during the batch editing session and allows users to move backward and forward through recorded actions.

## Goals

- Provide spreadsheet-like editing experience.
- Reduce accidental data-entry mistakes.
- Allow recovery of edits before batch save.
- Support keyboard shortcuts.
- Support toolbar actions.
- Expose public APIs.
- Maintain action history during batch editing.

## Supported Edit Operations

### Cell Edit
Track modifications made to individual cells.

### Row Add
Track newly added rows and allow reversal.

### Row Delete
Track deleted rows and restore them through Undo.

### Paste
Track multi-cell paste operations as a single action.

### AutoFill
Track AutoFill operations and allow restoration.

## Scope

Supported: EditMode.Batch

Not Supported:
- EditMode.Normal
- EditMode.Dialog
- EditMode.Cell

## Keyboard Shortcuts

- Ctrl+Z: Undo
- Ctrl+Y: Redo
- Ctrl+Shift+Z: Redo

## Behavioral Rules

### Undo
1. Remove latest action from Undo stack.
2. Reverse action.
3. Move action to Redo stack.

### Redo
1. Remove latest action from Redo stack.
2. Reapply action.
3. Move action to Undo stack.

### New Edit After Undo
Performing a new edit after Undo clears the Redo stack.

## History Cleanup

History is automatically cleared on:
- Batch Save
- Batch Cancel
- Grid Refresh
- Data Source Reload

## Supported Features Integration

- Sorting
- Filtering
- Grouping
- Paging
- Virtual Scrolling
- Frozen Columns
- Selection
