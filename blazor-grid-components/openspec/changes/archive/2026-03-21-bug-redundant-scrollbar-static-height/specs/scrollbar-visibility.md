# Spec: Scrollbar Visibility with Static Grid Height

**Domain**: Scroll Management  
**Component**: `SfGrid<TValue>`  
**Date**: 2026-03-21  

---

## Requirements

### REQ-SB-01: No vertical scrollbar when content fits static height

**WHEN** `Height` is set to a static pixel value (e.g., `Height="325"`)  
**AND** the total content height (`scrollHeight`) is less than or equal to the container height (`offsetHeight`)  
**THEN** the `.e-content` element MUST NOT render a visible vertical scrollbar or scroll arrows.

**Testable scenario**:
```
Given: SfGrid Height="325", DataSource = 3 rows (each ~42px ≈ 126px total)
When:  Grid renders
Then:  .e-content inline style contains overflow-y:hidden
And:   No vertical scrollbar arrows are visible
```

---

### REQ-SB-02: Vertical scrollbar MUST appear when content overflows static height

**WHEN** `Height` is set to a static pixel value  
**AND** `scrollHeight > offsetHeight`  
**THEN** the `.e-content` element MUST render a vertical scrollbar.

**Testable scenario**:
```
Given: SfGrid Height="325", DataSource = 50 rows (each ~42px ≈ 2100px total)
When:  Grid renders
Then:  .e-content inline style does NOT contain overflow-y:hidden
And:   Vertical scrollbar is visible and functional
```

---

### REQ-SB-03: Scrollbar state MUST update after data changes

**WHEN** data is filtered, grouped, sorted, or paged such that content height changes  
**THEN** the scrollbar visibility MUST reflect the new content height within one render cycle.

**Testable scenario**:
```
Given: SfGrid Height="400", DataSource = 100 rows (scrollbar visible)
When:  Filter applied → 3 rows remain
Then:  overflow-y:hidden is set → scrollbar hidden

And:
Given: SfGrid Height="400", DataSource = 3 rows (scrollbar hidden)
When:  Filter cleared → 100 rows restored
Then:  overflow-y:'' is set → CSS class manages scrollbar → scrollbar visible
```

---

### REQ-SB-04: Scrollbar state MUST be stable after browser resize

**WHEN** the browser window is resized  
**AND** content height remains less than static grid height  
**THEN** the vertical scrollbar MUST NOT reappear.

**Testable scenario**:
```
Given: SfGrid Height="325", DataSource = 2 rows, no scrollbar visible
When:  window resize event fires → windowResized() → setPadding()
Then:  overflow-y:hidden still set → scrollbar remains hidden
```

---

### REQ-SB-05: Horizontal scrollbar MUST be unaffected

**WHEN** REQ-SB-01 suppresses the vertical scrollbar  
**THEN** horizontal scrollbar visibility MUST be determined solely by content width vs container width  
**AND** `overflow-x` MUST NOT be modified by the vertical scrollbar suppression.

---

### REQ-SB-06: `e-yscroll` class MUST remain on `.e-content`

**WHEN** vertical scrollbar is suppressed per REQ-SB-01  
**THEN** the CSS class `e-yscroll` MUST still be present on the `.e-content` element  
**BECAUSE** `querySelector('.e-content.e-yscroll')` is used by `windowResized()` and `querySelector('.e-yscroll')` is used by the selection module.

---

### REQ-SB-07: Virtualized grids MUST NOT be affected

**WHEN** `EnableVirtualization="true"`  
**THEN** the vertical scrollbar MUST always be rendered (virtualized grids always overflow)  
**AND** REQ-SB-01 suppression MUST NOT apply.

---

### REQ-SB-08: Content-fits detection MUST use table height, not scrollHeight

**WHEN** determining whether content fits inside a static grid height  
**THEN** the comparison MUST use `getContentTable().offsetHeight` (actual rendered table height)  
**AND NOT** `content.scrollHeight` (which is inflated by the scrollbar gutter reserved by the `e-yscroll` CSS class)  
**AND** if a horizontal scrollbar is rendered, its height (`getScrollBarWidth()`) MUST be added to the table height before comparing against `content.offsetHeight`.

**Rationale**: `overflow-y: scroll` (from `e-yscroll`) causes browsers to reserve gutter space unconditionally, making `scrollHeight > offsetHeight` even when the table fits — defeating the content-fits check if `scrollHeight` is used.
