locals {
  db_connection = coalesce(
    var.database_connection_string,
    format(
      "Host=aws-0-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.%s;Password=%s;SSL Mode=Require;Trust Server Certificate=true",
      var.supabase_project_ref,
      var.supabase_db_password,
    ),
  )

  vercel_production_url = var.public_web_base_url != "" ? var.public_web_base_url : "https://${var.project_name}-ui.vercel.app"

  cors_origins = length(var.cors_allowed_origins) > 0 ? var.cors_allowed_origins : [
    local.vercel_production_url,
    "http://localhost:5173",
  ]
}
