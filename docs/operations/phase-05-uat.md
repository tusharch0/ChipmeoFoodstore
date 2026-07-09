# Phase 05 UAT scripts

Run these scenarios in staging with two organizations and at least two branches per organization. Record the tester, date, build SHA, result, and evidence link for every scenario.

## Owner

1. Sign in as an organization owner and open **Branches**. Add a branch, configure address, opening-hours JSON, VAT rate, kitchen route, and active state.
2. Assign employees to different branches and set organization roles. Verify the last active owner cannot be removed.
3. Open **Financial reports**, select all branches, and run a Kenyan-date period. Compare gross, refunds, net, wallet, and pending settlement to ledger journals and settlement statements.
4. Select one branch and verify every total and journal row belongs to that branch. Export report, wallet statement, and settlement CSV; verify internal and provider references are present.
5. Confirm another organization's IDs return `403` or `404` and never disclose data.

## Branch manager

1. Verify the branch user sees only their assigned branch in orders, catalog, sources, wallet, reports, and realtime updates.
2. Create an order and confirm configured VAT is applied. Pay it and verify the kitchen receives the order with the configured kitchen route.
3. Open the receipt and verify order code, payment-intent ID, provider reference, line totals, VAT, and total.
4. Attempt consolidated organization reporting and organization-role changes; verify access is denied.

## Waiter and kitchen

1. Create and update an order from an authorized source. Verify products, discounts, addons, and payment settings come only from the active branch.
2. Confirm a kitchen client in another branch receives no event. Move the order through preparing, ready, and served.
3. Attempt branch administration, finance operations, and exports; verify least-privilege permissions deny access.

## Finance

1. Verify payment failures, refunds awaiting action, negative wallets, settlement failures, and reconciliation discrepancies appear in the exception queue. Platform operators must also see unmatched payments.
2. Create a reconciliation discrepancy, resolve it with evidence, and verify it leaves the active queue.
3. Request a manual adjustment and approve it as a different user. Verify the journal remains balanced and the requester cannot self-approve.
4. Advance a settlement through maker/checker states. Verify a payout reference is required and appears in the exported statement.

## Pass criteria

- No cross-tenant data is visible through API, cache, export, or realtime delivery.
- Report totals reconcile to journal lines and settlement statements for the same Kenyan-date boundaries.
- All privileged actions are audited and all exported transaction rows have stable internal references.
