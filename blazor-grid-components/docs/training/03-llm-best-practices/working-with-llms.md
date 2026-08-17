# Working with LLMs — Syncfusion Blazor DataGrid

> **Audience**: Developers working with AI sub-agents on the DataGrid team  
> **Module**: 03 — LLM Best Practices  
> **Time Required**: 60 minutes  
> **Prerequisites**: [`../02-requirements-analysis/understanding-requirements.md`](../02-requirements-analysis/understanding-requirements.md)  
> **Reference**: [`../../ai-agents/usage-guidelines.md`](../../ai-agents/usage-guidelines.md)  
> **Last Updated**: March 12, 2026

---

## Overview

The Syncfusion Blazor DataGrid team uses AI sub-agents (LLMs such as Claude, GPT-4, and Gemini) as collaborative contributors. These agents assist with code generation, bug analysis, documentation, testing, and performance review. However, LLMs are not infallible. Used without discipline, they produce hallucinations, introduce regressions, and violate coding standards.

This module teaches you how to use LLMs effectively and safely in a professional component development context.

---

## Part 1 — The 7 AI Agent Roles

The DataGrid team has defined 7 specialized AI agent roles. Each has a specific responsibility boundary.

| Agent Role | Responsibilities | When to Invoke |
|-----------|-----------------|----------------|
| **Scrum Master AI** | Validates requirements, approves fix approaches, gates PRs | Before starting work, before PR submission |
| **Code Review AI** | Reviews code against standards, identifies regressions, checks XML comments | After implementation, before PR |
| **Bug Fix AI** | Analyzes root cause, proposes fix, identifies affected modules | When assigned a bug |
| **Documentation AI** | Generates and updates markdown docs, XML comments | When adding new API or features |
| **Test AI** | Writes BUnit and Playwright test cases | After implementation |
| **Performance AI** | Identifies memory leaks, render count issues, allocation hot spots | For performance-sensitive changes |
| **Accessibility AI** | Validates ARIA attributes, keyboard navigation, WCAG compliance | For UI-visible changes |

> **Rule**: Never ask one agent to perform another agent's responsibilities. A Bug Fix AI should not review code. A Code Review AI should not write tests.

---

## Part 2 — Writing Effective Prompts

### The 6 Elements of a Good Sub-Agent Prompt

Every prompt to a DataGrid sub-agent must include all 6 elements:

```
1. ROLE        — Which agent role you are invoking
2. CONTEXT     — What the component is and what files are involved
3. SCOPE       — Exactly what the agent must do (and NOT do)
4. CONSTRAINTS — Rules the agent must follow
5. INPUT       — The source code excerpt or requirement to process
6. OUTPUT      — Exactly what format the result should be in
```

### ❌ WRONG — Vague Prompt

```
Fix the bug where Tab causes an error after grouping.
```

**Why it fails**: No context, no file reference, no constraint, no output format. The LLM will hallucinate a solution based on guessed code.

### ✅ CORRECT — Scoped Prompt

```
ROLE: Bug Fix AI for Syncfusion Blazor DataGrid

CONTEXT:
- Component: SfGrid<TValue>, namespace Syncfusion.Blazor.Grids
- Bug: Script error when pressing Tab after grouping (Work Item #1015142)
- Affected module: Edit<T> in Internal/Actions/Edit.cs
- Affected feature: Normal edit mode, Tab key navigation, with AllowGrouping = true

SCOPE:
Analyze the Tab key handler in Edit<T>. Identify why the focus calculation
fails when group rows are present in the rendered row set. Propose a fix
that handles the group row type check before attempting focus navigation.
Do NOT modify any other module. Do NOT change public API.

CONSTRAINTS:
- No API breaking changes
- No behavior changes to non-grouped editing flows
- Follow naming-conventions.md (camelCase locals, PascalCase methods)
- Add XML documentation to any new or modified public members
- Zero analyzer warnings in the output

INPUT:
[paste the relevant Tab handler code excerpt from Edit.cs here]

OUTPUT:
1. Root cause explanation (3–5 sentences)
2. Modified code for the affected method only
3. List of regression risks
4. Required test case descriptions (Given-When-Then format)
```

---

## Part 3 — Validating LLM Output

Never accept LLM-generated code without validation. Apply this checklist to every response:

### Code Validation Checklist

- [ ] **Compiles without errors**: Paste the code into the IDE and build
- [ ] **Zero analyzer warnings**: Verify in the IDE Errors panel
- [ ] **XML comments present**: All `public` members have `/// <summary>`
- [ ] **No hardcoded strings**: String literals that are user-visible belong in localization
- [ ] **Correct null handling**: Nullable reference type annotations match the pattern
- [ ] **No direct `StateHasChanged()` calls**: The grid uses internal scheduling
- [ ] **No direct `JSRuntime.InvokeAsync` calls**: All JS goes through `GridJSInteropAdaptor<T>`
- [ ] **No new direct module-to-module dependencies**: Cross-module calls must use `EventAggregator`
- [ ] **No new public API added without review**: Check against `SfGrid.Properties.cs` and `SfGrid.Methods.cs`
- [ ] **Existing tests still pass**: Run `dotnet test` after applying the change

### Hallucination Red Flags

Watch for these patterns that indicate the LLM invented something:

| Red Flag | What to Check |
|----------|-------------|
| References a method that doesn't exist | Search codebase with grep for the method name |
| References a class with slightly wrong name | Search codebase — the LLM may have used a similar class name |
| Adds a `[Parameter]` property that doesn't exist | Check `SfGrid.Properties.cs` |
| Uses an enum value that doesn't exist | Check `Enumeration/GridsEnumerations.cs` |
| Suggests a pattern inconsistent with existing code | Compare with 3 similar existing patterns in the same file |
| Output is longer than the scope specified | The agent over-generated; trim to scope only |

---

## Part 4 — Scoping Sub-Agent Work

### The Golden Rule of Sub-Agent Scoping

> **Provide only the code excerpt the agent needs. Never provide the entire file.**

A large source file (500+ lines) consumes most of the LLM's context window, leaving little room for reasoning. Instead:

1. Identify the specific method or class the agent must work on
2. Extract that excerpt (50–150 lines maximum per task)
3. Include the class signature and relevant field declarations as context
4. Include only the methods the agent needs to read and the method it must modify

### Excerpt Template

```csharp
// FILE: Internal/Actions/Edit.cs
// CLASS: Edit<TValue>
// MODULE: Edit Action Module
// SCOPE: Tab key navigation handler only

// --- Relevant field declarations ---
private GridColumn[] _editableColumns;
private int _currentEditRowIndex;

// --- Method to READ (context) ---
private GridColumn GetNextEditableColumn(int currentIndex) { ... }

// --- Method to MODIFY ---
private async Task HandleTabKeyAsync(KeyboardEventArgs args)
{
    // [paste current implementation here]
}
```

This approach gives the agent exactly what it needs and prevents it from accidentally modifying unrelated code.

---

## Part 5 — The Approval Workflow

```
Developer identifies task
    ↓
Architect AI: scope the task, identify affected files
    ↓
Developer creates requirements/bugs/<id>/ or requirements/features/<name>/
    ↓
Scrum Master AI: approves fix-approach.md or feature-requirement.md
    ↓
Developer extracts code excerpt
    ↓
Bug Fix AI / Code AI: generates implementation
    ↓
Developer validates output (compile + test + checklist)
    ↓
Test AI: generates regression test cases
    ↓
Documentation AI: updates XML comments and markdown
    ↓
Code Review AI: final review
    ↓
Developer submits PR
    ↓
Scrum Master AI: PR approval gate check
```

Never skip a step in this workflow. Skipping the Scrum Master approval before implementation is the most common cause of PR rejection.

---

## Part 6 — Common Mistakes and How to Avoid Them

### Mistake 1 — Asking for a Complete Feature in One Prompt

❌ "Implement the Auto Cell Spanning feature for the DataGrid"

This generates an untestable, un-reviewable blob of code that touches 10+ files.

✅ Break it into scoped tasks:
- Task 1: Add `AutoSpan` parameter to `SfGrid.Properties.cs`
- Task 2: Add `AutoSpanMode` enum to `GridsEnumerations.cs`
- Task 3: Add span calculation logic to `MergeHandler.cs`
- Task 4: Update cell renderer to apply `colspan`/`rowspan`
- Each task is a separate sub-agent prompt with its own excerpt

### Mistake 2 — Not Specifying Constraints

Without explicit constraints, LLMs default to patterns from their training data, not Syncfusion DataGrid patterns. Always include:
- "Follow `naming-conventions.md`"
- "No API breaking changes"
- "Zero analyzer warnings"
- "Do not add any new public members without explicit instruction"

### Mistake 3 — Accepting Output Without Testing

An LLM response that looks correct may still fail at runtime due to:
- Race conditions in async flows
- Null reference exceptions in edge cases
- Memory leaks from event handler subscriptions not unsubscribed on dispose

Always run the sample application and exercise the affected scenario manually before submitting a PR.

### Mistake 4 — Using the LLM as a Search Engine

Do not ask:
"What does `DataGenerator<T>` do?"

Instead, read the source file directly. LLMs can hallucinate implementation details for internal Syncfusion APIs. Trust the source code, not the LLM's description of it.

---

## Part 7 — Request Templates

Use these templates when invoking sub-agents:

### Feature Implementation Request

```
ROLE: Code AI — Feature Implementation
COMPONENT: SfGrid<TValue> — Syncfusion.Blazor.Grids
FEATURE: [feature name]
WORK ITEM: [Azure DevOps URL]
MODULE: [action module name and file]
SCOPE: [exactly what to implement — one method or one class section]
CONSTRAINTS: No API changes, zero warnings, XML comments required
INPUT: [code excerpt]
OUTPUT: Modified method implementation only, with explanation
```

### Bug Fix Request

```
ROLE: Bug Fix AI
COMPONENT: SfGrid<TValue> — Syncfusion.Blazor.Grids
BUG: [description]
WORK ITEM: [Azure DevOps URL]
ROOT CAUSE FILE: docs/requirements/bugs/<id>/root-cause.md
MODULE: [module name and file]
SCOPE: [exact method to fix]
CONSTRAINTS: No behavior change outside the bug scenario, no API changes
INPUT: [code excerpt]
OUTPUT: Root cause (3–5 sentences) + fixed implementation + regression risk list
```

---

## Navigation

**Previous**: [`../02-requirements-analysis/understanding-requirements.md`](../02-requirements-analysis/understanding-requirements.md)  
**Next**: [`../04-code-processing/optimal-chunking-strategies.md`](../04-code-processing/optimal-chunking-strategies.md)  
**Reference**: [`../../ai-agents/usage-guidelines.md`](../../ai-agents/usage-guidelines.md)
