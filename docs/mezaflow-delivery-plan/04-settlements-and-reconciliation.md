# Phase 04 — Settlements and Reconciliation

## Objective

Automate transparent daily settlement calculation and controlled restaurant payout operations.

## Work

1. Define configurable commercial terms: flat/percentage commissions, tax treatment, provider fees, promotions funded by MezaFlow or restaurant, minimum payout threshold, and settlement calendar.
2. Build daily settlement batches per restaurant and currency from eligible, reconciled payment/ledger transactions.
3. Produce an immutable settlement statement showing gross sales, refunds, fees, commissions, adjustments, holds, net amount, and transaction-level detail.
4. Implement a settlement state machine: `draft → reviewed → approved → submitted → paid | failed | partially_paid → reconciled`.
5. Integrate automated payout only after provider support, contractual ownership, KYC, and operational controls are confirmed; otherwise support controlled manual settlement recording.
6. Add maker/checker approval, retry policy, payout idempotency, failed payout investigation, and reversal/adjustment procedures.
7. Reconcile settlement batches against actual provider/payout-bank records and retain evidence.
8. Send restaurant notifications and make statements available for download.

## Deliverables

- Commission configuration, settlement generator, approval workflow, and statement export.
- Finance operations dashboard for batches, exceptions, approvals, and reconciliation.
- Settlement runbook for failures, disputes, and manual intervention.

## Acceptance criteria

- A restaurant can explain how each settlement net amount was calculated.
- One eligible transaction appears in at most one finalized settlement.
- Payout and settlement records reconcile without unexplained balance differences.
