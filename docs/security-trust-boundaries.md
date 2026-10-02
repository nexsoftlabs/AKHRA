# Security trust boundaries

## Actors

- **Guest** — untrusted browser; may only access public catalog metadata and authorized trailers.
- **Authenticated user** — identity established via cookie/session (Phase 2+); still untrusted for authorization and payment amounts.
- **Content manager / admins** — elevated roles with step-up for destructive or financial actions.
- **Razorpay** — trusted only after HMAC signature verification on raw webhook body.
- **Google OIDC** — trusted after issuer, audience, and token validation.
- **AWS S3 / CloudFront** — trusted storage and delivery; credentials never exposed to clients.

## Rules

1. **Playback** — Every stream requires a short-lived server-issued authorization; frontend state is ignored.
2. **Payments** — Client callbacks never grant entitlements; only verified capture + idempotent persistence.
3. **Uploads** — Presigned URLs are scoped, time-limited, and tied to an upload session owned by an authorized user.
4. **Secrets** — Razorpay secrets, JWT signing keys, CloudFront private keys, and SMS keys live in environment/secret manager only.
5. **PII** — OTP codes and refresh tokens stored hashed; audit logs redact payment payloads where required.

## Threat model (summary)

| Threat | Mitigation |
|--------|------------|
| IDOR on movies/orders | Policy-based authorization + resource ownership checks |
| Price tampering | Server-side price from DB; Razorpay order amount match |
| Forged webhook | Signature over raw body; event ID deduplication |
| Token theft | HttpOnly cookies, rotation, revocation |
| Direct S3/CDN URL sharing | Short-lived signed URLs for manifest **and** segments |
| OTP brute force | Rate limits, attempt caps, short TTL |
| Admin abuse | RBAC, audit log, step-up auth for SuperAdmin |

Full STRIDE review scheduled in Phase 7.
