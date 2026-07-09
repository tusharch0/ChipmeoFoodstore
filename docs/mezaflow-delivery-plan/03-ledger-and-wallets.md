# Phase 03 — Ledger and Restaurant Wallets

## Objective

Create an auditable financial system of record and restaurant wallets without relying on mutable balances alone.

## Work

1. Implement a double-entry ledger with immutable journals, journal lines, account chart, source references, timestamps, actors, and reversal links.
2. Define accounts for customer payment clearing, IntaSend/provider clearing, platform commission revenue, provider fees, restaurant payable/wallet liability, refunds, adjustments, and settlement payable.
3. Require each posted journal to balance: total debits equal total credits in the same currency. Never edit posted entries; correct errors with explicit reversing entries.
4. Implement wallet accounts for restaurant organizations, optionally branch sub-ledgers where the business rules require them. Expose available, pending, and held amounts with their derivation.
5. Post ledger events atomically with payment confirmation, refunds, commissions, fee adjustments, and settlement completion using transactional-outbox/event patterns where needed.
6. Build wallet and ledger APIs: transaction history, statement export, filtered search, balance read model, and finance approval workflows for manual adjustments.
7. Add reconciliation tooling that compares internal payment/ledger totals with provider transactions and flags discrepancies for investigation.
8. Restrict manual financial adjustments through dual approval, reason codes, immutable audit logs, and role-based limits.

## Deliverables

- Ledger schema, posting service, account rules, and financial invariants.
- Restaurant wallet APIs and finance/admin views.
- Reconciliation report and discrepancy workflow.
- Ledger design document with example journal entries.

## Acceptance criteria

- Every monetary event posts a balanced, immutable journal or is rejected.
- Wallet balances reconcile to journal lines and provider-confirmed payment activity.
- Refunds and adjustments are traceable to the original transaction and approval record.
