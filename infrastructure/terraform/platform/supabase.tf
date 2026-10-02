# Database: Supabase Postgres (existing project — ref in supabase_project_ref).
# EF migrations run when the Render API starts (DbInitializer.MigrateAsync).

output "supabase_project_ref" {
  value       = var.supabase_project_ref
  description = "Supabase project ref (AKHRA Project)."
}

output "supabase_database_host" {
  value       = "db.${var.supabase_project_ref}.supabase.co"
  description = "Direct Postgres host for the Supabase project."
}
