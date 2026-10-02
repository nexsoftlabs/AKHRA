# Implementation plan

## Phase 1 — Foundation ✅ (in progress)

- Solution scaffold, domain model, initial migration, Docker Compose, health endpoints, architecture docs.

## Phase 2 — Identity

- Cookie auth, Google OIDC, SMS OTP abstraction, sessions, email verification, RBAC integration tests.

## Phase 3 — Catalog / admin

- Movie CRUD, search/filters, licensing metadata, admin UI, upload session API stubs.

## Phase 4 — Media

- S3 presigned upload, worker + MediaConvert, HLS packaging, CloudFront signed URLs, playback session API.

## Phase 5 — Commerce

- Razorpay orders, verify + webhooks, entitlements, subscriptions, refunds.

## Phase 6 — Frontend

- Full customer + admin flows, shadcn components, HLS player, checkout UX.

## Phase 7 — Quality

- Playwright E2E, security tests, accessibility, load tests, threat model sign-off.

## Phase 8 — Production

- Terraform AWS, CI/CD deploy, monitoring, backups, runbooks.
