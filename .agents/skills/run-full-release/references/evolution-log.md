# Evolution Log

> **Convention**: Reverse chronological order (newest on top, oldest at bottom). Prepend new entries.

---

## 2026-06-02: `.gitignore`-in-WIP Stash Trap (curve-dental v2.9.4) <!-- # SSoT-OK -->

**Trigger**: During a curve-dental release the working tree was dirty with mixed WIP. Choosing "stash WIP → release → restore" and running `git stash -u` reverted an uncommitted `.gitignore` whose only changes were ignore rules for local-only PII dirs (`correspondence/`, `site/`, `cdanet/`, `.wrangler/`, `.mise.local.toml`). Reverting `.gitignore` un-ignored those dirs, which then surfaced as `??` untracked — failing `release:preflight`'s clean-tree gate AND momentarily exposing clinical PII to accidental staging.

### What Changed

- **Added Known Issue** (Step 2d): "`git stash -u` of WIP `.gitignore` re-exposes ignored dirs and fails preflight (PII hazard)" — root cause is that an uncommitted `.gitignore` is load-bearing infrastructure, not feature WIP; fix is to commit the `.gitignore` hygiene (plus any tracked companion it references) as its own `chore:`, then pathspec-stash only the feature code.
- **Strengthened Step 3 dirty-tree guidance** to flag `.gitignore` changes as commit-not-stash and to prefer `git stash push -- <files>` over `-u`.

### Why It Changed

The generic "stash WIP first" advice has a sharp edge in repos where `.gitignore` itself is part of the uncommitted change set — common when ignore rules for newly-created local-only/PII dirs haven't been committed yet. Blindly stashing inverts the ignore state. In a clinical repo this is a privacy hazard, not just a preflight annoyance.

### Files Affected

- `SKILL.md`: +1 Known Issue paragraph (Step 2d); +1 caution in Step 3 dirty-tree guidance
- `references/evolution-log.md`: This entry

### Evidence

curve-dental v2.9.4 shipped clean after the corrected sequence: pop the over-broad stash → commit `.gitignore` + the secret-free `scripts/publish-brief.sh` it references as one `chore:` → `git stash push -- form_audit.py chart_template.py test_schema.py` (the real feature WIP) → `mise run release:full` (preflight green; tag + GitHub release + JSONL asset) → `git stash pop`. Release: <https://github.com/459ecs/curve-dental/releases/tag/v2.9.4>

---

## 2026-03-31: Consolidate into Single Self-Contained SKILL.md

**Trigger**: Skill was too big and brittle — two reference files (scaffolding-and-recovery.md at 177 lines, task-implementations.md at 173 lines) tried to cover every ecosystem with hardcoded templates. The skill was also conflating two jobs: running releases (trivial) and bootstrapping release workflows (the actual value).

### What Changed

- **Deleted** `references/scaffolding-and-recovery.md` and `references/task-implementations.md`
- **Rewrote SKILL.md** as a self-contained guide (98 lines, down from 55 + 350 in references)
- **Restructured** into 3 clear steps: Detect → Bootstrap → Execute
- **Bootstrap is now a guide**, not a template dump — audit the repo first, scaffold only what fits
- **Known issues and recovery** consolidated into compact 2d section (was 80+ lines across two files)
- **Description** updated to natural language "Use when..." pattern per skill-architecture guidance

### Why It Changed

The reference files were not robust for diverse repos because they assumed fixed ecosystem patterns. A Python-only repo doesn't need crates.io templates. A repo with existing Makefile releases doesn't need full semantic-release scaffolding. The new approach: audit first, then scaffold what the specific repo actually needs.

---

## 2026-03-09: Production Learnings from opendeviationbar-py Release <!-- # SSoT-OK -->

**Status**: Major update — 3 new sections added from real-world release failure.

### What Changed

- Added **Step 0: Pre-Release Sync** — mandatory `git pull origin main` before any release
- Added **Known Issue: `@semantic-release/git` Untracked File Explosion** — `git ls-files -m -o` missing `--exclude-standard` crashes plugin with ~100MB stdout in repos with `.venv/`
- Added **Partial Semantic-Release Recovery** — manual tag creation when semantic-release partially succeeds
- Added **Post-Release Deploy Reminder** — prevents version drift on production hosts
- Added 3 new error recovery rows for semantic-release-specific failures

### Why It Changed

During the opendeviationbar-py v13.2.0 release:

1. `@semantic-release/git` v10.0.1 crashed listing 100K+ gitignored `.venv/` files (upstream bugs: #345, #347, #107)
2. Semantic-release partially ran (bumped Cargo.toml + CHANGELOG.md) but failed before creating tag — required manual recovery
3. Forgot to pull remote changes before release, causing diverged branch confusion
4. Forgot to deploy to bigblack after PyPI publish, causing 30+ minutes of version drift alerts

### Files Affected

- `SKILL.md`: +80 lines (Step 0, Known Issue, Partial Recovery, Deploy Reminder, error table rows)
- `references/evolution-log.md`: This entry

---

## 2026-02-26: Initial Evolution Log

**Status**: Skill is in use and maintained. Track improvements here.

### Purpose

This evolution log tracks updates to the skill. Each entry should note:

- What changed (content, structure, tooling)
- Why it changed (bug fix, feature request, best practice)
- Files affected

### How to Use

1. When updating SKILL.md or references, add an entry here with the date
2. Keep entries reverse-chronological (newest first)
3. Link to ADRs or GitHub issues when relevant
4. Reference specific line changes when helpful

---
