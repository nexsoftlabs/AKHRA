variable "project_name" {
  type        = string
  description = "Short project slug used in service names."
  default     = "akhra"
}

variable "github_repo_url" {
  type        = string
  description = "HTTPS Git URL Render/Vercel deploy from (no branch suffix)."
  default     = "https://github.com/nexsoftlabs/AKHRA"
}

variable "github_branch" {
  type    = string
  default = "main"
}

# Nexsoft Render workspace (team owner id)
variable "render_owner_id" {
  type        = string
  description = "Render owner id (team id tea-... or user id usr-...)."
  default     = "tea-davvg1qjnfac73da8vu0"
}

variable "render_api_key" {
  type        = string
  sensitive   = true
  description = "Render API key (RENDER_API_KEY)."
}

variable "render_region" {
  type    = string
  default = "singapore"
}

variable "render_plan" {
  type    = string
  default = "starter"
}

variable "vercel_api_token" {
  type        = string
  sensitive   = true
  description = "Vercel API token."
}

variable "vercel_team_id" {
  type        = string
  default     = ""
  description = "Optional Vercel team id; leave empty for personal account."
}

variable "supabase_project_ref" {
  type        = string
  description = "Existing Supabase project ref (database host db.<ref>.supabase.co)."
  default     = "hauyxvckfhwegjjyqclc"
}

variable "supabase_db_password" {
  type        = string
  sensitive   = true
  description = "Supabase Postgres password (postgres user)."
}

variable "database_connection_string" {
  type        = string
  sensitive   = true
  default     = ""
  description = "Optional full Npgsql connection string. If empty, built from supabase_project_ref + password."
}

variable "redis_connection_string" {
  type        = string
  sensitive   = true
  default     = ""
  description = "Optional Redis URL for Render API. Leave empty to use in-memory cache."
}

variable "jwt_signing_key" {
  type        = string
  sensitive   = true
  description = "Jwt__SigningKey for production sessions."
}

variable "cors_allowed_origins" {
  type        = list(string)
  description = "Browser origins allowed to call the API with credentials."
  default     = []
}

variable "public_web_base_url" {
  type        = string
  description = "Consumer site URL (Vercel production)."
  default     = ""
}

variable "google_client_id" {
  type      = string
  default   = ""
  sensitive = false
}

variable "razorpay_key_id" {
  type      = string
  default   = ""
  sensitive = true
}

variable "razorpay_key_secret" {
  type      = string
  default   = ""
  sensitive = true
}
