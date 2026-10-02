# Movie Streaming Platform

Production-oriented licensed movie streaming: .NET 10 API, React 19 SPA, PostgreSQL, Redis, Razorpay checkout (sandbox), and private HLS delivery on AWS (later phases).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (PostgreSQL + Redis locally)

## Quick start (local)

```bash
# Data services
docker compose -f infrastructure/docker/docker-compose.yml up -d postgres redis

# API (applies EF migrations on startup)
dotnet run --project backend/MoviePlatform.Api

# Frontend
cd frontend && npm install && npm run dev
```

- API: http://localhost:5017  
- Frontend: http://localhost:5173 (proxies `/api` to the API)  
- Health: http://localhost:5017/healthz  

Copy `.env.example` to `.env` for reference; ASP.NET uses `appsettings.json` and environment variables.

## Repository layout

```text
backend/           # .NET modular monolith + worker + tests
frontend/          # React 19 + Vite + shadcn/ui
docs/              # Architecture, ADRs, trust boundaries
infrastructure/    # Docker Compose, Terraform (Phase 8)
```

## Documentation

- [Architecture](docs/architecture.md)
- [Implementation plan](docs/implementation-plan.md)
- [ER overview](docs/er-diagram.md)
- [PROJECT_PROGRESS.md](PROJECT_PROGRESS.md)

## Tests

```bash
dotnet test
cd frontend && npm run build
```

## Deploy (Render + Vercel + Supabase)

Terraform and GitHub Actions live under [`infrastructure/terraform/platform/`](infrastructure/terraform/platform/README.md):

- **API** → Render (Docker, `main`)
- **UI** → Vercel (`frontend/`)
- **DB** → Supabase Postgres (connection string on Render)

Add repository secrets listed in the platform README, then push to `main` or run the **Deploy (Terraform)** workflow.

## External credentials (not in repo)

- **Razorpay (buy flow):** Create a [test mode](https://razorpay.com/docs/payments/payments/test-card-upi-details/) account. Set `Razorpay__KeyId` and `Razorpay__KeySecret` on the API (see `.env.example`), restart the API, sign in, then **Buy now** on a movie. Payment is verified server-side (signature + Razorpay API) before an entitlement is granted. Optional: `Razorpay__WebhookSecret` for `POST /api/v1/checkout/webhooks/razorpay`.
- **Google (sign up / sign in):** In [Google Cloud Console](https://console.cloud.google.com/apis/credentials), create an **OAuth 2.0 Client ID** (Web application). Add **Authorized JavaScript origins** `http://localhost:5173` (and your production URL later). Copy the client ID into `Auth__GoogleClientId` when starting the API and into `frontend/.env` as `VITE_GOOGLE_CLIENT_ID` (same value). Restart the API and Vite. Sign in or register via **Continue with Google** on `/login` and `/register`. Existing email/password users can link Google under **Account** after signing in.
- SMS OTP, AWS (S3, MediaConvert, CloudFront signing keys) — later phases.
