# ADR 001: Modular monolith

## Status

Accepted — 2026-09-28

## Context

The platform must ship quickly with strong consistency for payments and entitlements while remaining evolvable.

## Decision

Deploy a single ASP.NET Core API and background worker with internal module boundaries (Identity, Catalog, Media, Commerce, etc.) rather than microservices.

## Consequences

- Simpler transactions for payment + entitlement grants
- One deployment unit until scale or team boundaries justify extraction
- Clear folders/projects prepare future service splits
