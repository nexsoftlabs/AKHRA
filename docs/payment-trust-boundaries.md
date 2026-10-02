# Payment trust boundaries

## Untrusted inputs

- Browser Razorpay checkout success handler
- Any `amount`, `currency`, or `movieId` from query/body without server correlation
- Replay of old payment IDs against new orders

## Trusted sources

- Razorpay server-to-server webhook (after HMAC verify on **exact** raw body)
- Razorpay REST API status fetch using server credentials (corroboration)
- Internal `orders` / `payments` rows created by the API

## State machine

```text
Pending → Authorized → Captured → (Refunded | Disputed)
         ↘ Failed
```

Entitlements are granted only on transition to **Captured** inside a DB transaction with unique constraints on `(user_id, movie_id)` active entitlements and `payment_events.provider_event_id`.

## Idempotency

- Webhook: unique `provider_event_id`
- Payment finalize: `idempotency_key` on payment row
- Concurrent purchases: transactional check for existing entitlement before order creation

## Reconciliation

Nightly job (Phase 5) compares Razorpay settlements with captured payments; discrepancies alert FinanceAdmin.
