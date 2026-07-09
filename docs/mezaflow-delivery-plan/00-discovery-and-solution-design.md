# Phase 00 — Discovery and Solution Design

## Objective

Agree the operational and financial rules before altering payment or balance logic.

## Work

1. Map the current customer journey: QR scan, menu, cart, order, payment, kitchen acceptance, fulfilment, cancellation, and refund.
2. Define actors and permissions: MezaFlow platform operator, restaurant owner, branch manager, cashier, waiter, kitchen staff, finance user, and customer.
3. Confirm payment-provider capabilities and commercial rules with IntaSend: supported M-Pesa flows, callback signing, refund support, settlement/payout APIs, fees, limits, sandbox, and webhook retry behaviour.
4. Define the money model in Kenyan shillings: order total, taxes, service charge, platform commission, payment-provider fees, restaurant net amount, refund rules, and settlement cut-off time.
5. Define wallet rules: which entities own wallets, permitted funding sources, withdrawal/payout eligibility, negative-balance policy, holds, reversals, and maker/checker approval thresholds.
6. Produce a data-classification and compliance assessment for customer, payment, tax, and audit data. Confirm eTIMS scope with the client’s tax adviser/provider.
7. Review the present codebase and create a gap assessment against the target architecture, including required migrations and backward-compatibility risks.

## Deliverables

- Signed user journeys and exception flows.
- Architecture decision records for tenancy, payment abstraction, ledger model, job processing, and settlement approach.
- Domain glossary and state-transition diagrams.
- Security threat model and prioritized risk register.
- Delivery estimates, pilot definition, and acceptance criteria per phase.

## Acceptance criteria

- Every payment, refund, cancellation, fee, commission, and settlement scenario has an agreed owner and outcome.
- The client confirms provider credentials, webhook URLs, settlement account ownership, and financial approval roles.
- No money-related development proceeds on undefined business rules.
