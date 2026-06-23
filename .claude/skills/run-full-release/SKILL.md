---
name: run-full-release
description: Run the current repo's mise release pipeline, or bootstrap one if missing. Use when user wants to release, version bump, publish a package, or.
allowed-tools: Read, Bash, Glob, Grep, Write, Edit, AskUserQuestion
argument-hint: "[--dry] [--status]"
---

# /mise:run-full-release

Run the current repo's mise release pipeline — or bootstrap one if it doesn't exist yet.

> **Self-Evolving Skill**: This skill improves through use. If instructions are wrong, parameters drifted, or a workaround was needed — fix this file immediately, don't defer. Only update for real, reproducible issues.

## Step 1: Detect Existing Release Tasks

```bash
mise tasks ls 2>/dev/null | grep -i release
```

**If tasks exist** → skip to [Step 3: Execute](#step-3-execute).
**If tasks NOT found** → continue to Step 1b.

## Step 1b: Classify the Repo Before Scaffolding

Not every repo is a publishable package. Before bootstrapping a versioning/publish pipeline, decide what kind of repo this is:

```bash
ls pyproject.toml Cargo.toml package.json setup.py go.mod 2>/dev/null || echo "NO_PACKAGE_MANIFEST"
```

- **Has a package manifest** (Python/Rust/Node/Go) → publishable repo. Continue to **Step 2** (scaffold a version+publish pipeline).
- **NO package manifest AND no existing release infra** → this is a **docs / config / sync repo** (personal dotfiles, notes, or a multi-machine `git`-synced config repo). There is nothing to version or publish. **Do NOT scaffold `semantic-release`.** For these repos, "release" = **sync to origin**:

  ```bash
  git fetch origin
  git rev-list --left-right --count origin/<branch>...HEAD   # behind  ahead
  [ -z "$(git status --porcelain)" ] || echo "DIRTY — commit or stash first"
  # Only when the user has asked to release/ship (push only on request):
  git push origin <branch>
  git fetch origin && [ "$(git rev-parse HEAD)" = "$(git rev-parse origin/<branch>)" ] && echo "✅ synced — release complete"
  ```

  If `behind > 0`, reconcile first (`git pull --rebase` or merge) before pushing. Optionally scaffold a minimal `release:full` = `preflight` (clean tree + correct branch + up-to-date) → `push` → `verify` (origin == HEAD), with NO version/publish phases — but only if the user wants repeatable sync; otherwise the one-shot push above IS the whole release.

  _Evidence (2026-06-04): invoked on `claude-sys`, a multi-machine git-synced config/docs repo — no package manifest, no `.mise.toml`, no semantic-release. Correct action was a fast-forward `git push origin main` + HEAD==origin verify, NOT a pipeline scaffold. Audit-first (Step 2a's ecosystem check) is what surfaced this; promoting it to an explicit Step 1b gate prevents the wrong reflex of scaffolding a publish pipeline on a repo with nothing to publish._

## Step 2: Bootstrap Release Workflow

This step scaffolds an individualized release pipeline for THIS repo. Every repo is different — do not copy templates verbatim. Audit first, then scaffold what fits.

### 2a. Audit the Repository

Run all of these to understand what this repo needs:

```bash
# Ecosystem detection
ls pyproject.toml Cargo.toml package.json setup.py 2>/dev/null

# Existing mise config
cat .mise.toml 2>/dev/null || cat mise.toml 2>/dev/null

# Existing release infra (semantic-release, Makefile, GitHub Actions)
ls .releaserc* release.config.* 2>/dev/null
ls .github/workflows/*release* 2>/dev/null
grep -i release Makefile 2>/dev/null

# Credentials already configured
grep -E 'GH_TOKEN|GITHUB_TOKEN|UV_PUBLISH_TOKEN|CARGO_REGISTRY_TOKEN|NPM_TOKEN' .mise.toml mise.toml 2>/dev/null
```

### 2b. Read the Reference Implementation

Read cc-skills' own release tasks as a working example — adapt, don't copy:

```bash
ls $HOME/.claude/plugins/marketplaces/cc-skills/.mise/tasks/release/
```

Also read: `$HOME/.claude/plugins/marketplaces/cc-skills/docs/RELEASE.md`

### 2c. Scaffold `.mise/tasks/release/`

Create only the tasks this repo actually needs. The 5-phase pattern is:

| Phase         | Task         | Purpose                                          | Required? |
| ------------- | ------------ | ------------------------------------------------ | --------- |
| 1. Preflight  | `preflight`  | Clean dir, auth, branch check                    | Always    |
| 2. Version    | `version`    | `semantic-release` (or repo-specific versioning) | Always    |
| 3. Publish    | `pypi`       | `uv publish` or custom script                    | If Python |
|               | `crates`     | `cargo publish --workspace` (Rust 1.90+)         | If Rust   |
|               | `npm`        | `npm publish`                                    | If Node   |
| 4. Verify     | `verify`     | Tag exists, release exists, artifacts published  | Always    |
| 5. Postflight | `postflight` | Clean git state, no unpushed, lockfile reset     | Always    |

**Orchestrator**: `full` task chains the phases with `depends = [...]`.

**Key rules**:

- Credentials in `.mise.toml` `[env]`, not hardcoded in scripts
- Tool versions in `.mise.toml` `[tools]`
- Lockfile drift reset in both `preflight` and `postflight` (build artifacts, not intentional changes)
- `--dry` and `--status` convenience tasks

### 2d. Known Issues

**Stale cc-skills marketplace-path hook entries leak into `~/.claude/settings.json`**: cc-skills' `release:preflight` hook-validation gate fails with `✗ N cc-skills marketplace-path entries found in settings.json`. Cause: hooks loaded from a previous Claude Code session can persist their resolved marketplace paths into `settings.json`, but plugin hooks are auto-loaded from each plugin's `hooks/hooks.json` — settings.json should NOT contain duplicates. The fix is built in: run `./scripts/sync-hooks-to-settings.sh` (idempotent — prunes only the leaked marketplace-path entries, leaves user-authored hooks alone), then re-run `mise run release:full`. Recurs across releases — not a one-time issue. Verified 2026-05-09 mid-release: 4 entries pruned, re-run succeeded.

**`@semantic-release/git` untracked file explosion**: v10.x runs `git ls-files -m -o` without `--exclude-standard`. Patch after install:

```bash
find $(npm root -g 2>/dev/null) node_modules \
  -path "*/@semantic-release/git/lib/git.js" 2>/dev/null | while read f; do
  grep -q 'exclude-standard' "$f" || \
    sed -i '' "s/\['ls-files', '-m', '-o'\]/['ls-files', '-m', '-o', '--exclude-standard']/" "$f"
done
```

**Partial semantic-release failure** (version bumped, no tag): Do NOT re-run semantic-release. Manually create tag + GitHub release, then continue with publish tasks.

**Cargo workspace lockfile cascade** (Rust workspaces with `version.workspace = true`): the perl-based version bump in `prepareCmd` only touches `Cargo.toml`, but the workspace version cascades into every member crate's entry in `Cargo.lock`. The `@semantic-release/git` plugin only stages files listed in `assets`, so without an explicit cargo invocation + `Cargo.lock` in assets, the lockfile stays at the old version. Symptoms: `release:preflight` fails with `M Cargo.lock` after a successful `release:version`, blocking `cargo publish`. Fix in `.releaserc.yml`:

```yaml
- - "@semantic-release/exec"
  - prepareCmd: |
      perl -i -pe 's/^version = ".*"/version = "${nextRelease.version}"/' Cargo.toml
      cargo update --workspace --offline 2>/dev/null \
        || cargo metadata --format-version=1 --offline >/dev/null 2>&1 \
        || true

- - "@semantic-release/git"
  - assets:
      - Cargo.toml
      - Cargo.lock # ← critical: capture the cascading version bump
    message: "chore(release): ${nextRelease.version} [skip ci]"
```

`--offline` keeps the sync local (no registry hit). The `|| true` fallback prevents `prepareCmd` from blocking the release if neither cargo command can run; if the lockfile actually needed sync, the next preflight will catch it.

**mise `depends` runs in parallel** (`release:full` deps `[preflight, version]`): preflight and version race. If version dirties the working tree (e.g. lockfile cascade above), preflight may report failure mid-stream while version still completes, leaving an irreversible tag + GitHub release without `release:crates`/`verify`/`postflight` having run. Recovery: fix the dirty state, then run the remaining phases manually (`release:crates` → `release:verify` → `release:postflight`) — do NOT re-run `release:full`, which would attempt another semantic-release pass.

**`release:verify` false-positive on naive grep against crates.io API**: a verify implementation of the form `if curl ... | grep -q "version"` matches the API's _error_ responses too, because the JSON error contains the word "version" in the detail text (e.g. `"crate <name> does not have a version <X.Y.Z>"`). Result: every release run reports "✓ <crate> published on crates.io" even when the actual cargo publish failed. Discovered after a multi-version, multi-day silent-failure window where every tagged version was absent from crates.io but verify reported green. Fix: match the unique success-only JSON shape `"num":"<exact-version>"` (or parse the JSON via Python so TOML's escape rules don't mangle the quotes inside a `run = """ ... """` block). Hard-fail on miss with the actual API response logged. Reference fix:

```bash
RESPONSE=$(curl -s "https://crates.io/api/v1/crates/<crate>/${VERSION}")
PUBLISHED_NUM=$(echo "$RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('version',{}).get('num',''))" 2>/dev/null || echo "")
if [[ "$PUBLISHED_NUM" == "$VERSION" ]]; then
    echo "✓ <crate> v${VERSION} confirmed on crates.io"
else
    echo "⚠ <crate> v${VERSION} NOT on crates.io (API returned: '${PUBLISHED_NUM:-error}')"
    echo "  raw: $(echo "$RESPONSE" | head -c 200)"
    exit 1
fi
```

**`release:full` silent-swallow of crates publish failures**: any orchestrator pattern of the form `mise run release:crates || { echo "non-fatal" }` masks the actual `cargo publish` error and continues with apparent success. Combined with the verify false-positive above, this hides multi-version publish gaps for days. Crates publish failure must be a hard fail; the operator runs `release:crates` manually after diagnosis (token, network, registry rate-limit, etc.). Replace any `||`-swallow with a plain unwrapped invocation.

**Schema-drift gate parser exits at indented `)`**: a parser of the form `awk '... in_table { if ($0 ~ /^[[:space:]]*\)/) { in_table = 0; exit } ... }'` truncates mid-table when a column's multi-line DEFAULT expression has a closing paren on its own indented line (e.g. nested `multiIf(\n  ...,\n  )` or arithmetic group close). Late columns are silently missed and falsely flagged as drift on the live-CH side. ClickHouse formatting convention puts the actual table-end `)` at column 0 — change the exit guard to `^\)` (no leading whitespace) for correctness.

**Parallel SSH ControlMaster race in pre-push gates**: when multiple mise tasks open SSH connections in parallel (`depends`), the loser prints `ControlSocket /Users/.../control-host:22 already exists, disabling multiplexing` to stderr. If your gate script uses `ssh ... 2>&1` (merging stderr into stdout), that warning leaks into the data pipe and trips identifier-shaped sanity checks with phantom "extra" entries. Fix: route SSH stderr to `/dev/null` (we don't need it for column extraction; real errors surface via the explicit exit-code propagation downstream), and add a defense-in-depth filter `grep -E '^[a-zA-Z_][a-zA-Z0-9_]*$'` so even if some other stderr path leaks in the future, only valid identifier-shaped strings flow through.

**Idempotent `git push` returns non-zero**: orchestrators that include a manual `git push --follow-tags origin main` step AFTER `release:version` (where semantic-release's `@semantic-release/git` plugin already pushed) can fail when re-pushing an already-up-to-date branch + an existing tag. Treat this as success: detect via `[[ $(git rev-parse HEAD) == $(git rev-parse origin/main) ]]` before pushing, and only push the latest tag if `git ls-remote --tags origin "refs/tags/$LATEST_TAG"` returns empty. Otherwise the wrapper exits non-zero on a release that actually shipped fine, polluting the success/failure signal.

**`git stash -u` of WIP `.gitignore` re-exposes ignored dirs and fails preflight (PII hazard)**: when the dirty working tree includes uncommitted `.gitignore` changes whose purpose is to ignore untracked directories, `git stash -u` reverts `.gitignore` to its committed state — which _un-ignores_ those dirs and surfaces them as `??` untracked. preflight's clean-tree gate (which typically exempts only `archive/sessions/` and `node_modules/`) then fails on them, and in a clinical/PII repo this momentarily exposes sensitive dirs (e.g. `correspondence/`, `site/`, `cdanet/`) to accidental `git add -A`. Root cause: an uncommitted `.gitignore` is _load-bearing infrastructure_, not feature WIP — stashing it inverts the ignore state. Fix: separate concerns. Commit the `.gitignore` hygiene (plus any tracked-companion files it references, e.g. a secret-free deploy script) as its own `chore:` commit so the ignore rules stay active and the tree is clean, then stash ONLY the true feature code with a pathspec'd `git stash push -- <files>` (NOT `-u`, which would re-expose the dirs). After the release, `git stash pop` restores the feature WIP. Verified 2026-06-02 on a clinical PII repo: a blanket `git stash -u` exposed three PII dirs + a machine-local TOML override; popping, committing `.gitignore` + the secret-free deploy script it references separately, then pathspec-stashing the feature files produced a clean preflight and a correctly-scoped release.

## Step 3: Execute

```bash
# Pre-release sync
git pull origin main

# Check for unpushed commits
git log --oneline @{u}..HEAD

# (cc-skills only) Preemptively prune leaked marketplace-path hook entries.
# This recurs across sessions — see Known Issues. Running this BEFORE
# release:full saves a preflight-fail/retry round trip. No-op if clean.
[[ -x ./scripts/sync-hooks-to-settings.sh ]] && ./scripts/sync-hooks-to-settings.sh

# Route by flags
mise run release:full    # default
mise run release:dry     # --dry
mise run release:status  # --status
```

If working directory is dirty: commit related changes or stash WIP first. **Caution**: if the dirt includes `.gitignore` changes, treat them as load-bearing — commit them as a `chore:` rather than stashing, because `git stash -u` reverts `.gitignore` and re-exposes any dirs it was ignoring (preflight then fails on them; PII hazard — see Known Issues). Stash only true feature code with a pathspec: `git stash push -- <files>`. Reset lockfile drift if present:

```bash
git diff --name-only | grep -E '(uv\.lock|package-lock\.json|Cargo\.lock|bun\.lockb)$' | xargs -r git checkout --
```

## Post-Execution Reflection

After this skill completes, reflect before closing the task:

0. **Locate yourself.** — Find this SKILL.md's canonical path before editing.
1. **What failed?** — Fix the instruction that caused it.
2. **What worked better than expected?** — Promote to recommended practice.
3. **What drifted?** — Fix any script, reference, or dependency that no longer matches reality.
4. **Log it.** — Evolution-log entry with trigger, fix, and evidence.

Do NOT defer. The next invocation inherits whatever you leave behind.
