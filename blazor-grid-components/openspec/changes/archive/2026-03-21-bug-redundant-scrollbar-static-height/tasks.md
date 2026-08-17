# Tasks: bug-redundant-scrollbar-static-height

**Change ID**: bug-redundant-scrollbar-static-height  
**Date**: 2026-03-21  
**Total tasks**: 10  

---

## Test Cases (must FAIL before fix, PASS after fix)

- [x] **Task 1.1** — Verify `StaticHeight_SmallData_ShouldNotHaveYScrollClass` fails before fix  
  File: `tests/Actions/ScrollbarVisibility.razor`  
  Method: `StaticHeight_SmallData_ShouldNotHaveYScrollClass`  
  Assert: `.e-content` inline style contains `overflow-y:hidden`  
  Expected pre-fix result: ❌ FAIL (inline style does not contain `overflow-y:hidden`)

- [x] **Task 1.2** — Verify `StaticHeight_AfterResize_ShouldNotHaveYScrollClass` fails before fix  
  File: `tests/Actions/ScrollbarVisibility.razor`  
  Method: `StaticHeight_AfterResize_ShouldNotHaveYScrollClass`  
  Assert: After `Refresh()`, `.e-content` inline style contains `overflow-y:hidden`  
  Expected pre-fix result: ❌ FAIL

- [x] **Task 1.3** — Verify `StaticHeight_LargeData_MustRetainYScrollClass` fails before fix  
  File: `tests/Actions/ScrollbarVisibility.razor`  
  Method: `StaticHeight_LargeData_MustRetainYScrollClass`  
  Assert: `.e-content` inline style does NOT contain `overflow-y:hidden` for 50-row dataset  
  Expected pre-fix result: ❌ FAIL (because `removeUnwantedScroll` always returns `false` pre-fix, `overflowY = ''` in the scrollbar-needed path is also never reached; test fails until all JS fixes are applied)

---

## Fix Implementation

- [x] **Task 1.5 — JS fix (content-fits detection)**: Modify `removeUnwantedScroll()` in `scripts/scroll.ts` (lines 65–67)  
  Change type: **Modify** — **primary fix**  
  Detail: Replace `content.scrollHeight` (inflated by `e-yscroll` gutter) with `getContentTable().offsetHeight` for actual table height. Replace hardcoded `+ 17` with `Scroll.getScrollBarWidth()`. New comparison: `content.offsetHeight >= tableHeight + (isHScrollRendered ? Scroll.getScrollBarWidth() : 0)`.  
  Without this fix, `removeUnwantedScroll` always returns `false` for small data and all subsequent `setPadding()` branch corrections are unreachable.

- [x] **Task 2 — C# fix**: Modify `GetContentStyle()` in `src/SfGrid.razor.cs` (lines 596–608)  
  Change type: **Modify**  
  Detail: When `Height` is static (not `"auto"` and not percentage), append `;overflow-y:hidden` to the initial inline style string.  
  Guard: Must NOT apply when `Height.Contains('%')` (percentage heights use different height strategy) and must NOT conflict with the frozen+column-virtualization branch that appends `overflow: hidden auto`.

- [x] **Task 3 — JS fix (content-fits branch)**: Modify `setPadding()` in `scripts/scroll.ts` (lines 124–126)  
  Change type: **Modify**  
  Detail: In the `removeUnwantedScroll('Height') === true` branch, after setting `this.content.style.overflow`, add `this.content.style.overflowY = 'hidden'` to explicitly suppress vertical scrollbar axis.

- [x] **Task 4 — JS fix (scrollbar-needed branch)**: Modify `setPadding()` in `scripts/scroll.ts` (line ~132, fall-through after early return)  
  Change type: **Modify**  
  Detail: At the start of the scrollbar-needed execution path (after the `removeUnwantedScroll` early-return block), add `this.content.style.overflowY = ''` to clear any prior `hidden` override and restore CSS class control.

---

## Verification

- [x] **Task 5 — Run failing tests, confirm all 3 now PASS**  
  Command: `dotnet test tests/Syncfusion.Blazor.Tests.Grids.csproj -c Debug --filter "StaticHeight_SmallData_ShouldNotHaveYScrollClass|StaticHeight_AfterResize_ShouldNotHaveYScrollClass|StaticHeight_LargeData_MustRetainYScrollClass"`  
  Expected: ✅ 3/3 PASS

- [x] **Task 6 — Run full scroll test suite, confirm no regressions**  
  Command: `dotnet test tests/Syncfusion.Blazor.Tests.Grids.csproj -c Debug --filter "ScrollbarVisibility"`  
  Expected: ✅ All existing + new tests PASS

- [x] **Task 7 — Run full test suite**  
  Command: `dotnet test tests/Syncfusion.Blazor.Tests.Grids.csproj -c Debug`  
  Expected: ✅ All tests PASS, 0 new failures

- [x] **Task 8 — Compile TypeScript**  
  Command: `npm run build` (or `gulp compile` per `gulpfile.js`)  
  Expected: ✅ 0 TypeScript errors, 0 warnings for modified files

- [x] **Task 9 — Build C# project**  
  Command: `dotnet build src/Syncfusion.Blazor.Grid.csproj -c Debug`  
  Expected: ✅ 0 errors, 0 warnings

---

## Test Suite Status (Pre-Fix)

| Test | File | Expected Pre-Fix | Expected Post-Fix |
|---|---|---|---|
| `StaticHeight_SmallData_ShouldNotHaveYScrollClass` | `ScrollbarVisibility.razor` | ❌ FAIL | ✅ PASS |
| `StaticHeight_AfterResize_ShouldNotHaveYScrollClass` | `ScrollbarVisibility.razor` | ❌ FAIL | ✅ PASS |
| `StaticHeight_LargeData_MustRetainYScrollClass` | `ScrollbarVisibility.razor` | ❌ FAIL | ✅ PASS |
| `StaticSmallContentNoScrollbar` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `DynamicRecordAdditionScrollbarAppears` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `VirtualFrozenColumnsScrollbarCorrect` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `GroupingExpandCollapseScrollbarUpdates` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `ScrollFilteringInteraction` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `ScrollSortingInteraction` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
| `ScrollPagingInteraction` | `ScrollbarVisibility.razor` | ✅ PASS | ✅ PASS |
