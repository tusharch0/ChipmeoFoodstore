# Phase 05 — Multi-branch Operations and Financial Reporting

## Objective

Give restaurant operators clear operational and financial control across branches.

## Work

1. Add branch management: branch identity, addresses, opening hours, QR sources/tables, tax settings, payment configuration, menus/pricing, and kitchen routing.
2. Support role assignment at organization and branch levels, with least-privilege defaults.
3. Build owner-level and branch-level dashboards for orders, payment conversion, gross sales, net sales, refunds, commissions, wallet position, pending settlement, and settlement history.
4. Provide configurable reporting periods using Kenyan time zone, correct day cut-offs, CSV/PDF exports, and report access auditing.
5. Add operational exception queues: unmatched payments, payment failures, refunds awaiting action, negative/held wallet state, settlement failures, and reconciliation discrepancies.
6. Add customer receipts, restaurant statements, and finance exports with stable references to internal and provider transaction IDs.
7. Validate performance for realistic restaurant/branch volume and ensure all reports remain tenant isolated.

## Deliverables

- Multi-branch administration interface and APIs.
- Revenue, wallet, settlement, and exception reporting.
- UAT scripts for owners, branch managers, waiters, and finance teams.

## Acceptance criteria

- An owner can view consolidated figures while a branch user sees only authorized branch data.
- Report totals agree with the ledger and settlement statements for the selected period.
