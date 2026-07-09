# 3. Client Applications

## A. Store client: POS and kitchen display

`foodstore-store` is a SvelteKit Node-adapter application. Its source is deliberately thin: it owns cashier and kitchen interaction, not administrative management.

```text
foodstore-store/src/
├── routes/
│   ├── +page.svelte                 # Employee login/start page
│   ├── +layout.svelte               # Global auth guard and landscape prompt
│   ├── pos/
│   │   ├── +page.svelte             # Cashier screen
│   │   ├── checkout/+page.svelte
│   │   └── store.svelte.ts          # POS view-model/state/actions
│   ├── kitchen/
│   │   ├── +page.svelte             # Kitchen columns
│   │   └── kitchen.svelte.ts        # Kitchen view-model/state/actions
│   ├── logout/+page.svelte
│   └── error/+page.svelte
└── lib/
    ├── api/                         # Auth, POS, orders, customers, fetch envelope
    ├── components/                  # Payment, image cropper, UI primitives
    ├── config/                      # Runtime `PUBLIC_*` configuration and endpoints
    ├── services/signalr.ts          # Hub lifecycle/event subscriptions
    ├── types/                       # API/domain shapes
    └── utils/                       # Cart, auth store, app state, formatting
```

### POS flow

1. The root layout checks client-side auth for `/pos` and `/kitchen`.
2. `POSStore.init()` loads menu/catalogue and establishes its presentation state.
3. Cashier selects source/table, customer, catalogue items, add-ons, combos, and discount.
4. The store sends create/update order data through `posAPI`.
5. It opens `PaymentModal`; payment changes the order state through `/api/pos/orders/{id}/payment`.
6. Client state is cleared and SignalR provides cross-screen updates.

### Kitchen flow

`kitchen.svelte.ts` is the UI model behind three columns: pending, preparing, completed. The visible transitions currently invoke status updates including `preparing` and `served`. It has a realtime connection lifecycle in the view model.

### State and auth reality

The code uses a legacy Svelte writable store for auth, while page view models use Svelte 5 runes in `.svelte.ts` files. The auth store persists token/user data in localStorage. This makes page refresh straightforward but exposes the bearer token to browser script; a fresh design should use an HttpOnly session/refresh-cookie flow instead.

## B. Admin client: management console

`foodstore-admin` uses Next.js App Router, React 19, Tailwind 4, shadcn-style components, TanStack Table/Query, TipTap, Recharts, and SignalR.

```text
foodstore-admin/src/
├── app/
│   ├── layout.tsx                   # Root providers/styles
│   ├── page.tsx                     # Entry redirect/page
│   ├── login/page.tsx
│   └── admin/
│       ├── layout.tsx               # Admin shell
│       ├── food/                    # Products, catalogue, orders, payments, e-invoice
│       ├── cms/                     # Posts, tags, categories, settings
│       ├── crm/                     # Customers and leaderboard
│       └── employees/               # Employees, roles, permissions
├── components/
│   ├── ui/                          # Generated/adapted shadcn primitives
│   ├── app-sidebar.tsx
│   ├── data-table.tsx
│   ├── crud-sheet.tsx
│   └── editor/tiptap.tsx
├── lib/
│   ├── api-client.ts                # `/api/proxy` fetch adapter
│   ├── auth-service.ts              # Login and in-memory JWT
│   ├── auth-context.tsx
│   ├── permissions.ts
│   ├── services/                    # One service module per API feature
│   └── types/
└── proxy.ts                         # Redirect guard for /admin without auth_token cookie
```

### API proxy

`next.config.ts` rewrites `/api/proxy/:path*` to `${API_PROXY_URL}/api/:path*`; in Docker the build argument points at the internal API base including `/v2`. SignalR uses a special `/api/proxy/hubs/:path*` rewrite because the hub is not version-prefixed.

The service modules call the browser-relative proxy, deserialize API envelopes, and attach the token returned by login when one is present. This provides a same-origin API surface but is not yet a full server-side BFF.

### UI patterns

Pages group into CMS, CRM, employees, and food operations. Reusable operational widgets are intentionally shared: `DataTable` for lists, `CrudSheet` for mutation forms, `ConfirmDialog` for destructive actions, `AppSidebar` for navigation, and TipTap for rich content. A fresh project should retain these reusable interaction patterns, but place feature-specific UI under a feature directory instead of accumulating page-level logic.

## C. Landing page

The Astro application is currently minimal:

```text
foodstore-landingpage/src/
├── pages/index.astro
├── layouts/Layout.astro
├── components/Welcome.astro
└── assets/
```

It is a clean public-site boundary but does not yet implement the documented CMS integration or Tailwind setup. Treat it as a starter scaffold, not as a completed marketing/CMS front end.

## Client design rule for a fresh project

Keep three user surfaces separate because their delivery, security, and performance needs differ:

* POS/KDS: highly interactive, resilient, realtime, touch-friendly.
* Admin: permission-aware, CRUD-heavy, internal only, server-first where possible.
* Public site: SEO-first, cacheable, little client JavaScript.

Share API schemas through generated clients or an explicit contracts package—not by copying TypeScript types between these three applications.
