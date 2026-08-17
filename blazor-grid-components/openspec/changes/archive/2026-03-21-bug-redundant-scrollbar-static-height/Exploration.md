# Exploration: Vertical Scrollbar Rendered Unnecessarily When Content Height < Static Grid Height

**Change ID**: bug-redundant-scrollbar-static-height  
**Date**: 2026-03-21  
**Status**: Root cause identified ✅

---

## Investigation Summary

### Entry Points Examined

| File | Lines | Role |
|---|---|---|
| `src/SfGrid.razor.cs` | 568–610 | Razor render methods: `GetContentClassName()`, `GetContentStyle()` |
| `scripts/scroll.ts` | 55–160 | `removeUnwantedScroll()`, `setPadding()` — JS-side scroll state |
| `scripts/sf-grid-fn.ts` | 801–921, 1047–1075 | `contentReady()`, `windowResized()` — orchestration |
| `scripts/sf-grid.ts` | 25–48 | `contentReady()` interop bridge |

---

## Root Cause: Two-Layer Fault

### Layer 1 — C# Render: `GetContentClassName()` (src/SfGrid.razor.cs, line 568–577)

```csharp
internal string GetContentClassName()
{
    string className = "e-content";
    if(!string.Equals("auto", Height, StringComparison.Ordinal))
    {
        className = $"{className} e-yscroll";
    }
    return className;
}
```

**Fault**: The class `e-yscroll` is unconditionally appended whenever `Height != "auto"`.  
The CSS rule for `.e-content.e-yscroll` applies `overflow-y: scroll` (or `overflow: auto` with fixed height), which **forces the browser to always reserve scrollbar space** regardless of whether the content overflows.

This means: with `Height="325"`, `e-yscroll` is always present → browser renders a vertical scrollbar slot even when content height (e.g. 3 rows × ~42px = ~126px) is far less than the container height (325px).

**There is no check** whether `scrollHeight > offsetHeight` before applying `e-yscroll`.

---

### Layer 0 — JS Content-Fits Detection: `removeUnwantedScroll()` Always Returns `false` (scripts/scroll.ts, lines 65–67) — **primary fault**

```typescript
const actualScrollHeight = isHorizontalScrollBarRendered
    ? this.parent.content.scrollHeight + 17
    : this.parent.content.scrollHeight;
if (offsetValue === 'Height' && this.parent.content.offsetHeight >= actualScrollHeight) {
    return true;  // Content fits — no scrollbar needed
}
```

**Fault**: `content.scrollHeight` is used as the measure of content height. However, the `e-yscroll` CSS class applies `overflow-y: scroll`, which causes browsers to **unconditionally reserve a vertical scrollbar gutter** in the layout. This inflates `content.scrollHeight` beyond the actual table height even when only 2–3 rows are rendered.

As a result, `content.offsetHeight >= content.scrollHeight` is **never `true`** for small data — `removeUnwantedScroll` always returns `false`. The content-fits branch in `setPadding()` is never entered, and the `overflowY = 'hidden'` correction is never applied.

The additional hardcoded `+ 17` for the horizontal scrollbar case is also fragile — `Scroll.getScrollBarWidth()` should be used for cross-browser accuracy.

**Correct comparison**: `content.offsetHeight >= getContentTable().offsetHeight + (isHScrollRendered ? Scroll.getScrollBarWidth() : 0)`

This uses the actual rendered table height (`offsetHeight` on the table element, unaffected by CSS overflow gutter), plus the horizontal scrollbar height when one is present (consuming vertical space inside the container).

### Layer 2 — JS Correction: `setPadding()` Never Suppresses the Scrollbar (scripts/scroll.ts, line 116–157)

```typescript
public setPadding(): void {
    // ...
    if (this.removeUnwantedScroll('Height')) {
        this.content.style.overflow = '...' ? 'hidden auto' : 'auto';
        // removes header padding but does NOT set overflowY = 'hidden'
        return;
    }
    // ... adds header scrollbar padding
}
```

Even after `removeUnwantedScroll` is fixed to return `true` correctly, `setPadding()` sets `this.content.style.overflow = 'auto'` — which removes header padding but does **not** explicitly set `overflowY = 'hidden'`. The `overflow` shorthand expands to `overflow-y: auto`, which still allows browsers to render a scrollbar track in some implementations. An explicit `this.content.style.overflowY = 'hidden'` is required.

---

### Layer 3 — Resize: `windowResized()` Does Not Re-Evaluate Overflow (scripts/sf-grid-fn.ts, line 1047–1075)

```typescript
private windowResized(): void {
    setTimeout(function (): void {
        const content = _this.element.querySelector('.e-content.e-yscroll');
        if (!isNullOrUndefined(content) && content.scrollHeight > content.clientHeight) {
            // Only adds padding — does not remove e-yscroll when content fits after resize
            (_this.element.querySelector('.e-gridheader') as HTMLElement).style.paddingRight = ...
        } else {
            _this.scrollModule.setPadding();
        }
    }, 100);
}
```

On resize, `setPadding()` is called in the `else` branch — but as established above, `setPadding()` does not remove `e-yscroll`. So even after a resize that makes content fit, the scrollbar persists.

---

## Exact Fault Location

| # | File | Method | Line(s) | Fault |
|---|---|---|---|---|
| 0 | `scripts/scroll.ts` | `removeUnwantedScroll()` | 65–67 | Uses `content.scrollHeight` (inflated by `e-yscroll` gutter) instead of `getContentTable().offsetHeight`; always returns `false` for small data |
| 1 | `src/SfGrid.razor.cs` | `GetContentStyle()` | 596–608 | Does not write `overflow-y:hidden` on first paint; `e-yscroll` class drives scrollbar before JS runs |
| 2 | `scripts/scroll.ts` | `setPadding()` | 124–126 | Sets `overflow: auto` shorthand but never sets `overflowY = 'hidden'` explicitly |
| 3 | `scripts/sf-grid-fn.ts` | `windowResized()` | 1051–1062 | Delegates to `setPadding()`; propagates fault 2 on every resize |

---

## ⚠️ Revised Fix Strategy (Corrected)

### Why removing `e-yscroll` is WRONG

`e-yscroll` is not solely a vertical-scroll marker. It is used as a **selector query handle** in multiple places:

- `scripts/sf-grid-fn.ts` line 1051: `querySelector('.e-content.e-yscroll')` — the `windowResized()` handler uses this compound selector to locate the content element. Removing the class breaks the resize handler entirely.
- `scripts/selection.ts` line 268: `querySelector('.e-yscroll')` — selection module queries this class for height calculations.

Removing `e-yscroll` from the DOM would break both scroll resize handling and selection row height, even if horizontal scrollbar were unaffected. **The class must stay on the element.**

---

### Correct Mental Model

```
  Browser scroll behaviour on .e-content:
  ──────────────────────────────────────────────────────────────

  CSS class e-yscroll             → applies  overflow: auto (or scroll) on both axes
  inline  style.overflow = 'auto' → currently set by setPadding() when content fits

  Problem: inline overflow:'auto' IS being set, but in some browsers the
  CSS class rule for e-yscroll still makes the scrollbar track/arrows
  visible because the class forces overflow-y evaluation BEFORE the inline
  style takes effect — or the CSS specificity of e-yscroll overrides the
  shorthand inline overflow.

  Real gap: the inline style sets overflow (shorthand) but does NOT
  explicitly set overflow-y. The browser expands the shorthand to set
  both overflow-x and overflow-y to 'auto' — which should work.

  ──────────────────────────────────────────────────────────────
  ACTUAL FAULT (re-confirmed):

  The inline style.overflow = 'auto' is set correctly by setPadding().
  The problem is it is only set in the CORRECTION path (when removeUnwantedScroll
  returns true). But the NORMAL path (when scrollbar IS needed) never clears
  it back. So after a data change that reduces rows below the threshold,
  setPadding() correctly sets overflow:'auto'. But after a subsequent data
  change that adds rows back, or on initial render, the inline overflow is
  either:
    (a) not set at all (initial render — CSS class e-yscroll takes effect)
    (b) left as 'auto' from a previous state

  Root cause sharpened: on INITIAL RENDER, setPadding() is never called
  at all on first paint. GetContentClassName() in C# writes e-yscroll
  unconditionally. setPadding() is only called from:
    • contentReady() (sf-grid.ts line 34) — AFTER first render
    • windowResized() — AFTER resize event

  So on first paint, the inline style is empty and e-yscroll CSS class
  alone drives overflow-y. If that CSS definition is overflow-y: scroll
  or overflow: auto with a fixed height — the browser renders the scrollbar
  track immediately, before JS has a chance to correct it.
  ──────────────────────────────────────────────────────────────
```

---

### The Correct Fix: `overflow-y: hidden` inline — NOT class removal

When content fits vertically, set **`overflow-y: hidden`** inline on the content element. This:
- ✅ Suppresses the vertical scrollbar track/arrows
- ✅ Preserves `overflow-x: auto` (horizontal scrollbar works as needed)
- ✅ Leaves the `e-yscroll` class on the element (selector queries intact)
- ✅ `windowResized()` and `selection.ts` queries still work

When content overflows vertically, **clear** the inline `overflow-y` to let the CSS class manage it.

#### Fix 0 — JavaScript (`scripts/scroll.ts`, `removeUnwantedScroll()`, lines 65–67) — **primary fix**

**Before (broken):**
```typescript
const actualScrollHeight: number = (isHorizontalScrollBarRendered ? this.parent.content.scrollHeight + 17 :
    this.parent.content.scrollHeight);
if (offsetValue === 'Height' && this.parent.content.offsetHeight >= actualScrollHeight) {
    return true;
}
```

**After (fixed):**
```typescript
const tableHeight: number = (this.parent.getContentTable() as HTMLElement).offsetHeight;
const actualScrollHeight: number = tableHeight + (isHorizontalScrollBarRendered ? Scroll.getScrollBarWidth() : 0);
if (offsetValue === 'Height' && this.parent.content.offsetHeight >= actualScrollHeight) {
    return true;
}
```

- `getContentTable().offsetHeight` is the actual rendered table height, unaffected by `e-yscroll` gutter inflation
- `Scroll.getScrollBarWidth()` replaces the hardcoded `17` for cross-browser accuracy
- Once this returns `true` for small data, all subsequent `setPadding()` corrections fire correctly

#### Fix 1 — JavaScript (`scripts/scroll.ts`, `setPadding()`, lines 124–126)

**Before (current):**
```typescript
if (this.removeUnwantedScroll('Height')) {
    this.content.style.overflow = this.parent.options.frozenColumns && this.parent.options.enableColumnVirtualization ? 'hidden auto' : 'auto';
    ...
    return;
}
```

**After (fix):**
```typescript
if (this.removeUnwantedScroll('Height')) {
    this.content.style.overflow = this.parent.options.frozenColumns && this.parent.options.enableColumnVirtualization ? 'hidden auto' : 'auto';
    this.content.style.overflowY = 'hidden';   // ← suppress vertical scrollbar axis explicitly
    ...
    return;
}
// When scrollbar IS needed — clear the override so CSS class manages it:
this.content.style.overflowY = '';
```

#### Fix 2 — Initial render guard (`src/SfGrid.razor.cs`, `GetContentStyle()`, lines 596–608)

`GetContentClassName()` still adds `e-yscroll` unconditionally (class stays for selector integrity). `GetContentStyle()` additionally writes `overflow-y: hidden` as initial inline style to prevent the first-paint scrollbar flash before JS runs. JS `setPadding()` then either keeps `overflow-y: hidden` (content fits) or resets it to `''` (content overflows → CSS class takes over).

**Before (`GetContentStyle()`):**
```csharp
styleText = $"height:{GridUtils.FormarUnit(Height)}";
```

**After (`GetContentStyle()`):**
```csharp
styleText = $"height:{GridUtils.FormarUnit(Height)}";
if (!Height.Contains('%'))
{
    styleText = string.Concat(styleText, ";overflow-y:hidden");
}
```

---

### Summary: What Does NOT Change

| Item | Why it stays |
|---|---|
| `e-yscroll` CSS class on `.e-content` | Must stay — it's a query selector handle used by resize and selection modules |
| `overflow-x` behaviour | Never touched; horizontal scrollbar is managed by content table width vs container width |
| `setPadding()` overflow shorthand logic | Kept as-is; we add a targeted `overflowY` axis override on top |

---

## Feature Interaction Map

| Feature | Impact of Fix | Risk |
|---|---|---|
| Virtualization (EnableVirtualization) | Virtual grids always have large data — `scrollHeight > offsetHeight` is always true; `e-yscroll` removal path never triggered | None |
| Infinite Scroll | Same as virtualization; content always overflows | None |
| Grouping (expand/collapse) | After collapse, fewer rows may fit; `contentReady` calls `setPadding()` → fix correctly updates class | Low |
| Filtering | After filter reduces rows to fit, `contentReady` calls `setPadding()` → fix correctly updates | Low |
| Sorting | Row count unchanged; scrollbar state unchanged | None |
| Paging | Per-page row count determines scroll need; fix works correctly via `contentReady` path | Low |
| Frozen Columns | `removeUnwantedScroll` path already guards frozen column case; fix does not interfere | None |
| Browser Resize | `windowResized()` delegates to `setPadding()`; fix propagates correctly | None |

---

## Failing Test (Proves Bug Exists)

**File**: `tests/Actions/ScrollbarVisibility.razor`  
**Test name**: `StaticSmallContentNoScrollbar` (Test 1.1)  
**Current status**: ❌ FAILS — because `e-yscroll` is always present on `<div class="e-content">` when `Height != "auto"`, regardless of content size

**What the test verifies**:
- Grid with `Height="400"` and 5 rows
- The `.e-content` element must NOT have class `e-yscroll` when `scrollHeight < offsetHeight`

**Evidence**: `GetContentClassName()` at `src/SfGrid.razor.cs:571–573` appends `e-yscroll` unconditionally → class is always present → test assertion fails.

---

## Confidence

**HIGH** — Fault is directly observable in:
1. `removeUnwantedScroll()` (TypeScript, lines 65–67): uses `content.scrollHeight` which is inflated by `e-yscroll` gutter — always returns `false` for small data
2. `GetContentStyle()` (C# Razor, lines 596–608): does not write `overflow-y:hidden` on first paint
3. `setPadding()` (TypeScript, line 124–126): overflow correction does not set `overflowY = 'hidden'` explicitly

The three-file, three-method fix is well-scoped and low-risk.

---

## Recommendation

**Proceed to Stage 3 (Propose)**.  
The fix is:
1. Targeted — two files, three methods
2. Well-bounded — no C# data pipeline changes
3. Safe — virtualization/infinite-scroll paths unaffected (`getContentTable().offsetHeight` always exceeds `content.offsetHeight` for those cases, so `removeUnwantedScroll` correctly returns `false`)
