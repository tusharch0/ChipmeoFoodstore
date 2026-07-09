# Phase 01 — Platform Foundation and Tenancy

## Objective

Make the platform safe to operate for multiple independent restaurants and branches.

## Work

1. Introduce core tenancy entities: `Organization` (restaurant business), `Branch`, `Membership`, and branch-aware staff assignments.
2. Add a tenant context resolved from authenticated identity and enforce it in every API query, command, background job, cache key, export, and real-time event.
3. Define platform and restaurant RBAC permissions, including separation of operational, finance, support, and administrative roles.
4. Make orders, menus, QR sources, payment settings, reports, and customer data branch/organization scoped. Decide explicitly where cross-branch visibility is allowed.
5. Establish a provider abstraction so IntaSend is an adapter behind application-level payment interfaces rather than embedded in controllers or UI components.
6. Add durable job processing for webhook handling, retries, reconciliation, settlement generation, and notifications. Redis may support queues/cache, but financial state remains in PostgreSQL.
7. Add platform audit logging for logins, configuration changes, payment actions, refunds, wallet actions, exports, and privileged access.
8. Strengthen operational controls: secret management through environment/managed secrets, HTTPS, rate limits, request correlation IDs, structured logs, health checks, database backups, and alerting.

## Deliverables

- Tenant-aware schema migrations and authorization model.
- Payment and job-processing interfaces.
- Audit event model and admin audit-log view/API.
- Architecture and data-flow documentation.

## Acceptance criteria

- A user from one restaurant cannot read or modify another restaurant’s records by URL, API, cache, real-time event, report, or background task.
- Privileged actions are attributable to a person/system actor and a timestamp.
- Secrets are absent from source control and application logs.
