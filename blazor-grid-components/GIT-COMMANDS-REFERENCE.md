# Git Commands Reference - Undo/Redo Change Analysis

This document provides the exact Git commands used to analyze the Undo/Redo feature implementation.

---

## Overview of Changes Between Branches

### Get Summary of All Changes
```bash
git diff --stat development...HEAD
```

**Output**: Shows files modified with insertion/deletion counts

```
 openspec/changes/2026-08-12-undo-redo-stage1/.openspec.yaml | 12 +
 openspec/changes/2026-08-12-undo-redo-stage1/Exploration.md | 123 +
 openspec/changes/2026-08-12-undo-redo-stage1/design.md | 45 +
 ...
 src/Internal/Actions/Edit.cs | 450 ++++++++++++++++++++-
 src/Internal/Actions/UndoRedoManager.cs | 300 +++++++++++++++
 src/Models/UndoRedoAction.cs | 123 ++++++
 ...
```

---

## File-by-File Analysis Commands

### List All Modified Files
```bash
git diff --name-status development...HEAD
```

**Output Format**: `[A/M] filename`
- `A` = Added (new file)
- `M` = Modified (existing file)

---

## Detailed Diff Commands

### View All Changes for a Single File
```bash
git diff development...HEAD -- src/Internal/Actions/Edit.cs
```

**Usage**: Shows all lines added/removed with context

### View Changes in Latest Commit Only
```bash
git diff HEAD~1 HEAD -- src/Internal/Actions/Edit.cs
```

**Usage**: Useful for seeing just the most recent modifications

### View Changes in Specific Commit
```bash
git show 7178e12:src/Internal/Actions/Edit.cs
git show development:src/Internal/Actions/Edit.cs
```

**Usage**: Shows file content at specific commit without the diff markers

---

## Commit History Analysis

### View Commit Messages for Feature
```bash
git log --oneline -20 development...HEAD
```

**Output**:
```
7178e12 1045786: commit the chnages related to the Delete and Add undo and redo action
173c3ca 1045786: Need to implemented Undo and Redo Batch Edit support in Blazor Data Grid
```

### View Full Commit Details
```bash
git log -p --follow -- src/Internal/Actions/Edit.cs
```

**Usage**: Shows full diffs for all commits affecting the file

### View Commit Statistics
```bash
git log --stat development...HEAD
```

**Usage**: Shows which files changed in each commit

---

## Specific File Analysis Commands

### 1. Edit.cs Changes
```bash
# View all changes
git diff development...HEAD -- src/Internal/Actions/Edit.cs

# Show just the stats
git diff --stat development...HEAD -- src/Internal/Actions/Edit.cs

# View in latest commit
git diff HEAD~1 HEAD -- src/Internal/Actions/Edit.cs

# Count lines of change
git diff development...HEAD -- src/Internal/Actions/Edit.cs | wc -l
```

### 2. New UndoRedoManager File
```bash
# View entire new file (it's new, so diff shows whole file)
git diff development...HEAD -- src/Internal/Actions/UndoRedoManager.cs

# View just the stats
git diff --stat development...HEAD -- src/Internal/Actions/UndoRedoManager.cs

# Show the file as it exists now
git show HEAD:src/Internal/Actions/UndoRedoManager.cs
```

### 3. GridEditSettings Changes
```bash
git diff development...HEAD -- src/GridEditSettings.cs
```

### 4. Toolbar Integration
```bash
git diff development...HEAD -- src/Internal/Renderer/GridToolbar.razor
```

### 5. Keyboard Utilities
```bash
git diff development...HEAD -- src/Internal/Base/Utils.cs
```

### 6. JavaScript Interop
```bash
git diff development...HEAD -- scripts/sf-grid-fn.ts
```

---

## Analyzing Specific Modifications

### Find where a method was added
```bash
git diff development...HEAD -- src/Internal/Actions/Edit.cs | grep -A 20 "RecordAction"
```

### Search for a specific change pattern
```bash
git log -S "UpdateLastRowAddAction" --oneline development...HEAD
```

**Usage**: Finds commits that introduced/removed this symbol

### See changes made to a specific method
```bash
git diff development...HEAD -- src/SfGrid.Methods.cs | grep -B 3 -A 10 "UndoAsync"
```

---

## Comparing Versions

### Compare current file with development branch
```bash
git diff development:src/Internal/Actions/Edit.cs HEAD:src/Internal/Actions/Edit.cs
```

### See what changed in a range of lines
```bash
git diff development...HEAD -- src/Internal/Actions/Edit.cs | sed -n '500,600p'
```

---

## Branch Information

### View current branch
```bash
git branch -v
```

### View all branches and their commits
```bash
git log --graph --oneline --all | head -20
```

### Compare two branches
```bash
git log --oneline development..HEAD  # Commits in HEAD but not development
git log --oneline HEAD..development  # Commits in development but not HEAD
```

---

## Tracking Feature Across Commits

### View timeline of changes to Edit.cs
```bash
git log --oneline -- src/Internal/Actions/Edit.cs
```

### See all commits that touched either undo-related files
```bash
git log --oneline development...HEAD -- \
  src/Internal/Actions/Edit.cs \
  src/Internal/Actions/UndoRedoManager.cs \
  src/GridEditSettings.cs
```

### Show detailed changes with author and date
```bash
git log --format="%h %ai %an %s" development...HEAD -- src/Internal/Actions/Edit.cs
```

---

## Analyzing Bug Fixes in the Changes

### Find commits that reference "Fix" or "Bug"
```bash
git log --oneline development...HEAD | grep -i "fix\|bug"
```

### See all debug statements added
```bash
git diff development...HEAD | grep "Debug.WriteLine"
```

### View comments added in changes
```bash
git diff development...HEAD | grep "^+.*//.*CRITICAL\|^+.*//.*FIX"
```

---

## Size and Scope Analysis

### Count total lines added/removed
```bash
git diff --stat development...HEAD | tail -1
```

### See which files had the most changes
```bash
git diff --stat development...HEAD | sort -k3 -rn | head -10
```

### Count changes per file
```bash
git diff development...HEAD --numstat | awk '{sum+=$1+$2} END {print sum}'
```

---

## Extracting Changes to a Patch File

### Create a patch file of all changes
```bash
git diff development...HEAD > undo-redo-feature.patch
```

### Create patches for specific files
```bash
git diff development...HEAD -- src/Internal/Actions/Edit.cs > edit-cs-changes.patch
git diff development...HEAD -- src/Internal/Actions/UndoRedoManager.cs > undoredo-manager-changes.patch
```

### Apply a patch to another branch
```bash
git apply undo-redo-feature.patch
```

---

## Reviewing for Specific Concerns

### Find all new public APIs
```bash
git diff development...HEAD | grep "^+.*public\|^+.*async"
```

### Find all configuration changes
```bash
git diff development...HEAD -- src/GridEditSettings.cs
```

### Find all event triggers
```bash
git diff development...HEAD | grep "EventAggregator.Trigger"
```

### Find all keyboard handling code
```bash
git diff development...HEAD | grep -i "keyboard\|ctrl+\|metakey"
```

### Find all UI integration points
```bash
git diff development...HEAD | grep -i "toolbar\|button\|disable\|enable"
```

---

## Comparing Against Other Features

### See features added in the same commit
```bash
git show --stat 7178e12
```

### Compare this feature branch with another branch
```bash
git diff development..BLAZ-1045786-UndoRedo -- src/
```

---

## Validating the Implementation

### Verify all files compile (pseudo-command for checking syntax)
```bash
git diff development...HEAD -- src/ | grep "using\|namespace\|class\|public"
```

### Find potential issues (empty catch blocks, TODO comments)
```bash
git diff development...HEAD | grep -i "TODO\|FIXME\|HACK\|catch\s*{" -A 2
```

### Verify error handling
```bash
git diff development...HEAD | grep "if.*null\|try\|catch"
```

---

## Quick Reference Summary

| Task | Command |
|------|---------|
| View all changes | `git diff development...HEAD` |
| List files changed | `git diff --name-status development...HEAD` |
| File statistics | `git diff --stat development...HEAD` |
| Single file changes | `git diff development...HEAD -- <file>` |
| Commit history | `git log --oneline development...HEAD` |
| Specific change | `git diff development...HEAD -- <file> \| grep <pattern>` |
| Create patch | `git diff development...HEAD > feature.patch` |
| Search for symbol | `git log -S <symbol> --oneline development...HEAD` |
| View file at commit | `git show <commit>:<file>` |
| Compare commits | `git diff <commit1> <commit2>` |

---

## Useful PowerShell Aliases for Windows

```powershell
# Add to your PowerShell profile
Function git-diff-count { git diff development...HEAD --stat | tail -1 }
Function git-new-files { git diff --name-status development...HEAD | grep "^A" }
Function git-modified-files { git diff --name-status development...HEAD | grep "^M" }
Function git-edit-diff { git diff development...HEAD -- src/Internal/Actions/Edit.cs }
Function git-undo-redo-files { 
    git diff --name-status development...HEAD | grep -E "UndoRedo|Edit\.cs|GridToolbar|FocusHandler"
}
```

---

## Example Usage Flow

### Step 1: Get Overview
```bash
git diff --stat development...HEAD
```

### Step 2: List All Files
```bash
git diff --name-status development...HEAD
```

### Step 3: Review Key Files
```bash
git diff development...HEAD -- src/Internal/Actions/UndoRedoManager.cs
git diff development...HEAD -- src/Internal/Actions/Edit.cs
git diff development...HEAD -- src/GridEditSettings.cs
```

### Step 4: Search for Specific Patterns
```bash
git diff development...HEAD | grep -i "critical\|bug\|fix"
git diff development...HEAD | grep "UndoRedoManager.*RecordAction"
```

### Step 5: Extract to Patch for Review
```bash
git diff development...HEAD > undo-redo-implementation.patch
```

### Step 6: Analyze Commits
```bash
git log --oneline development...HEAD
git show 7178e12  # Review specific commit
```
