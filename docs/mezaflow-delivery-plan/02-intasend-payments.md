# Phase 02 — IntaSend Payments

## Objective

Deliver a reliable, observable customer payment flow for M-Pesa through IntaSend.

## Work

1. Create a payment-intent model linked to an order, tenant, branch, amount, currency, provider reference, idempotency key, and lifecycle state.
2. Implement provider-adapter operations: create payment request, retrieve status, validate webhook/callback, issue refund where supported, and obtain reconciliation data.
3. Implement a strict payment state machine, for example: `created → pending → succeeded | failed | expired | cancelled`, with compensating states for refund processing.
4. Accept webhooks through a dedicated endpoint that validates signature, timestamp, replay protection, amount/currency, tenant ownership, and provider reference before queueing processing.
5. Make commands and webhook events idempotent. Store provider event IDs and processing outcomes so duplicates cannot charge, confirm, or credit an order twice.
6. Update customer, waiter, kitchen, and admin experiences with clear payment-pending, success, failure, timeout, and retry states. Kitchen release rules must be explicitly agreed.
7. Add a finance support console for searching a payment by order ID, phone/reference, provider reference, status, and webhook history.
8. Test against IntaSend sandbox and controlled live pilot credentials, including duplicate/out-of-order callback simulations.

## Deliverables

- IntaSend adapter and encrypted configuration per organization/branch as applicable.
- Webhook endpoint, event store, retry worker, and payment support tools.
- Payment-flow UI states and customer receipt/reference display.
- Automated unit, integration, and end-to-end payment tests.

## Acceptance criteria

- Payment success is confirmed only from trusted provider status/callback data.
- Replayed callbacks and repeated customer submissions do not create duplicate financial effects.
- Each successful order payment can be traced from customer order to provider reference and internal audit record.
