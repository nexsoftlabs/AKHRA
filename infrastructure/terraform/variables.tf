variable "aws_region" {
  type    = string
  default = "ap-south-1"
}

variable "project_name" {
  type    = string
  default = "akhra"
}

variable "media_bucket_name" {
  type = string
}

variable "db_instance_class" {
  type    = string
  default = "db.t4g.micro"
}

variable "db_username" {
  type    = string
  default = "movieplatform"
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "db_name" {
  type    = string
  default = "movieplatform"
}

variable "cloudfront_signing_public_key_pem" {
  type        = string
  default     = ""
  description = "PEM public key for CloudFront signed URLs; leave empty to skip key group."
}
