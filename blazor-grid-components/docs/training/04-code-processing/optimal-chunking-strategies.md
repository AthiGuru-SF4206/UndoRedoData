# Optimal Chunking Strategies — Syncfusion Blazor DataGrid

> **Audience**: Developers preparing source excerpts for LLM sub-agents  
> **Module**: 04 — Code Processing  
> **Time Required**: 45 minutes  
> **Prerequisites**: [`../03-llm-best-practices/working-with-llms.md`](../03-llm-best-practices/working-with-llms.md)  
> **Last Updated**: March 12, 2026

---

## Overview

The Syncfusion Blazor DataGrid source files are large. Several action modules exceed 1,000 lines of C# code. LLMs have a limited context window (typically 100K–200K tokens), and filling that window with irrelevant code degrades output quality significantly.

**Optimal chunking** is the discipline of extracting exactly the right code context for a given task — no more, no less.

---

## Part 1 — Why Chunking Matters

### The Context Window Problem

| Source File | Approximate Size | Tokens (estimated) |
|-------------|----------------|-------------------|
| `Internal/Actions/Edit.cs` | ~1,800 lines | ~25,000 tokens |
| `Internal/Actions/Sort.cs` | ~800 lines | ~11,000 tokens |
| `Internal/Actions/VirtualScroll.cs` | ~1,200 lines | ~17,000 tokens |
| `Internal/Renderer/GridRowRenderer.razor` | ~500 lines | ~7,000 tokens |
| `sf-grid.js` | ~2,500 lines | ~35,000 tokens |

If you paste the entire `Edit.cs` file (25,000 tokens) plus context for a task that only touches one 50-line method, you have wasted 99% of the context window. The LLM's attention is diluted across irrelevant code. Output quality drops.

**Target budget per sub-agent prompt**: 3,000–8,000 tokens of source code input.

---

## Part 2 — Identifying Semantic Chunk Boundaries

A **semantic chunk** is a coherent unit of code with a clear single responsibility. In the DataGrid codebase, chunk boundaries align with:

### 1. Method Boundaries

The safest and most common chunking unit. Each public or private method is a self-contained chunk.

```csharp
// CHUNK START: Tab key handler
private async Task HandleTabKeyAsync(KeyboardEventArgs args)
{
    // ... method body ...
}
// CHUNK END
```

### 2. Feature Responsibility Boundaries within a Class

Large action modules like `Edit<T>` contain sub-responsibilities. Identify them by comment regions or logical groupings:

```csharp
// --- REGION: Normal Edit Mode ---
// Methods: BeginEdit, SaveEdit, CancelEdit, ValidateForm
// Chunk size: ~150 lines

// --- REGION: Batch Edit Mode ---
// Methods: BeginBatchEdit, SaveBatchCell, CancelBatchEdit
// Chunk size: ~200 lines

// --- REGION: Keyboard Navigation in Edit ---
// Methods: HandleTabKey, HandleEnterKey, HandleEscapeKey
// Chunk size: ~100 lines
```

When you need to fix a Tab key bug, you only need the third chunk — not the entire file.

### 3. Lifecycle Phase Boundaries

Component lifecycle methods form natural boundaries:

```
Initialization chunk:    OnInitializedAsync + initialization helpers
Data fetch chunk:        GenerateQuery + ExecuteQuery + DataBound handlers
Render scheduling chunk: OnParametersSetAsync + PropertyChanges processing
Dispose chunk:           IDisposable.Dispose + cleanup helpers
```

### 4. JS-Interop Boundary

Any code that crosses the JS-interop boundary is a separate chunk from the C# business logic:

```
C# side chunk:  GridJSInteropAdaptor.InitializeAsync + SendScrollUpdateAsync
JS side chunk:  sfBlazor.Grid.initialize + handleScroll
```

Never include both C# and JS in the same agent prompt unless the task explicitly spans both sides.

---

## Part 3 — The Chunk Extraction Template

When extracting a chunk for a sub-agent, always include:

### 1. File Header Comment

```csharp
// FILE: Internal/Actions/Edit.cs
// CLASS: Edit<TValue> : IActionModule
// TOTAL FILE SIZE: ~1,800 lines (only excerpt below provided)
// CHUNK: Tab Key Navigation (lines 742–815)
// DEPENDS ON: GridColumn, FocusHandler<TValue>
```

### 2. Relevant Field Declarations

Include only the fields used by the methods in the chunk:

```csharp
// --- Relevant field declarations ---
private readonly SfGrid<TValue> _parent;
private readonly FocusHandler<TValue> _focusHandler;
private GridColumn[] _editableColumns;
private int _currentEditRowIndex = -1;
private bool _isEditing;
```

### 3. Signature of Methods the Agent Must READ

```csharp
// --- Context methods (signatures only — do not modify) ---
private GridColumn GetNextEditableColumn(int currentIndex) { ... }
private async Task FocusCellAsync(int rowIndex, int colIndex) { ... }
private bool IsGroupRow(int rowIndex) { ... }
```

### 4. Full Body of the Method the Agent Must MODIFY

```csharp
// --- Target method (MODIFY THIS) ---
private async Task HandleTabKeyAsync(KeyboardEventArgs args)
{
    // full current implementation
}
```

### 5. Expected Interface Contract

```csharp
// --- Expected behavior after fix ---
// Given: EditMode = Normal, AllowGrouping = true, user is on last editable cell
// When: Tab key is pressed
// Then: Focus moves to first cell of next non-group row without script error
```

---

## Part 4 — Token Budget Management

### Rough Token Estimates

| Code Element | Approximate Tokens |
|-------------|-------------------|
| One C# method (50 lines) | ~700 tokens |
| One C# class section (100 lines) | ~1,400 tokens |
| Method signatures block (10 signatures) | ~300 tokens |
| Field declarations block (10 fields) | ~200 tokens |
| One Razor component (200 lines) | ~2,800 tokens |
| One JS function (50 lines) | ~600 tokens |
| System prompt + instructions | ~500–1,000 tokens |

### Budget Allocation (8,000 token target)

| Section | Token Budget |
|---------|------------|
| System prompt + instructions | 800 |
| File header comment + field declarations | 400 |
| Context method signatures | 500 |
| Target method (full body) | 2,000 |
| Related excerpt (e.g., callers) | 1,500 |
| Expected behavior contract | 300 |
| **Total input** | **~5,500** |
| **LLM output budget** | **~2,500** |

Staying within this budget ensures the LLM has enough reasoning space for high-quality output.

---

## Part 5 — Chunking the Key DataGrid Source Files

### Chunking `Internal/Actions/Edit.cs`

| Chunk Name | Content | When to Use |
|-----------|---------|------------|
| `Edit-Init` | Constructor + module registration + lifecycle init | When changing edit mode initialization |
| `Edit-Normal` | BeginEdit, SaveEdit, CancelEdit for Normal mode | Normal edit mode bugs |
| `Edit-Dialog` | OpenDialog, CloseDialog, SaveDialog | Dialog edit mode bugs |
| `Edit-Batch` | BeginBatchEdit, SaveBatchCell, CancelBatch | Batch edit mode bugs |
| `Edit-Keyboard` | HandleTabKey, HandleEnterKey, HandleEscapeKey | Keyboard navigation bugs in edit |
| `Edit-Validation` | ValidateFormAsync, GetValidationErrors | Validation bugs |
| `Edit-AddNewRow` | ShowAddNewRow, HideAddNewRow, AddNewRowPosition | Add-new row bugs |
| `Edit-Dispose` | Dispose, UnsubscribeEvents | Memory leak investigation |

### Chunking `Internal/Actions/VirtualScroll.cs`

| Chunk Name | Content | When to Use |
|-----------|---------|------------|
| `Virtual-Init` | Initialization, row height detection | Initialization bugs |
| `Virtual-Scroll` | Scroll event handler, row range calculation | Scroll performance bugs |
| `Virtual-Render` | Row render range, overscan, placeholder rows | Wrong rows visible bugs |
| `Virtual-Edit` | Edit interaction with virtual DOM | Virtualization + edit bugs |
| `Virtual-Group` | Grouped virtual rendering | Virtualization + grouping bugs |
| `Virtual-Dispose` | Cleanup, observer disconnect | Memory leak investigation |

### Chunking `sf-grid.js`

| Chunk Name | Content | When to Use |
|-----------|---------|------------|
| `JS-Init` | `initialize`, event listener setup | JS initialization bugs |
| `JS-Scroll` | `handleScroll`, `sendScrollUpdate` | Scroll sync bugs |
| `JS-Focus` | `focusCell`, `focusElement`, `handleFocusLoss` | Focus/keyboard bugs |
| `JS-Resize` | `handleColumnResize`, `sendResizeDelta` | Column resize bugs |
| `JS-Drag` | `handleDragStart`, `handleDragMove` | Drag-and-drop bugs |
| `JS-Measure` | `measureColumnWidths`, `getViewportSize` | Layout measurement bugs |
| `JS-Dispose` | `destroy`, observer and listener cleanup | Memory leak bugs |

---

## Part 6 — Anti-Patterns to Avoid

### ❌ Providing the Entire File

```
// BAD: Pasting all 1,800 lines of Edit.cs
```

The LLM will produce a low-quality fix because its attention is diluted.

### ❌ Missing Field Declarations

```
// BAD: Providing only the method body without the fields it uses
private async Task HandleTabKeyAsync(KeyboardEventArgs args)
{
    int nextIndex = GetNextEditIndex(_currentEditRowIndex); // LLM doesn't know what _currentEditRowIndex is
}
```

### ❌ Omitting the Context Method Signatures

The LLM must know the signatures of methods called by the target method. Without them, it will hallucinate method signatures.

### ❌ Exceeding the Token Budget

If your chunk exceeds 8,000 tokens of input, split it into two separate tasks. Never ask the LLM to process more than it can reason about effectively.

### ❌ Mixing C# and JS in One Chunk

C# logic and JS DOM operations are fundamentally different contexts. Process them in separate prompts with a shared interface contract between them.

---

## Part 7 — Practice Exercise

**Task**: Extract the correct chunk from `Edit.cs` for fixing the Tab key bug (Work Item #1015142).

1. Open `Internal/Actions/Edit.cs`
2. Locate the Tab key handling code
3. Extract the chunk using the template from Part 3
4. Verify your chunk is within the 8,000 token budget (estimate using the table in Part 4)
5. Submit the chunk to the Bug Fix AI using the request template from Module 03

---

## Navigation

**Previous**: [`../03-llm-best-practices/working-with-llms.md`](../03-llm-best-practices/working-with-llms.md)  
**Next**: [`../05-practical-examples/feature-implementation-walkthrough.md`](../05-practical-examples/feature-implementation-walkthrough.md)
