# CI-build-and-push (short)

Quick reference for the manual workflow at `.github/workflows/CI-build-and-push.yml`.

What
- Builds one or more services and pushes images to GHCR (`ghcr.io/<owner>/<image>`).

How to run
- Actions UI → Manual Docker build & publish → Run workflow
- Or trigger `workflow_dispatch` via API/CLI

Main inputs
- `branch` — branch to checkout (default: `main`)
- `service` — service folder (e.g. `GrpcAuthService`) or `all`
- `push-latest` — `true`/`false` to also push `:latest` (default: `true`)

Tagging
- `commit`: image tagged with short commit SHA (recommended for CI)
- `:latest` is optional

Receipt & outputs
- Workflow prints a "Build & Publish Receipt" of pushed images
- Job output `pushed_images` contains newline-separated pushed image refs

Notes
- Uses `GITHUB_TOKEN` to auth to GHCR; repo/org must allow package writes

Examples
- Build `GrpcAuthService` from `main`, commit tag + latest: set `service=GrpcAuthService`, `push-latest=true`
- Build all services with semantic tag `v1.0.0`: set `service=all`, `tagging_mode=semantic`, `push-latest=false`
