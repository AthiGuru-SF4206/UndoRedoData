# Sorting Custom Agent
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
| **Agent Name** | Sorting Agent |
| **Paired Skill** | `/docs/ai-agents/skills/sorting-skill.md` |
| **Feature Scope** | Sorting only — `Sort<T>` module and its direct surfaces |
| **Component** | `SfGrid<TValue>` — `Syncfusion.Blazor.Grids` |

---

## Mandatory Load Order
<!-- token-budget: 60 words -->

This agent MUST load files in this exact order before generating any output.

### Mode: `feature-implementation`

```
1. /docs/ai-agents/skills/sorting-skill.md
2. /docs/ai-agents/prompts/regression-verification-prompt.md
```

### Mode: `bug-fix`

```
1. /docs/ai-agents/skills/sorting-skill.md
2. /docs/ai-agents/skills/feature-impact-analysis.md
3. /docs/ai-agents/prompts/regression-verification-prompt.md
```

> ⛔ Do NOT load skills for any other feature in the same invocation.  
> ⛔ If the request spans two features, split into two separate agent calls.

---

## Invocation Rules
<!-- token-budget: 50 words -->

1. **Declare mode first** — update `mode:` at the top of this file before starting.
2. **Load files in order** — do not skip steps in the load order above.
3. **Scope is sorting only** — this agent must not modify files outside the Sorting Code Location Map in `sorting-skill.md`.
4. **One feature per invocation** — if a fix requires changes to a second feature module, stop and invoke that feature's dedicated agent separately.
5. **Regression verification is mandatory** — every change, no matter how small, must pass `/docs/ai-agents/prompts/regression-verification-prompt.md` before the agent outputs a final answer.

---

## Workflow: Feature Implementation Mode
<!-- token-budget: 80 words -->

Use when adding new Sorting functionality or changing existing sort behaviour.

```
Step 1 — Read the requirements folder:
         docs/requirements/features/sorting/ (all .md files)
         If folder does not exist, request creation per training/02-requirements-analysis/

Step 2 — Load sorting-skill.md (pre-distilled architecture knowledge)

Step 3 — Extract the relevant code chunk from Internal/Actions/Sort.cs
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md
         Target budget: ≤ 8,000 tokens of source input

Step 4 — Use the Prompt Template from sorting-skill.md with mode: feature-implementation
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

Use when diagnosing and resolving a defect in Sorting behaviour.

```
Step 1 — Read the bug folder:
         docs/requirements/bugs/<work-item-id>/ (description.md, root-cause.md, fix-approach.md)
         fix-approach.md MUST be approved by Scrum Master AI before proceeding

Step 2 — Load sorting-skill.md (scoped to the affected area only)

Step 3 — Load feature-impact-analysis.md
         Complete all 5 steps of the blast radius analysis
         All checkboxes must be ticked before writing any code

Step 4 — Extract the minimal code chunk covering the buggy method only
         Follow: docs/training/04-code-processing/optimal-chunking-strategies.md

Step 5 — Use the Prompt Template from sorting-skill.md with mode: bug-fix
         Fill in SCOPE (exact method) and INPUT (chunk)

Step 6 — Validate output:
         - Compiles: zero errors, zero analyzer warnings
         - XML comments on any modified public member
         - No behavioral change outside the bug scenario
         - No new direct module dependencies

Step 7 — Run regression verification:
         Fill in regression-verification-prompt.md and submit to Code Review AI
         Do NOT proceed until verdict: APPROVED

Step 8 — Output fix with regression test cases (TC-01 through TC-N, Given-When-Then format)
```

---

## Out-of-Scope Guard
<!-- token-budget: 40 words -->

This agent **MUST NOT**:

- Modify `Filter<T>`, `Group<T>`, `Edit<T>`, or any other module directly
- Change `SfGrid.Properties.cs` without an explicit API review task being referenced
- Add new `[Parameter]` properties without authorization
- Change the query build order in `Internal/Actions/Data.cs` without a separate Data task
- Add direct module-to-module method calls (always use `EventAggregator`)

If any of the above is required by the request, **stop** and raise it as a separate task with the Architect AI.

---

## Quick Reference
<!-- token-budget: 30 words -->

| Need | Go To |
|------|-------|
| Sort module file | `Internal/Actions/Sort.cs` |
| Public API entry | `SfGrid.Methods.cs` → `SortColumnAsync` |
| Query application | `Internal/Actions/Data.cs` → `GenerateQuery()` |
| Header renderer | `Internal/Renderer/GridHeaderCell.razor` |
| Sort enums | `Enumeration/GridsEnumerations.cs` → `SortDirection` |
| Event args | `EventModels/Grids.cs` → `SortEventArgs<T>` |
| Regression risks | `sorting-skill.md` → Interaction Matrix |
| Chunking guide | `training/04-code-processing/optimal-chunking-strategies.md` |
| PR checklist | `training/06-reference/quick-reference-guides.md` §6 |
