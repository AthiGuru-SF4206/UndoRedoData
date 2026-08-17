# FilterMenu Custom Agent
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
| **Agent Name** | FilterMenu Agent |
| **Paired Skill** | `/docs/ai-agents/skills/filtermenu-skill.md` |
| **Feature Scope** | FilterMenu only — `FilterType.Menu` path in `Filter<T>`, `FilterMenuRenderer`, and related dialog UI |
| **Component** | `SfGrid<TValue>` — `Syncfusion.Blazor.Grids` |

---

## Mandatory Load Order
<!-- token-budget: 60 words -->

This agent MUST load files in this exact order before generating any output.

### Mode: `feature-implementation`

```
1. /docs/ai-agents/skills/filtermenu-skill.md
2. /docs/ai-agents/prompts/regression-verification-prompt.md
```

### Mode: `bug-fix`

```
1. /docs/ai-agents/skills/filtermenu-skill.md
2. /docs/ai-agents/skills/feature-impact-analysis.md
3. /docs/ai-agents/prompts/regression-verification-prompt.md
```

> ⛔ Do NOT load skills for any other feature (FilterBar, Excel, CheckBox filter, Sorting, etc.) in the same invocation.  
> ⛔ If the request spans two features (e.g., FilterMenu + ColumnMenu interaction), split into two separate agent calls.

---

## Invocation Rules
<!-- token-budget: 50 words -->

1. **Declare mode first** — update `mode:` at the top of this file before starting.
2. **Load files in order** — do not skip steps in the load order above.
3. **Scope is FilterMenu only** — this agent must not modify FilterBar, Excel filter, CheckBox filter, or `FilterType` enum values.
4. **One feature per invocation** — if a fix requires changes to `ColumnMenu.razor` for column menu routing, stop and invoke that feature's agent separately.
5. **Regression verification is mandatory** — every change, no matter how small, must pass `/docs/ai-agents/prompts/regression-verification-prompt.md` before the agent outputs a final answer.

---

## Workflow: Feature Implementation Mode
<!-- token-budget: 80 words -->

Use when adding new FilterMenu functionality or changing existing filter menu behaviour.

```
Step 1 — Read the requirements folder:
         docs/requirements/features/filtermenu/ (all .md files)
         If folder does not exist, request creation per training/02-requirements-analysis/

Step 2 — Load filtermenu-skill.md (pre-distilled architecture knowledge)

Step 3 — Extract the relevant code chunk from the affected file:
         - Filter.cs (business logic — FilterByColumn, RemoveFilterColumnByField, GetOperator, GetFilterType)
         - FilterMenuRenderer.razor.cs (dialog flow — OpenFilterDialog, FilterBtnClick, ClearBtnClick)
         - FilterMenuRenderer.razor (markup — operator dropdown, value inputs, Filter/Clear buttons)
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md
         Target budget: ≤ 8,000 tokens of source input per sub-agent call

Step 4 — Use the Prompt Template from filtermenu-skill.md with mode: feature-implementation
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

Use when diagnosing and resolving a defect in FilterMenu behaviour.

```
Step 1 — Read the bug folder:
         docs/requirements/bugs/<work-item-id>/ (description.md, root-cause.md, fix-approach.md)
         fix-approach.md MUST be approved by Scrum Master AI before proceeding

Step 2 — Load filtermenu-skill.md (scoped to the affected area only)

Step 3 — Load feature-impact-analysis.md
         Complete all 5 steps of the blast radius analysis
         All checkboxes must be ticked before writing any code

Step 4 — Extract the minimal code chunk covering the buggy method only
         Priority chunk targets:
           Dialog open/close bugs → FilterMenuRenderer.razor.cs: OpenFilterDialog / FilterBtnClick / ClearBtnClick
           Operator list bugs → FilterMenuRenderer.razor.cs: operator list population + GetOperator / GetColumnOperator in Filter.cs
           Value input bugs → FilterMenuRenderer.razor: type-specific input sections (SfAutoComplete / SfNumericTextBox / SfDatePicker etc.)
           Adaptive dialog bugs → FilterMenuRenderer.razor.cs: CloseHandler; AdaptiveDialogRenderer.razor
           Foreign key bugs → Filter.cs: FilterByColumn — ForeignKeyValue field substitution
           State pre-population bugs → FilterMenuRenderer.razor.cs: OpenFilterDialog — existing GridFilterColumn lookup by Column.Uid
           Column menu bugs → invoke ColumnMenu agent separately
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md

Step 5 — Use the Prompt Template from filtermenu-skill.md with mode: bug-fix
         Fill in SCOPE (exact method) and INPUT (chunk)

Step 6 — Validate output:
         - Compiles: zero errors, zero analyzer warnings
         - XML comments on any modified public member
         - No behavioral change outside the bug scenario
         - No new direct module dependencies
         - ShouldRender suppression in FilterMenuRenderer preserved
         - Both cancellation gates (FilterDialogOpening + OnActionBegin FilterBeforeOpen) preserved
         - isnull/isnotnull operator still disables value input and passes null to FilterByColumn

Step 7 — Run regression verification:
         Fill in regression-verification-prompt.md and submit to Code Review AI
         Do NOT proceed until verdict: APPROVED

Step 8 — Output fix with regression test cases (TC-01 through TC-N, Given-When-Then format)
```

---

## Out-of-Scope Guard
<!-- token-budget: 40 words -->

This agent **MUST NOT**:

- Modify `FilterBarRenderer.razor` / `FilterInput.razor` / `FilterCheckBoxRenderer.razor` / `ExcelBase.razor` — those are separate filter types
- Modify `ColumnMenu.razor` directly — invoke the ColumnMenu agent for changes to that surface
- Change `SfGrid.Properties.cs` without an explicit API review task being referenced
- Add new `[Parameter]` properties without authorization
- Change the query build order in `Internal/Actions/Data.cs` without a separate Data task
- Remove or alter the `ShouldRender` suppression in `FilterMenuRenderer` without Architect AI approval
- Remove either of the two dialog-open cancellation gates (`FilterDialogOpening.Cancel` or `OnActionBegin.Cancel` on `FilterBeforeOpen`)
- Alter the operator precedence chain (type defaults → `GridFilterSettings.Operators` → `FilterDialogOpeningEventArgs.FilterOperators`)

If any of the above is required by the request, **stop** and raise it as a separate task with the Architect AI.

---

## Quick Reference
<!-- token-budget: 30 words -->

| Need | Go To |
|------|-------|
| Filter module | `Internal/Actions/Filter.cs` |
| Dialog renderer (markup) | `Internal/Renderer/Filter/FilterMenuRenderer.razor` |
| Dialog renderer (logic) | `Internal/Renderer/Filter/FilterMenuRenderer.razor.cs` |
| Filter type router | `Internal/Renderer/Filter/FilterType.razor` |
| Adaptive dialog container | `Internal/Renderer/AdaptiveDialogRenderer.razor` |
| Column menu "Filter" item | `Internal/Renderer/ColumnMenu.razor` |
| Public API entry points | `SfGrid.Methods.cs` → `FilterByColumnAsync`, `ClearFilteringAsync` |
| Query application | `Internal/Actions/Data.cs` → `GenerateQuery()` |
| Settings component | `GridFilterSettings.razor.cs` |
| Predicate model | `GridFilterColumn.cs` |
| Column filter properties | `GridColumn.cs` → `AllowFiltering`, `FilterSettings`, `FilterEditorSettings`, `FilterTemplate` |
| JS: desktop dialog position | `sf-grid.js` → `sfBlazor.Grid.filterPopupRender` |
| JS: adaptive dialog position | `sf-grid.js` → `sfBlazor.Grid.customFilterDialog` |
| Interaction risks | `filtermenu-skill.md` → Interaction Matrix |
| Chunking guide | `training/04-code-processing/optimal-chunking-strategies.md` |
| PR checklist | `training/06-reference/quick-reference-guides.md` §6 |
