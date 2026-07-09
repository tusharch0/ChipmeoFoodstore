# Production operations runbooks

## Incident response

1. Acknowledge the alert, assign an incident commander, record start time, affected tenants, correlation IDs, and deployment SHA.
2. Classify severity. Stop deployments for payment, ledger, tenant-isolation, or data-loss incidents.
3. Use structured logs and `X-Correlation-ID` to trace the request. Preserve evidence before remediation.
4. Mitigate with a feature/config rollback or previous application image. Never reverse a database migration without a reviewed recovery plan.
5. Reconcile affected payments and journals, communicate status, then document cause, impact, remediation, and follow-up owner.

## Payment dispute or webhook failure

1. Search by provider reference, payment-intent ID, order code, and phone. Do not change payment state based only on a customer screenshot.
2. Compare immutable payment events with provider status. Replay the signed callback or allow the poller to reconcile it; duplicate callbacks must remain idempotent.
3. If no intent matches, leave the event in the unmatched queue for a platform operator. Never attach it without verified order, amount, currency, and provider reference.
4. Refund only through the approved refund flow and verify the refund journal is posted once.

## Settlement failure and reconciliation

1. Freeze the affected batch and record its ID, payout reference, period, currency, and failure reason.
2. Compare settlement lines to payment intents and ledger journals. Create a reconciliation case for every discrepancy.
3. Retry submission only after the cause is corrected. Require a distinct maker and checker, and a stable payout reference for paid status.
4. Export and archive the final statement; verify wallet debits equal the paid net amount.

## Backup restore

1. Confirm backup timestamp, checksum, encryption ownership, and target isolated environment.
2. Restore PostgreSQL, apply only migrations newer than the backup, then verify row counts, tenant filters, ledger balance, and latest settlement state.
3. Validate object-storage references and Redis can be rebuilt. Run health, authentication, order, payment-callback replay, report, and export smoke tests.
4. Record restore duration and evidence. Production cutover requires incident/change approval.

## Key rotation

1. Create the new JWT, database, S3, provider, or webhook secret in the environment secret store.
2. Deploy consumers that accept the overlap where supported, switch producers, verify health and callbacks, then revoke the old key.
3. Never place the secret in source, logs, tickets, screenshots, or chat. Record only secret identifier, owner, and rotation date.

## Provider outage

1. Keep orders pending and surface provider unavailability; do not mark a payment successful optimistically.
2. Preserve queued events and poll attempts with bounded retry/backoff. Monitor queue age and pending amount.
3. When service returns, reconcile provider state, process events idempotently, and compare payment, ledger, wallet, and settlement totals.
