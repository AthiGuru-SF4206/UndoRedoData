# Training Index — Syncfusion Blazor DataGrid

> **Purpose**: Complete navigation map for all training modules  
> **Last Updated**: March 12, 2026

---

## Complete Module Index

### Entry Point

| File | Description | Prerequisites |
|------|-------------|--------------|
| [`00-START-HERE.md`](./00-START-HERE.md) | Welcome, quick start, navigation | None |

---

### Module 01 — Getting Started

**Goal**: Understand the component architecture and set up a working development environment.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`01-getting-started/architecture-overview.md`](./01-getting-started/architecture-overview.md) | Understand 4-layer architecture, module injection, rendering pipeline, JS-interop role | 60 min |
| [`01-getting-started/project-setup-guide.md`](./01-getting-started/project-setup-guide.md) | Install prerequisites, clone repo, build and run the component, IDE configuration | 60–90 min |

**Prerequisites**: [`../overview/product-overview.md`](../overview/product-overview.md)

---

### Module 02 — Requirements Analysis

**Goal**: Learn how to read, decompose, and validate feature and bug requirements before writing code.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`02-requirements-analysis/understanding-requirements.md`](./02-requirements-analysis/understanding-requirements.md) | Interpret user stories, write acceptance criteria, identify edge cases, decompose backlog items | 60 min |

**Prerequisites**: Module 01

---

### Module 03 — LLM Best Practices

**Goal**: Work effectively with AI agents (Claude, GPT, Gemini) in a Syncfusion development context.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`03-llm-best-practices/working-with-llms.md`](./03-llm-best-practices/working-with-llms.md) | Write precise prompts, scope sub-agent tasks, validate LLM outputs, avoid hallucinations | 60 min |

**Prerequisites**: Module 02

---

### Module 04 — Code Processing

**Goal**: Apply optimal chunking strategies to process large Blazor source files within LLM context windows.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`04-code-processing/optimal-chunking-strategies.md`](./04-code-processing/optimal-chunking-strategies.md) | Identify semantic chunks, split by feature boundary, manage token budgets, maintain context coherence | 45 min |

**Prerequisites**: Module 03

---

### Module 05 — Practical Examples

**Goal**: Walk through a complete feature implementation from requirement to PR.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`05-practical-examples/feature-implementation-walkthrough.md`](./05-practical-examples/feature-implementation-walkthrough.md) | End-to-end feature delivery: spec → architecture decision → implementation → test → PR | 2–3 hours |

**Prerequisites**: Modules 01–04

---

### Module 06 — Reference

**Goal**: Provide quick lookup guides for daily development work.

| File | Learning Objectives | Estimated Time |
|------|--------------------|----|
| [`06-reference/quick-reference-guides.md`](./06-reference/quick-reference-guides.md) | API checklist, naming tables, PR checklist, regression checklist, agent request templates | Reference |

**Prerequisites**: None (use anytime)

---

### Completion

| File | Purpose |
|------|---------|
| [`DELIVERY-SUMMARY.md`](./DELIVERY-SUMMARY.md) | Training completion checklist and sign-off |

---

## Learning Objectives Summary

After completing all modules, a developer will be able to:

| Skill | Covered In |
|-------|-----------|
| Understand 4-layer architecture | Module 01 |
| Set up development environment | Module 01 |
| Navigate 14 action modules | Module 01 |
| Decompose requirements | Module 02 |
| Write acceptance criteria | Module 02 |
| Identify regression-sensitive areas | Module 02 |
| Prompt LLMs effectively | Module 03 |
| Validate AI-generated code | Module 03 |
| Chunk large source files | Module 04 |
| Manage token budgets | Module 04 |
| Implement a feature end-to-end | Module 05 |
| Write a compliant PR | Module 05 |
| Use daily reference guides | Module 06 |

---

## Dependency Graph

```
00-START-HERE
     │
     ▼
01-getting-started ──────────────────┐
     │                               │
     ▼                               ▼
02-requirements-analysis    06-reference (use anytime)
     │
     ▼
03-llm-best-practices
     │
     ▼
04-code-processing
     │
     ▼
05-practical-examples
     │
     ▼
DELIVERY-SUMMARY
```

---

*For full architecture depth, continue to [`../architecture/system-architecture.md`](../architecture/system-architecture.md).*
