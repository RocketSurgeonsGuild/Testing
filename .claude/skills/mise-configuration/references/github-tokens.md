# GitHub Multi-Account Auth (host-alias model)

**Parent Skill**: [mise-configuration](../SKILL.md)

> **⚠️ 2026-06-21 — this page was rewritten.** The previous version taught mise
> `[env]` `GH_TOKEN` injection from plaintext `~/.claude/.secrets/gh-token-*`
> files and cwd-based SSH `Match` directives. **All of that is retired.** mise no
> longer touches GitHub tokens (it manages tool versions only). The canonical
> model below is the **host-alias single-source-of-truth**. See the ADR:
> `~/.claude/docs/adr/2025-12-17-github-multi-account-authentication.md` (§2026-06-21).

## The model: the remote URL host-alias is the single source of truth

A repo's `origin` remote names its account, and **that one signal drives all three layers** — so identity travels with the repo, not its folder location.

```
git@github.com-<account>:owner/repo.git
              ^^^^^^^^^ the account
```

| Layer               | Mechanism (keyed off the alias)                                                                                                                                                                                                                                                                         |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **SSH key**         | `Host github.com-<account>` block in `~/.ssh/config` → `IdentityFile`, with `IdentitiesOnly yes` + `ControlMaster no` + `ControlPath none`. A top-of-file `Host github.com github.com-* ssh.github.com` block with `ControlPath none` is the multiplexing kill-switch (defeats the wrong-identity bug). |
| **Commit identity** | `~/.gitconfig`: `[includeIf "hasconfig:remote.*.url:git@github.com-<account>:*/**"] path = ~/.gitconfig-<account>` — binds identity to the remote, not the folder (git ≥ 2.36).                                                                                                                         |
| **gh CLI**          | The neutral `gh` wrapper in `~/.zshrc` derives the account from the remote alias → `GH_CONFIG_DIR=~/.config/gh-<account>` (isolated profile) and **strips `GH_TOKEN`** (a stale ambient token would otherwise outrank the profile and 401 after a rotation).                                            |

## Tokens are resolved fresh — never stored or injected

There are **no** plaintext token files and **no** ambient `GH_TOKEN`. Anything that
needs an HTTP token (semantic-release, CI scripts) resolves one at the moment of use:

```bash
# Account derived from the repo's origin alias → that account's gh profile → fresh token
GH_PAT="$(~/.claude/tools/bin/gh-token-for-repo)"
GITHUB_TOKEN="$GH_PAT" GH_TOKEN="$GH_PAT" npx semantic-release
```

`gh-token-for-repo` runs `GH_CONFIG_DIR=~/.config/gh-<account> gh auth token`
(verified to work headless under launchd with no Touch ID). **1Password is the
at-rest SSoT** (each account's gh login is provisioned from it; `~/.gitconfig-<account>`
references `githubToken1PasswordID`).

## Account → alias → key map

| Account      | Remote alias            | SSH key                         | gh profile                |
| ------------ | ----------------------- | ------------------------------- | ------------------------- |
| terrylica    | `github.com-terrylica`  | `id_ed25519_terrylica`          | `~/.config/gh-terrylica`  |
| tainora      | `github.com-tainora`    | `id_ed25519_tainora`            | `~/.config/gh-tainora`    |
| 459ecs       | `github.com-459ecs`     | `id_ed25519_459ecs`             | `~/.config/gh-459ecs`     |
| vanjobbers   | `github.com-vanjobbers` | `id_ed25519_vanjobbers`         | `~/.config/gh-vanjobbers` |
| Eon-Labs org | `github.com-eonlabs`    | `id_ed25519_terrylica` (member) | `~/.config/gh-terrylica`  |

## Verification

```bash
# In any repo: account is whatever the origin alias names
git -C <repo> remote get-url origin        # → git@github.com-<account>:...
~/.claude/tools/bin/gh-token-for-repo | GH_TOKEN=$(cat) gh api user --jq .login   # → <account>
```

## RETIRED — do NOT reintroduce

- mise `[env]` injecting `GH_TOKEN`/`GITHUB_TOKEN`/`GH_CONFIG_DIR`/`GH_ACCOUNT`
  (including via `read_file(... .secrets/gh-token-*)` **or** `op read` — any ambient
  token is the failure mode).
- plaintext `~/.claude/.secrets/gh-token-*` files (deleted).
- the `git-credential-gh-token` helper; "HTTPS-first" git remotes.
- cwd-based `Match host github.com exec "pwd | grep ..."` SSH directives.
- `includeIf "gitdir:..."` account selection (replaced by `hasconfig:remote.*.url`).
- the `~/.config/gh-profiles/*` and `~/.config/gh/profiles/*` conventions (deleted).

## References

- ADR: `~/.claude/docs/adr/2025-12-17-github-multi-account-authentication.md` (§2026-06-21)
- Resolver: `~/.claude/tools/bin/gh-token-for-repo`
