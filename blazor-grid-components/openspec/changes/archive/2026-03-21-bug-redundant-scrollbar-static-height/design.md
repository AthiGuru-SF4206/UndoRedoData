# Design: Vertical Scrollbar Suppression via Inline overflow-y Control

**Change ID**: bug-redundant-scrollbar-static-height  
**Date**: 2026-03-21  

---

## APPROACH

Set `overflow-y: hidden` as an **inline style** on the `.e-content` element when content does not overflow the static grid height. Clear it (`''`) when content overflows, deferring to the `e-yscroll` CSS class.

The prerequisite for `setPadding()` to apply the correct branch is that `removeUnwantedScroll('Height')` returns `true` when content fits. The original implementation used `content.scrollHeight` for this check — but because the `e-yscroll` CSS class applies `overflow-y: scroll`, the browser always reserves a scrollbar gutter, inflating `scrollHeight` beyond the actual table height and causing `removeUnwantedScroll` to always return `false`. The fix corrects the height comparison to use `getContentTable().offsetHeight` (actual table height) plus the horizontal scrollbar height when one is rendered.

This is the minimum-surface fix: three method bodies, no class changes, no new APIs.

---

## ALTERNATIVES CONSIDERED

### Alt A — Remove `e-yscroll` when not scrolling
**Rejected.** The class is used as a compound selector in `windowResized()` (`sf-grid-fn.ts:1051`) and in the selection module (`selection.ts:268`). Removing it breaks resize detection and selection row-height calculation.

### Alt B — Change `e-yscroll` CSS to `overflow-y: auto` instead of `scroll`
**Rejected as standalone.** Even `overflow-y: auto` with a fixed height can cause browsers to reserve scrollbar layout space before content is painted, producing the same visual artifact on first paint. Would also require changes to the shared Syncfusion CSS package, outside this component's scope.

### Alt C — Defer scrollbar class entirely to JS (remove from C# render)
**Rejected.** `GetContentClassName()` is called during server-side render. Removing `e-yscroll` from the initial markup would break all the `querySelector('.e-content.e-yscroll')` lookups that fire synchronously during JS initialisation — before any `setPadding()` correction has run.

### Alt D (chosen) — Write `overflow-y: hidden` inline on initial render; JS overrides as needed
**Selected.** Inline styles have higher specificity than class rules. Writing `overflow-y: hidden` on first paint costs nothing (one additional CSS property in the style attribute) and is overridden the moment `setPadding()` completes its first pass in `contentReady()`. This closes the first-paint window and the post-JS-correction window simultaneously.

---

## ARCHITECTURE

```
┌─────────────────────────────────────────────────────────────────┐
│                    FIX FLOW (two layers)                        │
└─────────────────────────────────────────────────────────────────┘

SERVER-SIDE RENDER  (C#)
  SfGrid.razor  →  GetContentStyle()
  ─────────────────────────────────────────────────────────────────
  BEFORE:  style="height:325px"
  AFTER:   style="height:325px;overflow-y:hidden"
                              ↑ browser paints no scrollbar arrows

  GetContentClassName() → unchanged → still emits "e-content e-yscroll"
                                       (selector integrity preserved)

CLIENT-SIDE CORRECTION  (JS, runs after Blazor renders)
  sf-grid.ts → contentReady() → scrollModule.setPadding()
  ─────────────────────────────────────────────────────────────────
  scroll.ts  removeUnwantedScroll('Height')  ← ROOT FIX

  BEFORE:  actualScrollHeight = content.scrollHeight [+ 17 if h-scroll]
           → e-yscroll CSS class inflates scrollHeight via gutter reservation
           → offsetHeight >= actualScrollHeight is NEVER true for small data
           → always returns false → scrollbar-needed path always taken

  AFTER:   tableHeight = getContentTable().offsetHeight   // actual data height
           actualScrollHeight = tableHeight + (isHScrollRendered ? getScrollBarWidth() : 0)
           → comparison is against real table height, not CSS-inflated scrollHeight
           → returns true correctly when data fits inside static grid height

  scroll.ts  setPadding()

  if (removeUnwantedScroll('Height')) {          // content fits — now fires correctly
      this.content.style.overflow = 'auto';      // existing (unchanged)
      this.content.style.overflowY = 'hidden';   // ← suppresses vertical scrollbar
      ...
      return;
  }
  // content overflows — scrollbar needed
  this.content.style.overflowY = '';             // ← clears any prior hidden override

  Called again by:
    • windowResized() (sf-grid-fn.ts:1062)       → resize re-evaluates ✓
    • contentReady() after filter/sort/group/page → data change re-evaluates ✓
```

---

## PATTERNS

- **Inline style specificity**: `element.style.overflowY` overrides any class-applied `overflow-y`. Setting it to `''` (empty string) removes the inline property entirely, handing control back to the stylesheet.
- **Axis-specific overflow control**: `overflow` shorthand sets both axes simultaneously. Using `overflowY` directly avoids interfering with `overflow-x` (horizontal scrollbar).
- **No new events or interop calls**: The fix is entirely within existing call chains. No new `DotNetObjectReference` calls, no new JS interop methods.

---

## FILES AFFECTED

| File | Method | Lines (current) | Change |
|---|---|---|---|
| `src/SfGrid.razor.cs` | `GetContentStyle()` | 596–608 | Add `overflow-y:hidden` to static-height initial style string (first-paint suppression) |
| `scripts/scroll.ts` | `removeUnwantedScroll()` | 65–67 | Replace `content.scrollHeight` with `getContentTable().offsetHeight + hScrollBarHeight` |
| `scripts/scroll.ts` | `setPadding()` | 124–126 (content-fits branch) | Add `this.content.style.overflowY = 'hidden'` |
| `scripts/scroll.ts` | `setPadding()` | 132 (scrollbar-needed path) | Add `this.content.style.overflowY = ''` |

---

## CROSS-FEATURE IMPACT

| Feature | Impact | Reason |
|---|---|---|
| Virtualization (`EnableVirtualization`) | None | Always has `scrollHeight > offsetHeight`; `removeUnwantedScroll` returns false; new `overflowY = ''` line runs, restoring CSS class control |
| Infinite Scroll | None | Same as virtualization |
| Frozen Columns + Column Virtualization | None | `GetContentStyle()` guard `GetFrozenCount() != 0 && EnableColumnVirtualization` appends `overflow: hidden auto` after our new property; shorthand overrides |
| Grouping expand/collapse | None | Routes through `contentReady()` → `setPadding()` on every data change |
| Filtering | None | Same as grouping |
| Sorting | None | Row count unchanged; `setPadding()` re-evaluates; `overflowY` stays `''` |
| Paging | None | Per-page data change triggers `contentReady()` → `setPadding()` |
| Selection | None | `e-yscroll` class stays on element; `selection.ts:268` querySelector unaffected |
| Browser resize | None | `windowResized()` → `setPadding()` → fix re-evaluates |

---

## SECURITY IMPACT

None. No new DOM input, no new JS interop, no `MarkupString`, no user-controlled values involved.

## ACCESSIBILITY IMPACT

Positive. Removing a spurious scrollbar improves screen-reader and keyboard navigation experience — no phantom scroll target to navigate through.

## PERFORMANCE IMPACT

Negligible. One additional CSS property written per `setPadding()` call. `setPadding()` is already called on every `contentReady()` cycle; the cost is a single string assignment to `element.style.overflowY`.
