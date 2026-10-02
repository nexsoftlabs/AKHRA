# ADR 002: Money as integer minor units

## Status

Accepted — 2026-09-28

## Decision

Store all monetary amounts as `bigint` minor units (INR paise) with ISO currency code string. Never use `float`/`double` for money.

## Consequences

- Razorpay amounts align naturally (5000 = ₹50)
- Requires explicit formatting at UI layer
