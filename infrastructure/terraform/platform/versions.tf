terraform {
  required_version = ">= 1.5.0"

  required_providers {
    render = {
      source  = "render-oss/render"
      version = "~> 1.8"
    }
    vercel = {
      source  = "vercel/vercel"
      version = "~> 1.14"
    }
  }

  # CI persists state via workflow artifact (see .github/workflows/deploy.yml).
  # For teams, switch to Terraform Cloud / S3 backend.
  backend "local" {
    path = "terraform.tfstate"
  }
}
