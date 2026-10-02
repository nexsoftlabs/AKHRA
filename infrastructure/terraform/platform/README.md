# AKHRA platform (Terraform)

Provisions:

| Component | Provider | Resource |
|-----------|----------|----------|
| **API** | Render | `render_web_service` (Docker, repo root `Dockerfile`) |
| **UI** | Vercel | `vercel_project` (`frontend/`, Vite) |
| **DB** | Supabase | Connection string → Render env (project ref `hauyxvckfhwegjjyqclc`) |

## CI/CD

`.github/workflows/deploy.yml` runs `terraform plan/apply` on `main` when platform files change. Application deploys are **automatic** from GitHub after Render/Vercel are linked (same repo/branch).

### GitHub repository secrets (`nexsoftlabs/AKHRA`)

| Secret | Purpose |
|--------|---------|
| `RENDER_API_KEY` | Render API key |
| `RENDER_OWNER_ID` | `tea-davvg1qjnfac73da8vu0` (Nexsoft Workspace) |
| `VERCEL_API_TOKEN` | Vercel token |
| `VERCEL_TEAM_ID` | Optional team id |
| `SUPABASE_DB_PASSWORD` | Postgres `postgres` user password |
| `JWT_SIGNING_KEY` | Production JWT signing secret |
| `GOOGLE_CLIENT_ID` | Optional Google Sign-In |
| `RAZORPAY_KEY_ID` / `RAZORPAY_KEY_SECRET` | Optional payments |
| `VERCEL_PRODUCTION_URL` | e.g. `https://akhra-ui.vercel.app` (for CORS + email links) |
| `DATABASE_CONNECTION_STRING` | Optional override for full Npgsql string |

Also install **Render** and **Vercel** GitHub apps on `nexsoftlabs/AKHRA` so auto-deploy works.

## Local apply

```bash
cd infrastructure/terraform/platform
cp terraform.tfvars.example terraform.tfvars
# export TF_VAR_render_api_key=... (and other TF_VAR_*)
terraform init
terraform plan
terraform apply
```

State: CI stores `terraform.tfstate` as a workflow artifact; migrate to Terraform Cloud or S3 for team use.
