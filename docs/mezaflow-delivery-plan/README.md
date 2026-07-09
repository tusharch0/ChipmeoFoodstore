# MezaFlow Client Delivery Plan

This plan turns the existing restaurant ordering platform into a multi-tenant restaurant payments and financial-operations platform for Kenya. It is intentionally phased: money movement is introduced only after tenant isolation, payment reliability, and audit controls are in place.

## Guiding delivery principles

- Treat provider callbacks as untrusted and make all payment processing idempotent.
- Use an immutable, balanced double-entry ledger as the financial source of truth; wallet balances are derived or safely materialized from it.
- Isolate every restaurant and branch at the database, API authorization, and reporting layers.
- Run slow or retryable work (webhooks, reconciliation, settlement and notifications) through durable background jobs.
- Release financial features behind flags, first in a provider sandbox and pilot restaurants, with reconciliation before broad rollout.

## Phases

| Phase | Outcome | Delivery gate |
|---|---|---|
| [00 Discovery](00-discovery-and-solution-design.md) | Approved scope, risk register, and target design | Client signs off on money and operational flows |
| [01 Foundation](01-platform-foundation-and-tenancy.md) | Tenant-aware, observable, secure platform | Isolation and security acceptance checks pass |
| [02 Payments](02-intasend-payments.md) | Reliable M-Pesa/IntaSend payment lifecycle | Sandbox payment and webhook reconciliation passes |
| [03 Ledger and Wallets](03-ledger-and-wallets.md) | Auditable balances and restaurant wallets | Ledger invariants and financial test suite pass |
| [04 Settlements](04-settlements-and-reconciliation.md) | Commission deductions and daily settlement workflow | Pilot settlement reconciles to provider data |
| [05 Operations](05-multibranch-and-financial-reporting.md) | Branch control, reporting, and finance operations | Restaurant owner/UAT sign-off |
| [06 Production](06-production-rollout-and-handover.md) | Production launch, monitoring, and handover | Go-live checklist and support transition complete |

## Recommended order

Do not begin wallet withdrawals, automatic payouts, or broad production rollout before phases 02–04 are accepted. QR ordering and kitchen workflows can continue to evolve in parallel only where they do not bypass the payment and ledger rules.
