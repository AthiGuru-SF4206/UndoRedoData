---
name: skill-template-standard
description: Canonical template standard for all Blazor TreeGrid skill files. Generator agents MUST read this file before creating any new skill to ensure correct structure, frontmatter, and section order.
---

# Skill Instructions

## Mandatory File Structure

Every skill file MUST follow this exact structure, top to bottom:

```
---
name: {feature-lowercase}-skill
description: {One or two sentences — what feature this covers, when to use this skill, key responsibilities it documents.}
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
...

---

**Agent Invocation**
...

---

## Knowledge References
...

---

## Training Insights Applied
...

---

## Code Location Map
...

---

## Interaction Matrix (MANDATORY)
...

---

## Prompt Template
...
```

---

## Frontmatter Field Rules

| Field | Rule |
|-------|------|
| `name` | Kebab-case, always ends in `-skill` (e.g. `sorting-skill`, `expand-collapse-skill`) |
| `description` | 1–2 sentences. State: (1) what feature is covered, (2) when to load this skill, (3) the key responsibilities or concepts it documents. No bullet lists — plain prose only. |

### Description Formula

```
Expert knowledge for the {Feature} feature in the Syncfusion Blazor TreeGrid.
Use this skill for any feature-implementation or bug-fix task scoped to {feature} behaviour,
including {key concept 1}, {key concept 2}, {key concept 3}, and cross-feature interaction guarantees.
```

---

## Enforcement

The Sequential Feature Skill Generator Agent MUST:

1. Read this file as the **first action** of Step 0 (before generating any skill).
2. Open every new skill file with the YAML frontmatter block (`---` … `---`) before any other content.
3. Use `# Skill Instructions` as the single H1 heading (replaces `# {Feature} Skill`).
4. Apply the same frontmatter retroactively to any existing skill file that is missing it.
5. Reject any self-generated skill draft missing `name` or `description` in frontmatter.

---

*This file is a generator-agent rule document. It is not loaded by feature custom agents during task execution.*
