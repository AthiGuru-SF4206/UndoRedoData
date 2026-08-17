# FilterBar Custom Agent
<!-- INVOCATION MODE — declare before loading any skill -->
<!-- Set mode to one of: feature-implementation | bug-fix -->
mode: feature-implementation

> **Change the `mode:` value above to `bug-fix` when diagnosing or fixing a defect.**  
> The mode determines which additional files this agent loads (see Workflow below).

---

## Agent Identity
<!-- token-budget: 30 words -->

| Field | Value |
|-------|-------|
| **Agent Name** | FilterBar Agent |
| **Paired Skill** | `/docs/ai-agents/skills/filterbar-skill.md` |
| **Feature Scope** | FilterBar only — `FilterType.FilterBar` path in `Filter<T>`, `FilterBarRenderer`, and `FilterInput` |
| **Component** | `SfGrid<TValue>` — `Syncfusion.Blazor.Grids` |

---

## Mandatory Load Order
<!-- token-budget: 60 words -->

This agent MUST load files in this exact order before generating any output.

### Mode: `feature-implementation`

```
1. /docs/ai-agents/skills/filterbar-skill.md
2. /docs/ai-agents/prompts/regression-verification-prompt.md
```

### Mode: `bug-fix`

```
1. /docs/ai-agents/skills/filterbar-skill.md
2. /docs/ai-agents/skills/feature-impact-analysis.md
3. /docs/ai-agents/prompts/regression-verification-prompt.md
```

> ⛔ Do NOT load skills for any other feature (Menu, Excel, CheckBox filter, Sorting, etc.) in the same invocation.  
> ⛔ If the request spans two features (e.g., FilterBar + Editing keyboard interaction), split into two separate agent calls.

---

## Invocation Rules
<!-- token-budget: 50 words -->

1. **Declare mode first** — update `mode:` at the top of this file before starting.
2. **Load files in order** — do not skip steps in the load order above.
3. **Scope is FilterBar only** — this agent must not modify Menu filter, Excel filter, CheckBox filter, or `FilterType` enum values.
4. **One feature per invocation** — if a fix requires changes to `FocusHandler<T>` or `Edit<T>` for keyboard routing, stop and invoke those features' agents separately.
5. **Regression verification is mandatory** — every change, no matter how small, must pass `/docs/ai-agents/prompts/regression-verification-prompt.md` before the agent outputs a final answer.

---

## Workflow: Feature Implementation Mode
<!-- token-budget: 80 words -->

Use when adding new FilterBar functionality or changing existing filter bar behaviour.

```
Step 1 — Read the requirements folder:
         docs/requirements/features/filterbar/ (all .md files)
         If folder does not exist, request creation per training/02-requirements-analysis/

Step 2 — Load filterbar-skill.md (pre-distilled architecture knowledge)

Step 3 — Extract the relevant code chunk from the affected file:
         - Filter.cs (business logic — FilterByColumn, RemoveFilterColumnByField, UpdateFilterMessage)
         - FilterInput.razor (UI — ProcessFilter, GetActualFilterValue, UpdateValue, StartTimer)
         - FilterBarRenderer.razor (row rendering — GetActualColumns, UpdateFilterBarColumns)
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md
         Target budget: ≤ 8,000 tokens of source input per sub-agent call

Step 4 — Use the Prompt Template from filterbar-skill.md with mode: feature-implementation
         Fill in SCOPE and INPUT sections

Step 5 — Validate output against:
         - docs/code-guidelines/coding-standards.md
         - docs/code-guidelines/naming-conventions.md
         - docs/code-guidelines/error-handling.md

Step 6 — Run regression verification:
         Fill in regression-verification-prompt.md and submit to Code Review AI
         Do NOT proceed until verdict: APPROVED

Step 7 — Output final implementation with test cases
```

---

## Workflow: Bug Fix Mode
<!-- token-budget: 100 words -->

Use when diagnosing and resolving a defect in FilterBar behaviour.

```
Step 1 — Read the bug folder:
         docs/requirements/bugs/<work-item-id>/ (description.md, root-cause.md, fix-approach.md)
         fix-approach.md MUST be approved by Scrum Master AI before proceeding

Step 2 — Load filterbar-skill.md (scoped to the affected area only)

Step 3 — Load feature-impact-analysis.md
         Complete all 5 steps of the blast radius analysis
         All checkboxes must be ticked before writing any code

Step 4 — Extract the minimal code chunk covering the buggy method only
         Priority chunk targets:
           Timing bugs → FilterInput.razor: StartTimer / DisplayTimeEvent / ProcessFilter
           Operator bugs → FilterInput.razor: GetActualFilterValue / GetOperator(string)
           State bugs → Filter.cs: FilterByColumn / RemoveFilterColumnByField / UpdateFilterMessage
           Rendering bugs → FilterBarRenderer.razor: GetActualColumns / UpdateFilterBarColumns
           Keyboard bugs → FocusHandler.cs lines 211–285 (invoke FocusHandler agent separately)
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md

Step 5 — Use the Prompt Template from filterbar-skill.md with mode: bug-fix
         Fill in SCOPE (exact method) and INPUT (chunk)

Step 6 — Validate output:
         - Compiles: zero errors, zero analyzer warnings
         - XML comments on any modified public member
         - No behavioral change outside the bug scenario
         - No new direct module dependencies
         - #pragma warning disable BL0005 suppressions preserved where present

Step 7 — Run regression verification:
         Fill in regression-verification-prompt.md and submit to Code Review AI
         Do NOT proceed until verdict: APPROVED

Step 8 — Output fix with regression test cases (TC-01 through TC-N, Given-When-Then format)
```

---

## Out-of-Scope Guard
<!-- token-budget: 40 words -->

This agent **MUST NOT**:

- Modify `FilterMenuRenderer.razor` / `FilterCheckBoxRenderer.razor` / `ExcelBase.razor` — those are separate filter types
- Modify `FocusHandler<T>` or `Edit<T>` keyboard paths directly — invoke their respective agents
- Change `SfGrid.Properties.cs` without an explicit API review task being referenced
- Add new `[Parameter]` properties without authorization
- Change the query build order in `Internal/Actions/Data.cs` without a separate Data task
- Remove or modify `#pragma warning disable BL0005` suppressions in `FilterByColumn` without Architect AI approval
- Alter `AdaptiveDialogRenderer.razor`'s `FilterType.FilterBar` exclusion guard

If any of the above is required by the request, **stop** and raise it as a separate task with the Architect AI.

---

## Quick Reference
<!-- token-budget: 30 words -->

| Need | Go To |
|------|-------|
| Filter module | `Internal/Actions/Filter.cs` |
| FilterBar row renderer | `Internal/Renderer/Filter/FilterBarRenderer.razor` |
| Per-cell input renderer | `Internal/Renderer/Filter/FilterInput.razor` |
| Public API entry points | `SfGrid.Methods.cs` → `FilterByColumnAsync`, `ClearFilteringAsync` |
| Query application | `Internal/Actions/Data.cs` → `GenerateQuery()` lines ~410–435 |
| Keyboard nav (read-only ref) | `Internal/Actions/FocusHandler.cs` lines 211–285 |
| Edit Shift+Tab path (read-only ref) | `Internal/Actions/Edit.cs` lines 361–398 |
| Settings component | `GridFilterSettings.razor.cs` |
| Predicate model | `GridFilterColumn.cs` |
| Frozen column slicing | `Internal/Base/InternalClass.cs` → `FilterBarParameters` |
| Locale keys | `Internal/Base/InternalClass.cs` → `FilterbarTitle`, `FilterBar` |
| JS functions | `sf-grid.js` → `focusFilterBar`, `updateFilterBarCell`, `searchClear` |
| Interaction risks | `filterbar-skill.md` → Interaction Matrix |
| Chunking guide | `training/04-code-processing/optimal-chunking-strategies.md` |
| PR checklist | `training/06-reference/quick-reference-guides.md` §6 |
