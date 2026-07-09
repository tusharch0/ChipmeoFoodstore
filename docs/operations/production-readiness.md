# Pilot and production readiness

## Pilot record

Record pilot restaurants and branches, release SHA, start/end dates, support window, daily reconciliation owner, incident summary, unresolved exceptions, performance results, and go/no-go decision. Reconcile payment, ledger, wallet, and settlement totals every pilot day.

## Go-live checklist

- CI build, static checks, tests, and migration review pass.
- Development, staging, sandbox, pilot, and production use isolated databases, Redis, object storage, secrets, provider projects, and callback URLs.
- HTTPS, reverse-proxy access controls, backups, secret ownership, monitoring destinations, uptime checks, and on-call responders are assigned externally.
- Payment and settlement alerts have been triggered in a rehearsal.
- Duplicate callback, provider outage, load, security, backup restore, and rollback exercises have recorded evidence.
- `scripts/phase5-load-smoke.ps1` passes against staging at the agreed request count, concurrency, and p95 threshold for both owner and branch-scoped reports.
- UAT in `phase-05-uat.md` passes for owner, branch manager, waiter/kitchen, and finance roles.
- Engineering, support, finance operations, and restaurant onboarding accept the API guide (`docs/3_api.md`), architecture/data flows (`docs/2_flows_and_project_structure.md`), runbooks, administrator UI, access ownership, and support responsibilities.

## Outstanding-risk register

Track risk, affected component/tenant, probability, impact, mitigation, owner, due date, and acceptance decision. External items intentionally deferred from this implementation are production TLS/certificates, managed backup scheduling, monitoring/alert destinations, provider production credentials, container-registry scanning, and assigned on-call personnel.
