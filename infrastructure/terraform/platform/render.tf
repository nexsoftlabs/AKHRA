resource "render_web_service" "api" {
  name   = "${var.project_name}-api"
  plan   = var.render_plan
  region = var.render_region

  runtime_source = {
    docker = {
      auto_deploy         = true
      branch              = var.github_branch
      repo_url            = var.github_repo_url
      dockerfile_path     = "Dockerfile"
      docker_context_path = "."
    }
  }

  health_check_path = "/healthz"

  env_vars = merge(
    {
      ASPNETCORE_ENVIRONMENT               = { value = "Production" }
      ConnectionStrings__DefaultConnection = { value = local.db_connection }
      Email__PublicWebBaseUrl              = { value = local.vercel_production_url }
      Jwt__SigningKey                      = { value = var.jwt_signing_key }
    },
    var.redis_connection_string != "" ? {
      ConnectionStrings__Redis = { value = var.redis_connection_string }
    } : {},
    var.google_client_id != "" ? {
      Auth__GoogleClientId = { value = var.google_client_id }
    } : {},
    var.razorpay_key_id != "" ? {
      Razorpay__KeyId = { value = var.razorpay_key_id }
    } : {},
    var.razorpay_key_secret != "" ? {
      Razorpay__KeySecret = { value = var.razorpay_key_secret }
    } : {},
    {
      for idx, origin in local.cors_origins :
      "Cors__AllowedOrigins__${idx}" => { value = origin }
    },
  )
}
