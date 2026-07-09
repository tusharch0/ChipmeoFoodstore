# Production rollout and handover

## Repository-provided controls

- `GET /v2/api/health` is a liveness check; `GET /v2/api/health/ready` confirms PostgreSQL connectivity for deployment readiness checks.
- `.github/workflows/verify.yml` restores, builds, and tests the API and builds every frontend on pull requests and `master` pushes.
- Financial reports use `Africa/Nairobi` dates, record each view/export in `report_access_logs`, and never cross organization or branch scope without an owner/platform claim.

## Required environment separation

Create separate Docker/managed-service environments for development, staging, sandbox, pilot, and production. Each must have its own PostgreSQL database, Redis instance, S3 bucket, JWT secret, IntaSend credentials, callback URL, and admin proxy URL. Set these only through the deployment secret store or environment; do not commit them.

## Deployment checklist

1. Run the verification workflow and review database migrations before release.
2. Take and verify a PostgreSQL backup; record its restore location and owner.
3. Deploy to staging, wait for `/v2/api/health/ready`, and run payment/webhook duplicate-callback smoke tests.
4. Apply migrations once, deploy application containers, then re-check readiness and critical dashboards.
   Set `DATABASE_AUTO_MIGRATE=false` outside local development and run `dotnet ef database update --project foodstore-api/FoodstoreApi.Infrastructure --startup-project foodstore-api/FoodstoreApi.Web` as the controlled migration step.
5. For rollback, revert the application image first. Do not roll back a database migration without an approved migration-specific recovery plan.

## Operational ownership

The repository cannot provision TLS certificates, managed backups, on-call responders, IntaSend production credentials, or monitoring/alert destinations. Assign those items before pilot or production go-live, then rehearse backup restore, provider outage, webhook replay, and settlement failure procedures.
