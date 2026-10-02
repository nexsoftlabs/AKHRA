# AWS (media delivery)

S3 media bucket, CloudFront OAC, and optional RDS baseline. **Production app database** is on **Supabase** (see `../platform/`).

```bash
cd infrastructure/terraform/aws
terraform init
terraform plan -var="media_bucket_name=your-unique-bucket" -var="db_password=..."
```
