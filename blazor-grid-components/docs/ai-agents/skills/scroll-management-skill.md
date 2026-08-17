---
name: scroll-management-skill
description: Expert knowledge for scroll management in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to scroll behaviour, including row scrolling, column scrolling, virtual scroll coordination, infinite scroll on-demand loading, focus-driven scrolling, frozen column/row synchronization, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for scroll management in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**  
When working on scroll-related bugs or implementing scroll features, load this skill first before consulting architecture files. Use it to understand viewport rendering, scroll offset calculations, frozen column coordination, and scroll event chains.

---

## Knowledge References
<!-- token-budget: 60 words -->

**Training Files Consulted**:
- `/docs/training/00-START-HERE.md`
- `/docs/training/01-getting-started/architecture-overview.md`
- `/docs/training/TRAINING-INDEX.md`

**Architecture Files**:
- `/docs/architecture/system-architecture.md`
- `/docs/code-guidelines/coding-standards.md`

**Live Source Locations**:
- `scripts/scroll.ts` — DOM scrolling, padding, height calculations
- `scripts/virtual-scroll.ts` — viewport renderer, cache management, offset tracking
- `scripts/infinite-scroll.ts` — on-demand loading, scroll position tracking
- `src/GridInfiniteScrollSettings.razor.cs` — infinite scroll configuration
- `Internal/Actions/VirtualScroll.cs` — row/column virtualization orchestration
- `Internal/Actions/InfiniteScroll.cs` — on-demand data fetching
- `Internal/Base/GridJSInteropAdaptor.cs` — scroll event bridge to JS

---

## Training Insights Applied
<!-- token-budget: 80 words -->

**Key Rules from Training**:
1. Scroll management is a **hybrid concern**: C# handles viewport calculations and data fetch decisions; JavaScript handles DOM measurement and scroll position tracking.
2. **Three simultaneous environments**: .NET state, Blazor render tree, JS scroll coordinates must remain in sync.
3. **EventAggregator is mandatory**: Cross-module communication (e.g., VirtualScroll + Grouping) must use pub-sub, never direct calls.
4. **PropertyChanges-driven updates**: Only modules whose parameters changed are notified; scroll state must be re-initialized only when EnableVirtualization or EnableInfiniteScrolling changes.
5. **No StateHasChanged() calls**: Use internal render scheduling via grid's methods.

---

## Code Location Map
<!-- token-budget: 80 words -->

| File | Responsibility |
|------|-----------------|
| `scripts/scroll.ts` | DOM scroll measurement, content padding, frozen column/row height sync |
| `scripts/virtual-scroll.ts` | Viewport rendering, cache, offset tracking, virtual row/col animation |
| `scripts/infinite-scroll.ts` | On-demand data trigger, scroll direction detection, block calculation |
| `Internal/Actions/VirtualScroll.cs` | Row/column cache invalidation, viewport size updates, freeze sync |
| `Internal/Actions/InfiniteScroll.cs` | Block offset calculation, direction detection, data request trigger |
| `Internal/Base/GridJSInteropAdaptor.cs` | JS-to-.NET scroll event bridge, initialization |
| `src/GridInfiniteScrollSettings.razor.cs` | InitialBlocks, MaximumBlocks, EnableCache parameter binding |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

Built from live codebase + training docs cross-reference. Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Scroll + Virtualization | Offset tracking must survive sort/filter/group; cache invalidation on data change | Critical |
| Scroll + InfiniteScroll | Only one scroll mode active at a time; mutual exclusivity enforced | Critical |
| Scroll + Grouping | Group row heights affect virtual offset calculations; group expand/collapse resets scroll cache | High |
| Scroll + Sorting | Sort preserves scroll position unless data count changed; cache offsets invalidated | High |
| Scroll + Filtering | Filter reduces data count; virtual scroll must recalculate viewport start/end; offset cache cleared | High |
| Scroll + Paging | Paging resets scroll to top; infinite scroll incompatible with paging | High |
| Scroll + Editing | Edit mode may trigger focus-driven scroll; cell focus must be visible in viewport | Medium |
| Scroll + Selection | Row selection may trigger scroll-into-view; keyboard nav requires scroll sync | Medium |
| Scroll + FrozenColumns | Horizontal scroll must sync frozen and movable content; offset calculations must account for frozen width | High |
| Scroll + DetailRows | Expanding detail row may shift viewport; scroll cache must be invalidated | Medium |

---

## Prompt Template
<!-- token-budget: 300 words -->

**Mode**: {feature-implementation | bug-fix}  
**Skill**: Scroll Management  

**Context Setup**:
Before implementing any scroll-related change, consult ALL of the following:
1. Read `/docs/training/01-getting-started/architecture-overview.md` (Scroll is a hybrid concern spanning Layers 1, 2, and 3)
2. Review `/docs/training/05-practical-examples/feature-implementation-walkthrough.md` for cross-feature testing patterns
3. Read the Interaction Matrix in this skill (above) — verify your change does not affect listed feature pairs

**Implementation Checklist**:
- [ ] Scroll logic touches **both JavaScript** (scroll.ts, virtual-scroll.ts, infinite-scroll.ts) **and C#** (VirtualScroll.cs, InfiniteScroll.cs)
- [ ] All JS-to-.NET scroll events flow **only** through `GridJSInteropAdaptor.cs`
- [ ] Changes to cache/offset calculations must **preserve scroll position** across sort/filter/group
- [ ] Frozen column horizontal scroll **must sync** with unfrozen content — test both at same time
- [ ] Infinite scroll and row virtualization are **mutually exclusive** — validate only one is enabled
- [ ] Focus-driven scrolling (keyboard navigation) must not interfere with user-initiated scroll
- [ ] Viewport size changes (grid resize, header height changes) must trigger **viewport recalculation**
- [ ] Group expand/collapse, filter apply, sort apply must **invalidate scroll cache** but **preserve position** if possible

**Regression Testing Required**:
- Scroll + Sorting: 10K rows, sort by multiple columns, verify scroll position preserved
- Scroll + Grouping: Group + expand/collapse, verify offset cache invalidated correctly
- Scroll + Filtering: Apply filter to reduce data, verify scroll does not jump beyond new viewport end
- Frozen Columns: Scroll horizontally, verify frozen column stays fixed and movable content scrolls
- Infinite Scroll: Scroll to bottom, verify data block loads without flicker or duplicate rows
- Keyboard Navigation: Tab/Arrow keys in virtualized grid, verify focus stays in viewport and scroll follows

**Code Path References**:
- Scroll initialization: `GridJSInteropAdaptor.Init()` → `sfBlazor.Grid.initialize()`
- Virtual scroll trigger: `VirtualContentRenderer.onScroll()` → `VirtualContentRenderer.scrollHandler()` → `VirtualHelper.calcPage()`
- Infinite scroll trigger: `InfiniteScroll.infiniteScrollHandler()` → `EventAggregator.Trigger("InfiniteScrolling")`
- Cache invalidation: `VirtualScroll.OnAfterRender()` calls `virtualEle.setVirtualHeight()`

