# Project progress

Last updated: 2026-09-29

## Phases 1–4 — Media pipeline

| Item | Status |
|------|--------|
| S3 presign upload + MediaConvert worker (when `Media__UseMediaConvert` + ARNs set) | Done |
| Dev fallback HLS (Mux demo URL) when AWS not configured | Done |
| HLS manifest + segment proxy (`/api/v1/playback/stream/...`) | Done |
| CloudFront signed manifest URLs when `Media__CloudFront*` set | Done |
| Terraform: S3 OAC bucket policy + optional CloudFront key group | Done |

## Phase 5 — Commerce

| Item | Status |
|------|--------|
| Razorpay movie checkout + verify + webhooks | Done |
| Razorpay **Subscriptions** checkout when plan has `RazorpayPlanId` (auto-sync on API start) | Done |
| Webhooks: `subscription.charged`, `subscription.halted`, `subscription.cancelled` | Done |
| Partial refunds + full refund revokes entitlements / active subscription | Done |
| Reconciliation: DB vs Razorpay payment count + warning on mismatch | Done |
| Plan change / proration / dunning automation | Not implemented (manual + webhooks baseline) |

## Phase 6 — Frontend / admin

| Item | Status |
|------|--------|
| Browse genre filter + `GET /api/v1/movies/genres` | Done |
| Admin movie edit (`/admin/movies/:id`) | Done |
| Finance: search, CSV export, refund history, partial refunds | Done |
| Google Sign-In hint when `VITE_GOOGLE_CLIENT_ID` unset | Done |
| Email confirm banner + resend (`POST /api/v1/auth/email/resend`) | Done |

## Phase 7–8

Playwright smoke, Terraform RDS/S3/CloudFront, production runbook — baseline unchanged.

## Run locally

```bash
docker compose -f infrastructure/docker/docker-compose.yml up -d postgres redis
dotnet run --project backend/MoviePlatform.Api --launch-profile http
dotnet run --project backend/MoviePlatform.Worker
cd frontend && npm run dev
```

**Accounts:** `demo@akhra.app` / `DemoPass123!` · `admin@akhra.app` / `AdminPass123!`

**AWS media (optional):** set `Media__S3Bucket`, `Media__MediaConvertRoleArn`, `Media__MediaConvertQueueArn`, `Media__MediaConvertEndpoint`, `Media__UseMediaConvert=true`.
