resource "vercel_project" "ui" {
  name      = "${var.project_name}-ui"
  framework = "vite"

  root_directory = "frontend"

  git_repository = {
    type = "github"
    repo = replace(replace(var.github_repo_url, "https://github.com/", ""), ".git", "")
  }

  build_command   = "npm run build"
  install_command = "npm ci"
  output_directory = "dist"
}

resource "vercel_project_environment_variable" "api_base_url" {
  project_id = vercel_project.ui.id
  key        = "VITE_API_BASE_URL"
  value      = coalesce(render_web_service.api.url, "https://${var.project_name}-api.onrender.com")
  target     = ["production", "preview", "development"]
}

resource "vercel_project_environment_variable" "google_client_id" {
  count = var.google_client_id != "" ? 1 : 0

  project_id = vercel_project.ui.id
  key        = "VITE_GOOGLE_CLIENT_ID"
  value      = var.google_client_id
  target     = ["production", "preview", "development"]
}
