# 5. Fresh Project Blueprint

This is a recommended clean starting point based on what is valuable in Foodstore, without copying its accumulated duplication and configuration drift.

## Target monorepo layout

```text
restaurant-platform/
├── apps/
│   ├── api/                         # ASP.NET Core host
│   ├── pos/                         # SvelteKit POS + KDS
│   ├── admin/                       # Next.js internal console
│   └── web/                         # Astro public site
├── backend/
│   ├── Domain/                      # Entities, value objects, domain policies
│   ├── Application/                 # Feature use cases, contracts, ports
│   ├── Infrastructure/              # EF, cache, media, provider adapters
│   └── tests/                       # Unit + integration tests
├── packages/
│   ├── api-contracts/               # Generated OpenAPI client/types or schemas
│   └── design-tokens/               # Optional shared visual tokens
├── deploy/
│   ├── compose/                     # Docker Compose and environment examples
│   └── database/                    # Explicit migrations/bootstrap only
├── docs/
└── README.md
```

Keeping deploy configuration separate makes application directories portable. Use workspace tooling only if shared packages are genuinely needed; otherwise separate lockfiles are acceptable.

## Feature organization

Use feature-first folders in Application and Infrastructure:

```text
Application/
├── Orders/
│   ├── CreateOrder/
│   ├── ProcessPayment/
│   ├── ChangeOrderStatus/
│   ├── Contracts/
│   └── IOrderRepository.cs
├── Catalogue/
├── Identity/
├── Customers/
└── Publishing/
```

This preserves Clean Architecture while colocating commands/queries, validation, response contracts, and tests. It is easier to navigate than a single global DTO/interface/service collection.

## Implementation sequence

1. **Foundation**: Docker Compose for PostgreSQL, Redis, object storage, API; environment validation at startup; CI build/test commands.
2. **Identity and access**: choose one auth model. For browser applications, prefer a BFF/session cookie approach or short-lived access token plus HttpOnly refresh cookie. Define permission vocabulary in code and seed it deterministically.
3. **Catalogue and operations**: categories, products, add-ons, sources, orders, payments, immutable status history. Enforce order transitions in the application layer.
4. **POS/KDS**: API contracts first, then cart, source selection, payment, realtime events, and reconnection behavior. Add offline strategy only when requirements justify it.
5. **Admin**: feature routes and reusable table/sheet workflows; enforce authorization on the API rather than trusting navigation guards.
6. **Media and CMS**: isolated media service/port, then posts and public read models.
7. **Analytics and external providers**: add when there is reliable production data. Keep providers behind interfaces and test them with contract fixtures.

## Contracts and error design

Keep the API envelope if clients benefit from uniform metadata, but define it once in OpenAPI/schema and generate both client types. Return RFC-style validation/problem information consistently. Do not let hand-written endpoint lists and duplicated client types drift apart.

Use explicit request/response types; do not expose EF entities. For pagination, define page number/size constraints and a shared `Page<T>` contract.

## Security baseline

* Keep secrets out of source and validate all mandatory settings on boot.
* Use HTTPS and secure, HttpOnly, `SameSite` cookies in deployed environments.
* Put authorization on every protected command/query, including SignalR hub methods.
* Allow CORS only for known browser origins; never use wildcard CORS with credentials.
* Treat all uploaded media as untrusted: validate content, size, extension, disposition and access policy.
* Add rate limits by risk category (login, writes, uploads), not merely global limits.

## Quality gates

Required early tests: order total calculation, invalid state transition rejection, permission checks, auth refresh/logout, media failure compensation, and controller/API contract integration tests against PostgreSQL. Add browser-level flows for POS payment and kitchen updates once the core API stabilizes.

Run builds per application plus `dotnet test`; do not rely only on Docker image builds to catch type errors.
