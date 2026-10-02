terraform {
  required_version = ">= 1.5.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

resource "aws_s3_bucket" "media" {
  bucket = var.media_bucket_name

  tags = {
    Project = "akhra"
    Phase   = "8"
  }
}

resource "aws_cloudfront_origin_access_control" "media" {
  name                              = "${var.project_name}-media-oac"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "media" {
  enabled             = true
  comment             = "${var.project_name} HLS"
  default_root_object = ""

  origin {
    domain_name              = aws_s3_bucket.media.bucket_regional_domain_name
    origin_id                = "media-s3"
    origin_access_control_id = aws_cloudfront_origin_access_control.media.id
  }

  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD", "OPTIONS"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = "media-s3"
    viewer_protocol_policy = "redirect-to-https"

    forwarded_values {
      query_string = true
      cookies { forward = "none" }
    }
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
  }

  tags = { Project = var.project_name }
}

resource "aws_s3_bucket_public_access_block" "media" {
  bucket                  = aws_s3_bucket.media.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

data "aws_iam_policy_document" "media_cloudfront_oac" {
  statement {
    sid    = "AllowCloudFrontOACRead"
    effect = "Allow"
    principals {
      type        = "Service"
      identifiers = ["cloudfront.amazonaws.com"]
    }
    actions   = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.media.arn}/*"]
    condition {
      test     = "StringEquals"
      variable = "AWS:SourceArn"
      values   = [aws_cloudfront_distribution.media.arn]
    }
  }
}

resource "aws_s3_bucket_policy" "media_cloudfront" {
  bucket = aws_s3_bucket.media.id
  policy = data.aws_iam_policy_document.media_cloudfront_oac.json
}

# Optional: create a CloudFront key group for signed URLs (set Media__CloudFrontKeyPairId + private key in app).
resource "aws_cloudfront_public_key" "media_signing" {
  count       = var.cloudfront_signing_public_key_pem != "" ? 1 : 0
  name        = "${var.project_name}-media-signing"
  encoded_key = var.cloudfront_signing_public_key_pem
}

resource "aws_cloudfront_key_group" "media_signing" {
  count = var.cloudfront_signing_public_key_pem != "" ? 1 : 0
  name  = "${var.project_name}-media-keys"
  items = [aws_cloudfront_public_key.media_signing[0].id]
}

resource "aws_db_instance" "postgres" {
  identifier              = "${var.project_name}-postgres"
  engine                  = "postgres"
  engine_version          = "16"
  instance_class          = var.db_instance_class
  allocated_storage       = 20
  username                = var.db_username
  password                = var.db_password
  db_name                 = var.db_name
  skip_final_snapshot     = true
  publicly_accessible     = false
  backup_retention_period = 7

  tags = {
    Project = var.project_name
  }
}

output "media_bucket" {
  value = aws_s3_bucket.media.bucket
}

output "postgres_endpoint" {
  value = aws_db_instance.postgres.address
}

output "cloudfront_domain" {
  value = aws_cloudfront_distribution.media.domain_name
}

output "cloudfront_key_group_id" {
  value = length(aws_cloudfront_key_group.media_signing) > 0 ? aws_cloudfront_key_group.media_signing[0].id : null
}
