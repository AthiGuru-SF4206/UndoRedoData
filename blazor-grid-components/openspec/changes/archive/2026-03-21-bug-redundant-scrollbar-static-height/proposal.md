# Proposal: Vertical Scrollbar Rendered Unnecessarily When Content Height < Static Grid Height

**Change ID**: bug-redundant-scrollbar-static-height  
**Date**: 2026-03-21  
**Confidence**: HIGH  
**Risk**: LOW  

---

## PROBLEM

When `Height` is set to a static pixel value (e.g., `Height="325"`) on the Blazor Grid and the bound dataset is small (2–3 rows), a vertical scrollbar — including top/bottom scroll arrows — is rendered even though the total content height is far less than the container height. This occurs without enabling any explicit scroll properties, is reproducible consistently on initial render, and reappears after browser window resizing.

**User impact**: Visual inconsistency that misleads users into thinking more data exists below the visible area.

---

## ROOT CAUSE

Three-layer fault. See `Exploration.md` for full line-level evidence.

**Layer 0 — JavaScript content-fits detection** (`scripts/scroll.ts`, `removeUnwantedScroll()`, lines 65–67) — **primary fault**:

`removeUnwantedScroll('Height')` determines whether a scrollbar is needed by comparing `content.offsetHeight >= content.scrollHeight`. However, because the `e-yscroll` CSS class applies `overflow-y: scroll`, the browser unconditionally reserves a vertical scrollbar gutter, inflating `content.scrollHeight` beyond the actual table height. As a result, `offsetHeight >= scrollHeight` is **never `true`** for small data — `removeUnwantedScroll` always returns `false`, causing `setPadding()` to always take the scrollbar-needed path, and the subsequent `overflowY` corrections in `setPadding()` are never reached.

The correct comparison is `content.offsetHeight >= getContentTable().offsetHeight + horizontalScrollbarHeight`, using actual table height rather than CSS-inflated `scrollHeight`.

**Layer 1 — C# initial render** (`src/SfGrid.razor.cs`, `GetContentStyle()`, lines 596–608):

`GetContentStyle()` writes only `height:{value}` as the inline style for the content element. It does **not** write `overflow-y: hidden`. On first paint, before any JavaScript runs, the element carries the `e-yscroll` CSS class (added unconditionally by `GetContentClassName()` whenever `Height != "auto"`). That class applies an overflow rule that causes the browser to render scrollbar arrows even when nothing overflows.

**Layer 2 — JavaScript correction gap** (`scripts/scroll.ts`, `setPadding()`, lines 124–126):

After first render, `setPadding()` is called from `contentReady()`. When `removeUnwantedScroll('Height')` returns `true` (content fits), `setPadding()` sets `this.content.style.overflow = 'auto'` — which removes header padding correctly. However, it never sets `overflowY = 'hidden'` explicitly. The inline `overflow` shorthand sets `overflow-y: auto` which, combined with a fixed container height, still causes browsers to reserve scrollbar space in the DOM layout.

**Why `e-yscroll` class MUST NOT be removed**: `querySelector('.e-content.e-yscroll')` is used by `windowResized()` (`sf-grid-fn.ts` line 1051) and `querySelector('.e-yscroll')` by the selection module (`selection.ts` line 268). Removing the class breaks resize handling and selection height calculations.

---

## SOLUTION

**Three targeted, minimal changes — no architectural impact:**

### Change 0 — `scripts/scroll.ts` → `removeUnwantedScroll()` — **primary fix**
Replace the `content.scrollHeight`-based comparison with `getContentTable().offsetHeight + horizontalScrollbarHeight`. This corrects the content-fits detection so it uses actual rendered table height instead of the CSS-inflated `scrollHeight`. When a horizontal scrollbar is present its height (`Scroll.getScrollBarWidth()`) is added, since it consumes vertical space inside the container.

### Change 1 — `src/SfGrid.razor.cs` → `GetContentStyle()`
Append `overflow-y:hidden` to the initial inline style when `Height` is static and not percentage-based. This ensures that on first paint, before JS runs, the browser does not render a scrollbar track. The JS layer overrides this inline value after `removeUnwantedScroll` correctly evaluates content height.

### Change 2 — `scripts/scroll.ts` → `setPadding()`
In the "content fits" branch (when `removeUnwantedScroll('Height')` returns `true`): explicitly set `this.content.style.overflowY = 'hidden'` to suppress the vertical scrollbar axis.  
In the "scrollbar needed" branch (fall-through): explicitly set `this.content.style.overflowY = ''` to clear any prior hidden override and allow the `e-yscroll` CSS class to manage scrollbar visibility.

**What is preserved:**
- `e-yscroll` class stays on the DOM element — all `querySelector` calls intact
- `overflow-x` is never touched — horizontal scrollbar unaffected
- Virtualization, infinite-scroll, frozen column paths unaffected (`getContentTable().offsetHeight` is always greater than `content.offsetHeight` for these cases)
- Grouping, filtering, paging, sorting all route through `contentReady()` → `setPadding()` → fix applies correctly on every data change

---

## ACCEPTANCE CRITERIA

1. Grid with `Height="325"` and ≤ 3 rows renders **no vertical scrollbar** on initial load
2. Grid with `Height="325"` and ≥ 50 rows **still renders** a vertical scrollbar
3. After browser resize, a small-data grid does **not** re-show the scrollbar
4. Horizontal scrollbar is **unaffected** in all scenarios
5. All existing scrollbar visibility tests continue to pass
6. Virtualization grid (`EnableVirtualization=true`) scrollbar is **unaffected**

---

## NON-GOALS

- No changes to the `e-yscroll` CSS class definition
- No changes to `GetContentClassName()` — the class must stay unconditional
- No changes to horizontal scroll behaviour
- No API changes (parameters, events, public methods)

---

## SPECS AFFECTED

- **MODIFIED**: `openspec/specs/scroll/spec.md` — add requirement: vertical scrollbar MUST NOT render when `scrollHeight ≤ offsetHeight` with static grid height
- **NEW**: `openspec/changes/bug-redundant-scrollbar-static-height/specs/scrollbar-visibility.md`

---

## FILES TO MODIFY

| File | Method | Change Type |
|---|---|---|
| `scripts/scroll.ts` | `removeUnwantedScroll()` | Modify — replace `content.scrollHeight` with `getContentTable().offsetHeight + hScrollBarHeight` |
| `src/SfGrid.razor.cs` | `GetContentStyle()` | Modify — add `overflow-y:hidden` to initial inline style |
| `scripts/scroll.ts` | `setPadding()` | Modify — add explicit `overflowY` axis control in both branches |
