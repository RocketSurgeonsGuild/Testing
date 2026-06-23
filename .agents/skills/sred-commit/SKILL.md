---
name: sred-commit
description: "Create a git commit with SR&ED (CRA tax credit) trailers. TRIGGERS - sred commit, sred, cra commit, tax credit commit, scientific research commit."
allowed-tools: Read, Bash, Glob, Grep, Write, AskUserQuestion
argument-hint: "[commit message summary]"
disable-model-invocation: false
---

# /mise:sred-commit

Create a git commit with SR&ED (Scientific Research & Experimental Development) trailers for Canada CRA tax credit compliance.

> **Self-Evolving Skill**: This skill improves through use. If instructions are wrong, parameters drifted, or a workaround was needed — fix this file immediately, don't defer. Only update for real, reproducible issues.

## Step 1: Understand Staged Changes

```bash
git diff --cached --stat
```

If nothing is staged, run `git status` and ask the user what to stage.

## Step 2: Draft the Commit Message

Write a commit message to `/tmp/sred-commit-msg.txt` following this format:

```
<type>(<scope>): <subject>

<body>

SRED-Type: <category>
SRED-Claim: <PROJECT-IDENTIFIER>
```

### Conventional Commit Types

| Type     | When to Use                             |
| -------- | --------------------------------------- |
| feat     | New feature or capability               |
| fix      | Bug fix                                 |
| docs     | Documentation only changes              |
| style    | Formatting, missing semicolons, etc.    |
| refactor | Code change that neither fixes nor adds |
| perf     | Performance improvement                 |
| test     | Adding or correcting tests              |
| build    | Build system or external dependencies   |
| ci       | CI configuration files and scripts      |
| chore    | Maintenance tasks                       |
| revert   | Reverting a previous commit             |

### SR&ED Types (CRA Definitions)

| SRED-Type                | CRA Definition                                                    |
| ------------------------ | ----------------------------------------------------------------- |
| experimental-development | Achieving technological advancement through systematic work       |
| applied-research         | Scientific knowledge with specific practical application in view  |
| basic-research           | Scientific knowledge without specific practical application       |
| support-work             | Programming, testing, data collection supporting SR&ED activities |

### SRED-Claim Format

Format: `PROJECT[-VARIANT]` (uppercase alphanumeric + hyphens).

To discover existing project identifiers in this repo:

```bash
git log --format='%(trailers:key=SRED-Claim,valueonly)' | sort -u | grep .
```

If no existing identifiers match, derive from the commit scope (e.g., `feat(my-feature):` → `MY-FEATURE`).

If unsure which SRED-Type or SRED-Claim to use, use `AskUserQuestion` to ask the user.

## Step 3: Validate

Run the validator on the draft message:

```bash
bun $HOME/.claude/plugins/marketplaces/cc-skills/plugins/itp-hooks/hooks/sred-commit-guard.ts --validate-message /tmp/sred-commit-msg.txt
```

Parse the JSON output:

- `{"valid": true}` → proceed to commit
- `{"valid": false, "errors": [...]}` → fix the message and re-validate
- If the output includes `"discovery"`, present the suggestions to the user via `AskUserQuestion`

## Step 4: Commit

```bash
git commit -F /tmp/sred-commit-msg.txt
```

Using `-F` avoids shell quoting issues with special characters in commit messages.

## Step 5: Clean Up

```bash
rm -f /tmp/sred-commit-msg.txt
```

Show the user the commit hash and summary.


## Post-Execution Reflection

After this skill completes, reflect before closing the task:

0. **Locate yourself.** — Find this SKILL.md's canonical path before editing.
1. **What failed?** — Fix the instruction that caused it.
2. **What worked better than expected?** — Promote to recommended practice.
3. **What drifted?** — Fix any script, reference, or dependency that no longer matches reality.
4. **Log it.** — Evolution-log entry with trigger, fix, and evidence.

Do NOT defer. The next invocation inherits whatever you leave behind.
