# Branching Strategy

## Overview

This document defines the Git Flow branching strategy for the Syncfusion Blazor Grid component. It establishes consistent branch management, commit conventions, and release tagging practices to ensure a stable development lifecycle.

---

## Branch Types

| Branch Type | Naming Pattern | Purpose |
|-------------|---------------|---------|
| `main` | `main` | Production-ready, always deployable |
| `develop` | `develop` | Integration branch for all features and fixes |
| `feature/*` | `feature/description` | New features or enhancements |
| `bugfix/*` | `bugfix/description` | Bug fixes targeting the develop branch |
| `hotfix/*` | `hotfix/description` | Urgent fixes targeting production (main) |
| `docs/*` | `docs/description` | Documentation updates only |

---

## Git Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        GIT FLOW BRANCHING STRATEGY                          │
└─────────────────────────────────────────────────────────────────────────────┘

                    ╔═════════════════════════════════════╗
                    ║        DEVELOP BRANCH               ║
                    ║   (Development/Integration)         ║
                    ║  • Merge: Features, fixes, chores   ║
                    ║  • Deploy: To staging/beta          ║
                    ║  • Status: Pre-release code         ║
                    ╚═════════════════════════════════════╝
                         │              │              │
            ┌────────────┴──────┐       │      ┌───────┴────────────┐
            │                   │       │      │                    │
            ▼                   ▼       │      ▼                    ▼
        ┌────────────┐     ┌────────┐   │  ┌──────────┐         ┌──────────┐
        │ feature/*  │     │bugfix/*│   │  │ hotfix/* │         │  docs/*  │
        │(New Feat.) │     │ (Bugs) │   │  │ (Urgent) │         │(Docs)    │
        └────────────┘     └────────┘   │  └──────────┘         └──────────┘
            │                   │       │      │                    │
            └────────────┬──────┘       │      └────────┬──────────┘
                         │              │               │
                         └──────────────┼───────────────┘
                                        ▼
                    ╔═════════════════════════════════════╗
                    ║        MAIN BRANCH                  ║
                    ║   (Production-ready code)           ║
                    ║  • Merge: Only via PR approval      ║
                    ║  • Deploy: Automatically to prod    ║
                    ║  • Tag: v{MAJOR}.{MINOR}.{PATCH}    ║
                    ╚═════════════════════════════════════╝
                                        │
                         ┌──────────────┴────────────┐
                         │                           │
                         ▼                           ▼
                    ╔──────────────┐      ╔──────────────────╗
                    ║  Production  ║      ║  Release Tags    ║
                    ║  Deployment  ║      ║  v32.1.19        ║
                    ║              ║      ║  v32.1.20 (Patch)║
                    ║              ║      ║  v32.1.21 (Patch)║
                    ╚──────────────╝      ╚──────────────────╝
```

---

## Branch Flow Summary

```
FEATURE DEVELOPMENT:
feature/* → (PR) → DEVELOP → (Release) → MAIN → (Tag) → Production
                                                   ↓ v32.1.19

BUG FIXES (Development):
bugfix/* → (PR) → DEVELOP → (Release) → MAIN → (Tag) → Production
                                                  ↓ v32.1.19

PRODUCTION HOTFIX (Patch Release):
hotfix/* → (PR) → MAIN → (Tag) → Production Patch
        └─ (PR) → DEVELOP → (Keep in sync)
                      ↓ v32.1.20
```

---

## Branch Naming Conventions

### Feature Branches
```
feature/add-freeze-columns
feature/row-virtualization-improvements
feature/column-chooser-search
feature/adaptive-ui-support
```

### Bug Fix Branches
```
bugfix/fix-flickering-addnew-row
bugfix/fix-tab-key-after-grouping
bugfix/fix-filter-row-focus-loss
bugfix/fix-virtual-scroll-memory-leak
```

### Hotfix Branches
```
hotfix/fix-critical-memory-leak
hotfix/fix-data-loss-on-delete
hotfix/fix-security-injection-vulnerability
```

### Documentation Branches
```
docs/update-api-reference
docs/add-virtualization-guide
docs/improve-getting-started
```

---

## Branch Protection Rules

### `main` Branch
- ✅ Require pull request before merging
- ✅ Require at least 2 approvals
- ✅ Require status checks to pass (build, unit tests, accessibility)
- ✅ Require branches to be up to date before merging
- ❌ No direct push allowed
- ❌ No force push allowed
- ❌ No branch deletion allowed

### `develop` Branch
- ✅ Require pull request before merging
- ✅ Require at least 1 approval
- ✅ Require status checks to pass (build, unit tests)
- ✅ Require branches to be up to date before merging
- ❌ No direct push allowed
- ❌ No force push allowed

---

## Commit Message Conventions

### Format
```
<type>(<scope>): <subject>

[optional body]

[optional footer(s)]
```

### Types
| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat(grid): add row drag-and-drop support` |
| `fix` | Bug fix | `fix(grid): resolve flickering on add-new-row delete` |
| `perf` | Performance improvement | `perf(virtualization): reduce DOM node creation by 40%` |
| `refactor` | Code restructure (no feature/bug) | `refactor(selection): extract selection state manager` |
| `test` | Test additions/changes | `test(filter): add BUnit tests for multi-column filter` |
| `docs` | Documentation only | `docs(api): update GridColumn XML comments` |
| `chore` | Build/tooling/config changes | `chore(deps): upgrade Syncfusion.Blazor to v32.1.20` |
| `style` | Code formatting (no logic change) | `style(grid): apply consistent spacing in SfGrid.razor.cs` |
| `revert` | Revert prior commit | `revert: feat(grid): add row drag-and-drop support` |

### Scope Values (Grid Component)
- `grid` — Core SfGrid component
- `column` — Column features
- `selection` — Selection module
- `filter` — Filtering module
- `sort` — Sorting module
- `group` — Grouping module
- `edit` — Edit/CRUD operations
- `virtualization` — Virtual scrolling
- `export` — PDF/Excel export
- `aggregate` — Aggregate rows
- `pager` — Pagination
- `toolbar` — Toolbar module
- `accessibility` — Keyboard/ARIA features
- `interop` — JS-interop layer
- `renderer` — Rendering engine

### Commit Examples
```bash
feat(column): add support for frozen right columns

fix(virtualization): prevent flickering when deleting add-new-row record
Resolves: #1015142

perf(renderer): batch DOM updates to reduce reflow cycles

test(edit): add BUnit tests for inline editing with validation

docs(interop): document JS-interop lifecycle in component-architecture.md
```

---

## Workflow Steps

### 1. Feature Development
```bash
# Create feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/add-freeze-columns

# Work and commit
git add .
git commit -m "feat(column): implement freeze-column state management"

# Push and create PR to develop
git push origin feature/add-freeze-columns
#  Create PR: feature/add-freeze-columns  develop
```

### 2. Bug Fix (Development)
```bash
# Create bugfix branch from develop
git checkout develop
git pull origin develop
git checkout -b bugfix/fix-tab-after-grouping

# Fix and commit
git add .
git commit -m "fix(group): resolve tab key script error after grouping

Tab key caused unhandled exception in focus manager when
grouped rows were rendered. FocusService now checks for
grouped context before invoking cell focus.
Resolves: #1015142"

# Push and create PR to develop
git push origin bugfix/fix-tab-after-grouping
#  Create PR: bugfix/fix-tab-after-grouping  develop
```

### 3. Production Hotfix
```bash
# Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/fix-critical-memory-leak

# Fix and commit
git add .
git commit -m "fix(virtualization): dispose scroll observer on component teardown

JS ResizeObserver was not disposed when component unmounted,
causing memory leak in long-running Blazor Server sessions."

# PR to main (production patch)
git push origin hotfix/fix-critical-memory-leak
#  Create PR: hotfix/fix-critical-memory-leak  main
#  After merge: Tag v32.1.20

# Sync back to develop
#  Create PR: hotfix/fix-critical-memory-leak  develop
```

### 4. Release to Production
```bash
# PR from develop to main
#  Create PR: develop  main
#  After approval and merge:
git checkout main
git pull origin main
git tag -a v32.1.19 -m "Release v32.1.19: freeze columns, accessibility improvements"
git push origin v32.1.19
#  Automated CI/CD deploys to NuGet and production
```

---

## Release Tagging Convention

### Semantic Versioning: `v{MAJOR}.{MINOR}.{PATCH}`

| Release Type | Tag Pattern | Trigger | Example |
|-------------|-------------|---------|---------|
| **Major** | `v20.0.0` | Breaking changes, new major features | `v20.0.0` |
| **Minor** | `v20.1.0` | New features, no breaking changes | `v32.1.0` |
| **Patch** | `v20.1.1` | Hotfix for production issue | `v32.1.20` |

### Tag Message Format
```
Release v32.1.19: <one-line summary>

Features:
- feat(column): freeze right columns support
- feat(selection): checkbox selection for virtual rows

Bug Fixes:
- fix(group): tab key script error after grouping (#1015142)
- fix(edit): add-new-row flicker on delete under virtualization

Performance:
- perf(renderer): reduced initial render time by 15%
```

---

## CI/CD Integration Points

### On Push to `feature/*` / `bugfix/*`
1.  Build validation (dotnet build)
2.  BUnit test execution
3.  Code analysis (Roslyn analyzers)
4.  XML comment validation

### On PR to `develop`
1.  All push checks
2.  Accessibility checks (axe-core)
3.  Bundle size check
4.  Playwright E2E smoke tests
5.  Code review approval gate (min 1 reviewer)

### On PR to `main`
1.  All develop PR checks
2.  Full Playwright E2E regression suite
3.  Performance benchmark comparison
4.  Memory leak detection
5.  Cross-platform verification (Blazor Server + WASM)
6.  Code review approval gate (min 2 reviewers)
7.  Scrum Master sign-off

### On Merge to `main`
1.  Auto-tag with version number
2.  NuGet package publish
3.  Release notes generation
4.  Production deployment

---

## Example Workflow Diagram

```
Day 1: Feature Start
  develop 
               
                feature/add-freeze-columns

Day 3: Commits
  develop 
               feature/add-freeze-columns  commit1  commit2

Day 5: PR Opened
  develop 
               feature/add-freeze-columns  commit1  commit2
                                                               
                                                        
                                                          PR Review  
                                                          CI/CD Pass 
                                                        
                                                                Approved

Day 6: Merged to Develop
  develop  ...  merge(feature/add-freeze-columns) 
  
Release Day: Develop  Main
  develop 
                                                             
                                                    PR to main
                                                              Approved 2
                                                             
  main  merge
                                                             
                                                         Tag v32.1.19
                                                             
                                                    NuGet Publish
```

---

## Stale Branch Policy

| Branch Type | Stale Threshold | Action |
|------------|----------------|--------|
| `feature/*` | 30 days no activity | Warning notification |
| `feature/*` | 60 days no activity | Auto-close PR, archive branch |
| `bugfix/*` | 14 days no activity | Warning notification |
| `bugfix/*` | 30 days no activity | Auto-close PR, archive branch |
| `hotfix/*` | 7 days no activity | Escalate to team lead |
| `docs/*` | 30 days no activity | Warning notification |

---

## Related Documents

- [Development Workflow](./development-workflow.md)  7-phase development lifecycle
- [PR Guidelines](./pr-guidelines.md)  PR template and review checklist
- [Coding Standards](../code-guidelines/coding-standards.md)  Code quality rules
- [Agents Overview](../ai-agents/agents-overview.md)  AI agent collaboration
