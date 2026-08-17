# Bug-Fix-Resolver Agent Specification

> **Audience**: AI Agents, Developers, QA, Code Reviewers  
> **Purpose**: Define the autonomous workflow for bug resolution with human governance gates, root-cause analysis, and comprehensive regression prevention  
> **Last Updated**: March 19, 2026

---

## Overview

The **Bug-Fix-Resolver Agent** is an autonomous workflow that systematically resolves bugs through a **7-stage lifecycle** aligned with the development workflow governance gates and OpenSpec architecture.

**Key Principles**:
- ✅ **Human-in-the-Loop Governance**: Critical decisions require human reviewer confirmation
- ✅ **TDD-First Approach**: Test cases written BEFORE bug fixes (failing test = proof of bug)
- ✅ **Feature-Aware Analysis**: Integrates feature-impact-analysis-skill to prevent regressions
- ✅ **Security & Performance**: Applies C# standards, security checks, and memory leak analysis
- ✅ **OpenSpec Integration**: Leverages existing domain specs and skill documentation

---

## Stage 1: Bug Intake & Human Confirmation

### Goal
Receive bug report and secure human reviewer approval before investigation begins.

### Inputs
- Bug ticket ID (e.g., BoldDesk ticket, Azure DevOps task)
- Bug description (steps to reproduce, expected vs. actual behavior)
- Environment (Blazor Server/WASM, .NET version, browser)
- Severity level (Critical / High / Medium / Low)

### Agent Actions
1. **Parse Bug Report**
   - Extract reproduction steps
   - Identify affected feature area (sorting, filtering, grouping, editing, etc.)
   - Determine severity classification

2. **Request Human Confirmation**
   - Present structured bug summary to reviewer
   - Ask: "Does this bug description clearly identify the issue and reproduction steps?"
   - Display checklist:
     - [ ] Steps to reproduce are clear and testable
     - [ ] Expected behavior vs. actual behavior clearly stated
     - [ ] Severity level appropriate
     - [ ] No duplicate of existing bug

### Gate: **Human Reviewer Approval Required**
```
REVIEWER_CONFIRMATION_REQUIRED:
  - [ ] Bug is valid and not a duplicate
  - [ ] Reproduction steps are clear
  - [ ] Severity and priority assigned
  - [ ] Initial feature mapping provided (optional)
  → APPROVE / REQUEST_CHANGES / REJECT
```

**Proceed to Stage 2 only after APPROVE**

---

## Stage 2: Issue Reproduction & Root Cause Analysis

### Goal
Confirm the bug exists and identify the exact root cause through systematic analysis.

### Agent Actions

#### 2a. Reproduction Confirmation
1. Create a minimal test case based on provided steps
2. Write a **failing BUnit test** that reproduces the bug
   - Test file: `test/Bugs/<BugID>/Reproduction.cs`
   - Test must fail with the described behavior
   - Document the exact assertion that fails

3. Log reproduction details:
   ```csharp
   // Example failing test structure
   [Fact]
   public async Task BUG_<ID>_SortOrderNotPreservedAfterFilter()
   {
       // Arrange
       var grid = new SfGrid<OrderData>();
       var data = GetTestData(); // 100 items
       
       // Act: Apply sort
       await grid.SetPropertyAsync("SortSettings", new GridSortSettings 
       { 
           Columns = new List<GridSortColumn> 
           { 
               new GridSortColumn { Field = "OrderID", Direction = SortDirection.Ascending }
           }
       });
       
       // Act: Apply filter
       await grid.SetPropertyAsync("FilterSettings", new GridFilterSettings
       {
           Type = FilterType.FilterBar
       });
       // Apply filter to OrderID > 1000
       
       // Assert: Sort order should be preserved
       var rows = grid.GetRows();
       Assert.True(IsSortedByOrderID(rows));
   }
   ```

#### 2b. Feature Domain Mapping
1. Identify which feature(s) are affected by examining:
   - Razor component names (GridSortSettings, GridFilterSettings, etc.)
   - JS interop calls (scripts/sf-grid.ts methods)
   - Event callbacks in the bug description
   
2. Map to OpenSpec domain:
   ```yaml
   Affected_Domains:
     - sorting:
         risk_level: high
         spec: "openspec/specs/sorting/spec.md"
     - filtering:
         risk_level: high
         spec: "openspec/specs/filtering/spec.md"
     - virtualization:
         risk_level: critical
         spec: "openspec/specs/virtualization/spec.md"
   ```

#### 2c. Root Cause Analysis
1. **Code Path Tracing**
   - Follow the reproduction steps through the component lifecycle
   - Identify exact file(s) and method(s) involved
   - Use grep to search for related code patterns

2. **Scan Component Files**
   - Review `SfGrid.Lifecycle.cs` (lifecycle hooks)
   - Review `SfGrid.Properties.cs` (parameter changes)
   - Review `SfGrid.Methods.cs` (public API)
   - Review domain-specific files (e.g., `GridSortSettings.razor.cs`)

3. **Run Feature-Impact-Analysis**
   - Invoke `feature-impact-analysis-skill` for affected domains
   - Generate interaction matrix to identify secondary effects
   - Document all affected features in `ANALYSIS.md`:
     ```markdown
     ## Bug <ID> Root Cause Analysis
     
     ### Reproduction Confirmed
     ✅ Failing test: `test/Bugs/<ID>/Reproduction.cs:TestName`
     
     ### Root Cause
     **File**: `src/Internal/Actions/Sort.cs`
     **Method**: `UpdateSortOrder()` (lines 45-67)
     **Issue**: Sort state not persisted when filter context changes
     - Current: Sort descriptor cleared on FilterSettings update
     - Expected: Sort descriptor preserved across filter changes
     - Root: Missing synchronization in lifecycle.OnParametersSet()
     
     ### Affected Components
     - GridSortSettings (primary)
     - GridFilterSettings (secondary - triggers the bug)
     
     ### Cross-Feature Impact (Feature-Impact-Analysis)
     | Feature | Risk | Must Test |
     |---------|------|-----------|
     | Sorting | Critical | Sort + Filter combination |
     | Filtering | High | Filter doesn't clear sort |
     | Grouping | Medium | Group + Sort + Filter |
     | Virtualization | High | Virtualization + Sort reset |
     
     ### Proposed Root Cause
     The `OnParametersSet()` lifecycle method in `SfGrid.Lifecycle.cs:89-110`
     calls `FilterSettings.OnUpdate()` which unconditionally clears the sort state
     (line 105: `SortDescriptors.Clear()`).
     
     ### Confidence Level
     **HIGH** — Root cause identified with line numbers and reproduction test
     ```

4. **Document Findings**
   - Create `docs/requirements/bugs/<BugID>/ANALYSIS.md`
   - Include:
     - Exact file paths and line numbers
     - Reproduction test that fails
     - Root cause hypothesis with evidence
     - Feature impact matrix
     - Confidence level (HIGH / MEDIUM / LOW)

### Gate: **Human Architect Review Required**
```
ARCHITECT_ROOT_CAUSE_APPROVAL:
  - [ ] Root cause identified with line numbers
  - [ ] Failing test reproduces the bug
  - [ ] Feature impact analysis completed
  - [ ] Confidence level assessed
  - [ ] No conflicting prior fixes documented
  → APPROVE / REQUEST_INVESTIGATION / REDIRECT
```

**Proceed to Stage 3 only after APPROVE**

---

## Stage 3: Test-Driven Development (TDD) - Design Phase

### Goal
Define **all** test cases BEFORE writing fix code (TDD approach).

### Agent Actions

#### 3a. Test Case Design
1. **Identify Test Categories**:
   - **Positive Case**: Expected behavior after fix
   - **Negative Cases**: Edge cases and error conditions
   - **Boundary Cases**: Limits and extremes
   - **Regression Cases**: Feature interactions (from Stage 2 impact analysis)

2. **Write Failing Test Suite**
   ```csharp
   // test/Bugs/<BugID>/FixTests.cs
   
   public class BugFixTests_<ID>
   {
       // CATEGORY: Positive — Fix should restore expected behavior
       [Fact]
       public async Task SortOrderPreservedAfterFilter_SingleSort()
       {
           // GIVEN: Grid with single sort applied
           // WHEN: Filter is applied
           // THEN: Sort order remains unchanged
       }
       
       [Fact]
       public async Task SortOrderPreservedAfterFilter_MultiSort()
       {
           // GIVEN: Grid with multi-column sort (OrderID ASC, Amount DESC)
           // WHEN: Filter is applied to OrderDate
           // THEN: Multi-sort state preserved in exact order
       }
       
       // CATEGORY: Edge Cases
       [Fact]
       public async Task SortPreserved_EmptyFilterResult()
       {
           // GIVEN: Sort applied
           // WHEN: Filter results in empty dataset
           // THEN: Sort descriptor still active (for when data returns)
       }
       
       [Fact]
       public async Task SortPreserved_FilterCleared()
       {
           // GIVEN: Sort + Filter both applied
           // WHEN: Filter cleared (FilterSettings = null)
           // THEN: Sort still active, full dataset restored with sort
       }
       
       // CATEGORY: Regression — Cross-Feature
       [Theory]
       [InlineData(FilterType.FilterBar)]
       [InlineData(FilterType.FilterMenu)]
       [InlineData(FilterType.Excel)]
       public async Task SortPreserved_AcrossFilterTypes(FilterType type)
       {
           // GIVEN: Various filter UI types
           // WHEN: Any filter type applied with sort active
           // THEN: Sort state preserved
       }
       
       [Fact]
       public async Task SortPreserved_WithGrouping()
       {
           // GIVEN: Grid grouped by Category
           // AND: Sorted by OrderID
           // WHEN: Filter applied
           // THEN: Groups respect sort order, sort state preserved
       }
       
       [Fact]
       public async Task SortPreserved_WithVirtualization()
       {
           // GIVEN: 10K rows, virtualization enabled, sort applied
           // WHEN: Filter applied
           // THEN: Viewport updated, sort preserved, no memory spike
       }
       
       [Fact]
       public async Task SortPreserved_InEditMode()
       {
           // GIVEN: Grid in edit mode with sort active
           // WHEN: Filter applied
           // THEN: Edit context preserved, sort preserved
       }
       
       // CATEGORY: Boundary Conditions
       [Fact]
       public async Task SortPreserved_ClearSortThenFilter()
       {
           // GIVEN: Sort applied then cleared
           // WHEN: Filter applied
           // THEN: No sort applied (correct)
       }
   }
   ```

3. **Map Tests to Feature Skills**
   - For **Sort** tests: Reference `sorting-skill/SKILL.md`
   - For **Filter** tests: Reference `filtering-skill/SKILL.md`
   - For **Group** tests: Reference `grouping-skill/SKILL.md`
   - For **Virtualization** tests: Reference `virtualization-skill/SKILL.md`
   - Document each test with skill reference

#### 3b. Create Test Specification Document
   ```markdown
   # Test Specification for Bug <ID>
   
   ## Test Suite Structure
   - **Positive**: 2 tests
   - **Negative/Edge**: 4 tests
   - **Regression (Cross-Feature)**: 5 tests
   - **Boundary**: 1 test
   - **Total**: 12 tests (ALL MUST FAIL INITIALLY)
   
   ## Test Execution Plan
   1. Run full suite locally: `dotnet test --filter "Category=BugFix_<ID>"`
   2. All tests must FAIL (proving bug exists)
   3. After fix, all tests must PASS
   4. Cross-feature tests prevent regressions
   
   ## Feature Skill References
   | Test | Feature | Skill | Interaction |
   |------|---------|-------|-------------|
   | SortPreserved_SingleSort | Sorting | sorting-skill | Direct |
   | SortPreserved_WithFilter | Filtering | filtering-skill | Primary interaction |
   | SortPreserved_WithGrouping | Grouping | grouping-skill | Secondary effect |
   | SortPreserved_WithVirtualization | Virtualization | virtualization-skill | Critical interaction |
   | SortPreserved_InEditMode | Editing | editing-skill | Cross-domain |
   ```

#### 3c. C# Standards Checklist for Tests
```
TDD_TEST_DESIGN_CHECKLIST:
  ✓ Arrange-Act-Assert structure followed
  ✓ Each test is ATOMIC (one logical assertion)
  ✓ Test naming follows: TestName_Scenario_ExpectedOutcome
  ✓ Positive test cases documented
  ✓ Edge cases identified and tested
  ✓ Cross-feature scenarios included
  ✓ No hardcoded test data (use TestDataFactory)
  ✓ XML comments document test purpose and motivation
  ✓ Async/await patterns correct (no deadlocks, proper Task handling)
  ✓ Disposal cleanup for bUnit fixtures
```

### Gate: **Test Lead Approval Required**
```
TEST_DESIGN_APPROVAL:
  - [ ] All test cases written and currently FAILING
  - [ ] Positive, negative, edge, and regression cases covered
  - [ ] Cross-feature tests prevent known regressions
  - [ ] Feature skill references documented
  - [ ] C# standards checklist passed
  → APPROVE / REQUEST_MORE_TESTS / REVISE_SCOPE
```

**Proceed to Stage 4 only after APPROVE**

---

## Stage 4: Root Cause Fix Implementation

### Goal
Implement the bug fix with minimal scope, following C# standards and security practices.

### Agent Actions

#### 4a. Prepare Development Environment
1. Create bugfix branch: `bugfix/<BugID>-<brief-description>`
2. Verify tests still fail: `dotnet test --filter "Category=BugFix_<ID>" --no-build`
3. Document fix approach in `docs/requirements/bugs/<BugID>/fix-approach.md`

#### 4b. Security & Performance Pre-Flight Checks
1. **Security Analysis**:
   - Review if fix touches: MarkupString, user templates, eval, new JS interop calls
   - Check for: SQL injection patterns, XSS risks, privilege escalation
   - Verify: Input validation, output encoding applied
   - Document in fix-approach.md:
     ```markdown
     ## Security Impact
     - ✅ No MarkupString usage
     - ✅ No user code execution
     - ✅ No new JS interop calls (if applicable)
     - ✅ Input validation maintained
     ```

2. **Performance Pre-Analysis**:
   - Identify if change affects:
     - Render cycles (re-renders)
     - Memory allocations (large objects in loops)
     - Event subscription/disposal
   - Create baseline metrics:
     ```markdown
     ## Performance Baseline
     - Execution time for test case: ___ ms
     - Memory allocation (10K items): ___ MB
     - DOM node count: ___
     - JS interop calls: ___
     ```

#### 4c. Implementation
1. **Modify Only Identified Root Cause Files**
   - Strict scope: Only files identified in Stage 2 root cause analysis
   - Example for sort/filter interaction:
     - Modify: `SfGrid.Lifecycle.cs` (OnParametersSet method)
     - Modify: `Internal/Actions/Filter.cs` (filter update logic)
     - DO NOT: Modify other unrelated files

2. **Follow C# Standards**
   ```csharp
   // ✅ DO: Clear, well-named, follows component lifecycle
   private async Task SynchronizeSortStateOnFilterChange()
   {
       // Preserve existing sort descriptors
       var existingSortDescriptors = SortDescriptors?.ToList() ?? new List<SortDescriptor>();
       
       // Apply filter update
       await ApplyFilterSettings();
       
       // Restore sort descriptors
       SortDescriptors = existingSortDescriptors;
       
       // Trigger re-render with correct sort+filter state
       await InvokeAsync(StateHasChanged);
   }
   
   // ✅ DO: XML comments for public/protected
   /// <summary>
   /// Ensures sort state is preserved when filter settings change.
   /// Addresses Bug-<ID>: Sort order cleared after filter application.
   /// </summary>
   /// <remarks>
   /// Called from <see cref="OnParametersSet"/> to maintain cross-feature consistency.
   /// See <see cref="sorting-skill/SKILL.md"/> and <see cref="filtering-skill/SKILL.md"/>.
   /// </remarks>
   public async Task OnFilterParametersChanged()
   ```

3. **Memory & Disposal**
   - Check if new event listeners added → Ensure disposal in `Dispose()` / `DisposeAsync()`
   - Check if new object allocations → Verify they're not in render loops
   - Example disposal pattern:
     ```csharp
     private GridEvents _gridEvents;
     
     protected override async Task OnInitializedAsync()
     {
         _gridEvents = new GridEvents();
         _gridEvents.OnFilterApplied += HandleFilterChanged;
     }
     
     async ValueTask IAsyncDisposable.DisposeAsync()
     {
         if (_gridEvents != null)
         {
             _gridEvents.OnFilterApplied -= HandleFilterChanged;
             _gridEvents = null;
         }
         await base.DisposeAsync();
     }
     ```

#### 4d. Code Quality Gates
```
IMPLEMENTATION_QUALITY_CHECKLIST:
  ✓ Build succeeds: dotnet build -c Debug
  ✓ Zero analyzer warnings: dotnet build -warnaserror
  ✓ Scope verified: git diff shows only necessary files
  ✓ No dead code: All comments removed, unused code deleted
  ✓ XML comments added for changed public members
  ✓ Naming conventions followed (PascalCase for public, _camelCase for private)
  ✓ Async/await patterns correct
  ✓ Disposal cleanup verified (no resource leaks)
  ✓ No hardcoded magic numbers
  ✓ Security checklist completed
  ✓ Performance baseline documented
```

#### 4e. Run Initial Test Validation
```bash
dotnet test test/Bugs/<BugID>/FixTests.cs -c Debug --no-build
```
- At least 80% of tests should now PASS
- Document any failing tests and investigate

### Gate: **Self-Review & Code Quality Approval Required**
```
IMPLEMENTATION_APPROVAL:
  - [ ] All code quality checklist items passed
  - [ ] Build succeeds with zero warnings
  - [ ] Security analysis completed
  - [ ] Performance baseline established
  - [ ] 80%+ of TDD tests passing
  - [ ] No unintended file changes in diff
  → APPROVE / REQUEST_REVISIONS / STOP_AND_REASSESS
```

**Proceed to Stage 5 only after APPROVE**

---

## Stage 5: Comprehensive Testing & Regression Prevention

### Goal
Validate the fix against ALL related features to prevent regressions.

### Agent Actions

#### 5a. Execute Full Test Matrix

**Level 1: Bug-Specific Tests**
```bash
dotnet test test/Bugs/<BugID>/FixTests.cs -c Debug
# Assertion: 100% pass rate
```

**Level 2: Feature-Specific Tests** (from feature-impact-analysis)
```bash
# Test all affected features
dotnet test test/Features/Sorting/SortTests.cs -c Debug
dotnet test test/Features/Filtering/FilterTests.cs -c Debug
dotnet test test/Features/Grouping/GroupTests.cs -c Debug
dotnet test test/Features/Virtualization/VirtualTests.cs -c Debug
# Assertion: All pass, no new failures
```

**Level 3: Cross-Feature Interaction Tests**
```bash
# Test combinations identified in Stage 2
dotnet test test/CrossFeature/ -c Debug --filter "Category=Sort+Filter OR Category=Filter+Group OR Category=Group+Virtualization"
# Assertion: All pass, interaction matrix verified
```

**Level 4: Large Dataset Performance Tests**
```bash
# Run with 10K+ rows to verify virtualization + fix don't introduce memory leaks
dotnet test test/Performance/MemoryLeakTests.cs -c Debug
# Metrics captured: Memory growth, GC pressure, DOM mutation count
```

#### 5b. Memory Leak Detection
```csharp
[Fact]
public async Task BugFix_<ID>_NoMemoryLeakWith10KRows_SortFilterVirtualization()
{
    // Arrange
    var initialMemory = GC.GetTotalMemory(true);
    var grid = new SfGrid<OrderData> { EnableVirtualization = true };
    var largeData = GenerateTestData(10000);
    
    // Act: Apply sort
    await grid.SetPropertyAsync("SortSettings", ...);
    // Apply filter
    await grid.SetPropertyAsync("FilterSettings", ...);
    // Scroll through virtualized viewport
    await grid.ScrollToRow(5000);
    await grid.ScrollToRow(9999);
    // Clear filter
    await grid.SetPropertyAsync("FilterSettings", null);
    // Dispose
    await grid.DisposeAsync();
    
    // Assert: Memory growth within acceptable limits (< 10%)
    var finalMemory = GC.GetTotalMemory(true);
    var memoryGrowth = (finalMemory - initialMemory) / (double)initialMemory;
    Assert.True(memoryGrowth < 0.10, $"Memory leak detected: {memoryGrowth:P}");
}
```

#### 5c. Browser Compatibility (if UI involved)
```bash
# Run Playwright tests on multiple browsers
npx playwright test tests/e2e/BugFix_<ID>.spec.ts --project=chromium
npx playwright test tests/e2e/BugFix_<ID>.spec.ts --project=firefox
npx playwright test tests/e2e/BugFix_<ID>.spec.ts --project=webkit
# Assertion: Pass on all browsers
```

#### 5d. Regression Testing Report
```markdown
# Regression Testing Report — Bug <ID>

## Test Execution Summary
- **Bug-Specific Tests**: 12/12 ✅
- **Feature Tests (Sorting)**: 24/24 ✅
- **Feature Tests (Filtering)**: 28/28 ✅
- **Cross-Feature Tests**: 15/15 ✅
- **Memory Leak Tests**: 2/2 ✅
- **Browser Compatibility**: 3/3 ✅
- **Total**: 84/84 ✅

## Performance Metrics
| Metric | Before Fix | After Fix | Change | Status |
|--------|-----------|----------|--------|--------|
| Sort+Filter Time | 150ms | 152ms | +1.3% | ✅ Acceptable |
| Memory (10K rows) | 85MB | 86MB | +1.2% | ✅ No leak |
| GC Collections | 5 | 5 | 0% | ✅ Stable |
| DOM Mutations | 342 | 341 | -0.3% | ✅ Improved |

## Affected Features Cleared
- ✅ Sorting: All tests pass
- ✅ Filtering: All tests pass
- ✅ Grouping: Cross-feature tests pass
- ✅ Virtualization: No memory regression
- ✅ Editing: Cross-domain tests pass

## Risk Assessment
- **Risk Level**: LOW ← All regression tests cleared
- **Confidence**: HIGH ← Feature impact analysis comprehensive
- **Release Readiness**: APPROVED for code review
```

### Gate: **QA Team Approval Required**
```
QA_APPROVAL:
  - [ ] All bug-specific tests passing (100%)
  - [ ] Feature-specific tests passing (no new failures)
  - [ ] Cross-feature tests passing (regression matrix clear)
  - [ ] Memory leak tests passing (no leaks detected)
  - [ ] Performance metrics acceptable (< 5% regression)
  - [ ] Browser compatibility verified (if applicable)
  → APPROVE_FOR_REVIEW / REQUEST_MORE_TESTING / REJECT
```

**Proceed to Stage 6 only after APPROVE**

---

## Stage 6: Code Review & Approval

### Goal
Ensure correctness, architectural compliance, and readiness for merge.

### Review Checklist
- [ ] Root cause actually addressed (not symptoms)
- [ ] No performance regression detected
- [ ] No memory leaks introduced
- [ ] Accessibility features preserved (keyboard nav, ARIA)
- [ ] Code follows C# standards
- [ ] Security practices applied
- [ ] Cross-feature impact verified
- [ ] All tests passing
- [ ] Documentation updated

### Reviewer Approval Gates
```
CODE_REVIEWER_APPROVAL:
  - [ ] Root cause addressed with evidence from Stage 2
  - [ ] TDD approach followed (tests written first)
  - [ ] No performance regression detected
  - [ ] No memory leaks introduced
  - [ ] Accessibility features preserved
  - [ ] C# standards and security practices applied
  - [ ] Feature impact analysis confirms no new regressions
  - [ ] All regression tests passing
  → APPROVE / REQUEST_REVISIONS / REJECT
```

---

## Stage 7: Merge & Documentation

### Goal
Integrate fix into main codebase and document the resolution.

### Actions
1. **Merge to develop**: Squash-merge with description
2. **Create Release Tag**: `v32.x.x`
3. **Update Documentation**:
   - `CHANGELOG.md`: Add bug fix entry
   - `docs/requirements/bugs/<BugID>/RESOLUTION.md`: Document fix
   - Feature skill docs: Update with any new interaction notes

4. **Close Tickets**: BoldDesk, Azure DevOps with reference to merged PR

---

## Agent Configuration & Skills Integration

### Skills to Reference & Load

The Bug-Fix-Resolver Agent MUST reference the following skills:

```yaml
Required_Skills:
  
  # Feature Domain Skills (dynamically loaded by feature-impact-analysis)
  - sorting-skill/SKILL.md          # If sort-related
  - filtering-skill/SKILL.md        # If filter-related
  - grouping-skill/SKILL.md         # If group-related
  - editing-skill/SKILL.md          # If edit-related
  - virtualization-skill/SKILL.md   # If virtualization involved
  - paging-skill/SKILL.md           # If paging involved
  - infinite-scroll-skill/SKILL.md  # If infinite scroll involved
  
  # Cross-Cutting Skills (always loaded)
  - feature-impact-analysis-skill/SKILL.md    # Stage 2 root cause analysis
  - testing/SKILL.md                          # Stage 3 TDD design & Stage 5 testing
  - javascript-interop/SKILL.md               # If JS interop involved
  - accessibility-requirements/SKILL.md       # Stage 5 regression testing
  - performance-optimization/SKILL.md         # Stage 4 & 5 performance checks
  - blazor-lifecycle/SKILL.md                 # Stage 2 & 4 lifecycle understanding
  - blazor-framework/SKILL.md                 # General Blazor patterns
```

### Skill Invocation Flow

```
Stage 1: Bug Intake
  → Load: (none — human confirmation only)

Stage 2: Root Cause Analysis
  → Load: feature-impact-analysis-skill/SKILL.md
  → Read: domain_map from openspec/config.yaml
  → Load: Specific feature skills (sorting, filtering, etc.)
  → Read: openspec/specs/<domain>/spec.md for each affected feature

Stage 3: TDD Design
  → Load: testing/SKILL.md
  → Reference: Feature skills from Stage 2
  → Reference: Lifecycle/Framework skills for test context

Stage 4: Implementation
  → Load: blazor-lifecycle/SKILL.md (OnParametersSet hooks)
  → Load: performance-optimization/SKILL.md (memory management)
  → Load: javascript-interop/SKILL.md (if JS changes)
  → Load: accessibility-requirements/SKILL.md (preserve keyboard nav)

Stage 5: Testing
  → Load: testing/SKILL.md (test execution)
  → Load: performance-optimization/SKILL.md (memory leak detection)
  → Load: Feature skills (cross-feature verification)

Stage 6: Code Review
  → Reference: All skills applied in Stages 2-5
  → Cross-check: Compliance with standards from each skill

Stage 7: Merge & Documentation
  → Update: Feature skill docs with new interaction patterns discovered
  → Update: openspec/specs/lessons-learned.md with this bug fix experience
```

---

## Example: Bug-Fix-Resolver Workflow for Sort+Filter Bug

### Scenario
Bug #1247: "Sort order cleared when filter applied"

### Workflow Execution

```
STAGE 1: BUG INTAKE ✅ HUMAN APPROVAL
├─ Parse: "After applying sort OrderID ASC, then applying filter, sort is lost"
├─ Classify: Feature=Sorting+Filtering, Severity=HIGH
├─ Request: Human confirmation
└─ Result: APPROVED → Continue to Stage 2

STAGE 2: ROOT CAUSE ANALYSIS ✅ ARCHITECT APPROVAL
├─ Reproduction: Write failing BUnit test (test fails ✅)
├─ Feature Mapping: GridSortSettings + GridFilterSettings
├─ Load: feature-impact-analysis-skill
├─ Load: sorting-skill/SKILL.md, filtering-skill/SKILL.md
├─ Load: openspec/specs/sorting/spec.md, filtering/spec.md
├─ Scan: GridSortSettings.razor.cs, GridFilterSettings.razor.cs
├─ Scan: SfGrid.Lifecycle.cs OnParametersSet()
├─ Find: Line 105 in OnParametersSet clears sort descriptors on filter change
├─ Impact Analysis: Affects Sort, Filter, Group, Virtualization
├─ Document: ANALYSIS.md with root cause + cross-feature matrix
└─ Result: ROOT CAUSE IDENTIFIED (HIGH confidence) → Architect approves

STAGE 3: TDD DESIGN ✅ TEST LEAD APPROVAL
├─ Write 12 test cases (all currently failing):
│  ├─ 2 positive tests
│  ├─ 4 edge cases
│  ├─ 5 regression (Sort+Filter, Sort+Group, Sort+Virtualization, etc.)
│  └─ 1 boundary case
├─ Map tests to skills: sorting-skill, filtering-skill, grouping-skill, virtualization-skill
├─ Verify: All tests fail (bug confirmed)
└─ Result: APPROVED by Test Lead → Continue to Stage 4

STAGE 4: IMPLEMENTATION ✅ SELF-REVIEW APPROVAL
├─ Branch: bugfix/1247-sort-preserved-after-filter
├─ Security: No MarkupString, no eval, no JS interop added → ✅
├─ Performance: Baseline established (sort+filter time: 150ms)
├─ Modify: SfGrid.Lifecycle.cs OnParametersSet() (preserve sort state)
├─ Code Review: Zero analyzer warnings ✅
├─ Memory: Disposal paths verified, no new leaks ✅
└─ Result: SELF-REVIEW APPROVED → Continue to Stage 5

STAGE 5: TESTING ✅ QA APPROVAL
├─ Run bug-specific tests: 12/12 ✅ PASS
├─ Run feature tests:
│  ├─ Sorting: 24/24 ✅ PASS
│  ├─ Filtering: 28/28 ✅ PASS
│  ├─ Grouping: 18/18 ✅ PASS
│  └─ Virtualization: 15/15 ✅ PASS
├─ Cross-feature tests: 20/20 ✅ PASS
├─ Memory leak tests: 2/2 ✅ PASS (no leaks detected)
├─ Performance: Sort+Filter time 152ms (+1.3%) → ✅ ACCEPTABLE
└─ Result: ALL TESTS PASS → QA APPROVAL GRANTED

STAGE 6: CODE REVIEW ✅ HUMAN REVIEWER APPROVAL
├─ Reviewer checks:
│  ├─ Root cause addressed ✅
│  ├─ No performance regression ✅
│  ├─ No memory leaks ✅
│  ├─ Accessibility preserved ✅
│  ├─ C# standards followed ✅
│  └─ Cross-feature impact verified ✅
└─ Result: APPROVED → Continue to Stage 7

STAGE 7: MERGE & RELEASE ✅
├─ Squash merge: develop (commit message references bug + skill insights)
├─ Tag: v32.4.1
├─ Update: CHANGELOG.md
├─ Update: docs/requirements/bugs/1247/RESOLUTION.md
├─ Update: openspec/specs/lessons-learned.md (document this fix workflow)
└─ Result: ✅ BUG FIXED & DEPLOYED
```

---

## Key Success Metrics

| Metric | Target | Assessment |
|--------|--------|-----------|
| **Root Cause Accuracy** | 100% (line-level precision) | Line numbers in ANALYSIS.md |
| **Test Coverage** | ≥85% (new code) | dotnet test with /p:CollectCoverage=true |
| **TDD Adherence** | 100% (tests written before fix) | All TDD tests fail before fix, pass after |
| **Cross-Feature Regression** | 0 new failures | All feature test suites pass post-fix |
| **Memory Leaks** | 0 detected | GC metrics stable in performance tests |
| **Performance Regression** | <5% tolerance | Baseline vs. post-fix metrics |
| **C# Standard Compliance** | 0 analyzer warnings | Build output: "Warnings: 0" |
| **Security Posture** | No new vulnerabilities | Security checklist completed |
| **Human Review Time** | <2 hours | From PR creation to merge |

---

## Summary

The **Bug-Fix-Resolver Agent** provides a **human-governed, systematic, regression-safe** approach to bug resolution by:

1. ✅ **Securing human confirmation** at critical gates (intake, root cause, test design, implementation, review)
2. ✅ **Using TDD discipline** — tests written BEFORE code (failing tests = proof of bug)
3. ✅ **Applying feature-impact-analysis** to map cross-feature regression risks
4. ✅ **Following C# standards** with zero-analyzer-warning enforcement
5. ✅ **Ensuring security** through explicit security checklist
6. ✅ **Verifying performance** with baseline metrics and memory leak detection
7. ✅ **Documenting everything** for future learning and compliance

This workflow aligns with the organization's 7-phase development lifecycle and OpenSpec architecture, ensuring every bug fix is thoroughly vetted, well-tested, and regression-safe.
