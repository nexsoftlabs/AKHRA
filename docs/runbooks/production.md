# Production runbook

## Deploy

1. Apply Terraform (`infrastructure/terraform`) with unique `media_bucket_name` and strong `db_password`.
2. Set secrets: `ConnectionStrings__DefaultConnection`, `Razorpay__*`, `Email__*`, `Media__S3Bucket`, `Media__S3Region`, `Media__CloudFrontDomain`, `Media__CloudFrontKeyPairId`, `Media__CloudFrontPrivateKeyPem` (PEM newlines as `\n` or file mount).
3. Run API + Worker containers (see `infrastructure/docker/Dockerfile.api`).
4. Run EF migrations on startup (`DbInitializer`).

## Health

- Liveness: `GET /healthz`
- Readiness: `GET /healthz/ready`
- Version: `GET /api/v1/version`

## Incidents

| Symptom | Check |
|---------|--------|
| Payments fail | Razorpay dashboard, API logs, `payments` table |
| No playback | `MediaAssets` HLS ready, entitlements/subscriptions |
| Admin 403 | MFA step-up cookie, roles |

## Backups

- RDS: automated snapshots (7-day retention in Terraform template).
- Media: S3 versioning recommended (enable manually).

## Monitoring

- CloudWatch alarms on API 5xx, RDS CPU, Worker error logs (wire in Phase 8 extension).
