# build-and-publish (short)

Quick reference for the manual workflow at `.github/workflows/build-and-publish.yml`.

What
- Builds one or more services and pushes images to GHCR (`ghcr.io/<owner>/<image>`).

How to run
- Actions UI → Manual Docker build & publish → Run workflow
- Or trigger `workflow_dispatch` via API/CLI

Main inputs
- `branch` — branch to checkout (default: `main`)
- `service` — service folder (e.g. `GrpcAuthService`) or `all`
- `push-latest` — `true`/`false` to also push `:latest` (default: `true`)
- `tagging_mode` — `commit` (default) or `semantic`
- `version` — when `tagging_mode=semantic`, the tag to use (e.g. `v1.0.0`)

Tagging
- `commit`: image tagged with short commit SHA (recommended for CI)
- `semantic`: image tagged with provided `version` (use for releases)
- `:latest` is optional; it's mutable and convenient but not ideal for reproducible deployments

Receipt & outputs
- Workflow prints a "Build & Publish Receipt" of pushed images
- Job output `pushed_images` contains newline-separated pushed image refs

Notes
- Uses `GITHUB_TOKEN` to auth to GHCR; repo/org must allow package writes
- Runner builds single-arch images by default (amd64). Add `buildx` for multi-arch
- Can extend to post PR comments, create releases, or enable multi-arch builds on request

Examples
- Build `GrpcAuthService` from `main`, commit tag + latest: set `service=GrpcAuthService`, `tagging_mode=commit`, `push-latest=true`
- Build all services with semantic tag `v1.0.0`: set `service=all`, `tagging_mode=semantic`, `version=v1.0.0`, `push-latest=false`
