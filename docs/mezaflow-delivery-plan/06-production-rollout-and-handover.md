# Phase 06 — Production Rollout and Handover

## Objective

Launch safely, prove operational readiness, and transfer ownership with clear documentation.

## Work

1. Create separate development, staging, sandbox, pilot, and production configurations with isolated credentials and databases.
2. Build CI/CD with automated tests, static checks, container image scanning, migration review, staged deployment, and rollback procedures.
3. Deploy production infrastructure with HTTPS, Nginx/reverse-proxy controls, Docker orchestration, managed PostgreSQL backups, Redis, secure secrets, and least-privilege service accounts.
4. Add observability: structured logs, metrics, traces/correlation IDs, payment and settlement alerts, uptime checks, and on-call escalation paths.
5. Execute load, security, webhook resilience, backup restore, and disaster-recovery tests. Include simulated provider outage and duplicate callback scenarios.
6. Run a limited pilot with selected restaurants, daily reconciliation, a defined support window, and go/no-go review before wider release.
7. Complete handover sessions for engineering, support, finance operations, and restaurant onboarding teams.

## Deliverables

- Deployment architecture, infrastructure configuration, CI/CD pipeline, and monitoring dashboards.
- Operations runbooks: incident response, payment dispute, webhook failure, settlement failure, reconciliation, backup restore, and key rotation.
- API documentation, data dictionary, architecture diagrams, and administrator/user guides.
- Pilot report, outstanding-risk register, and production go-live checklist.

## Go-live exit criteria

- Payment, ledger, wallet, and settlement reconciliation passes throughout the pilot period.
- Critical alerts are tested and have assigned responders.
- Backup restore and deployment rollback have been successfully rehearsed.
- Client accepts documentation, access ownership, and support responsibilities.
