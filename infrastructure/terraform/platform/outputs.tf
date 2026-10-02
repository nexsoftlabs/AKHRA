output "render_api_url" {
  description = "Public URL of the .NET API on Render."
  value       = coalesce(render_web_service.api.url, "https://${var.project_name}-api.onrender.com")
}

output "vercel_project_id" {
  value = vercel_project.ui.id
}

output "vercel_production_url" {
  value = local.vercel_production_url
}
