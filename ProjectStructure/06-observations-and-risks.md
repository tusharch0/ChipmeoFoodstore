# 6. Observations and Risks

These are evidence-based reconciliation notes, not proposed code changes. Resolve them before cloning Foodstore’s structure into a new project.

| Area | Current implementation evidence | Documented/expected state | Implication |
|---|---|---|---|
| Admin auth | `foodstore-admin/package.json` has no Better Auth/Drizzle dependency. `auth-service.ts` stores login JWT in a module variable; `proxy.ts` only checks an `auth_token` cookie. | Docs describe Better Auth, DB sessions, and a BFF. | Choose and implement a single coherent model; current guard/token handling can diverge after refresh. |
| Store auth | `foodstore-store/src/lib/utils/auth.ts` persists JWT to localStorage. | Standards mention in-memory tokens/HttpOnly refresh cookie as preferred. | XSS exposure and inconsistent browser-session strategy across clients. |
| Admin API proxy | Next rewrites browser proxy requests; service modules execute client-side fetches. | Docs call it a server BFF with internal header. | The server is not currently the sole API caller; do not assume tokens are hidden from browser code. |
| CMS public route | `CmsPublicController` is `[Route("api/cms")]`. | API docs list `/api/public/*`. | Generate/verify OpenAPI before consuming public CMS endpoints. |
| Blog settings/blocks routes | Controllers use `api/blog/settings` and `api/blog/{postId}/blocks`. | API docs list admin-prefixed blog settings/blocks. | Route documentation needs an as-built refresh. |
| Landing page | Source contains only basic `index`, layout, welcome component, and no configured integrations. | Root docs describe an SEO CMS landing app and Tailwind. | Treat it as scaffolding, not a finished consumer of CMS data. |
| Configuration hygiene | Checked-in `appsettings.json` has a concrete localhost DB credential-shaped value while secrets use placeholders elsewhere. | Team standards require placeholders/no hard-coded secrets. | Replace with placeholders and provide an environment-specific local override. |
| Startup migration | API calls `MigrateAsync()` and seed logic during host start. | Not explicitly framed as a deployment choice. | Multiple production replicas could contend for schema/seed work; run migrations through a release job. |
| Database mapping | All mappings reside in one large `StoreDbContext.OnModelCreating`. | Standards illustrate per-entity configurations. | Split configuration files as domain grows to reduce merge conflicts and improve reviewability. |
| Documentation naming | Historical docs reference `Store`, `LandingPage`, `MediaStorageManagement`, and a separate media service. | Current root has `foodstore-store`, `foodstore-landingpage`; RustFS is embedded in Compose and no `MediaStorageManagement` project exists. | Use actual repository names/configuration as the source of truth. |

## Decisions to make before scaffolding

1. Do you need both a POS client and a separate admin client, or can one meet the operating model?
2. Which authentication boundary is canonical: cookie session/BFF, JWT + refresh cookie, or device-specific JWT? Avoid mixed models.
3. Will the public site consume CMS content at build time, request time, or through a cache/CDN?
4. Which order states are legal, who may transition them, and should payment be independent of kitchen completion?
5. Is multi-tenant/multi-branch support a future requirement? If yes, add outlet/tenant boundaries before data grows.

## Recommended source-of-truth order

For implementation decisions, trust in this order: current source code and generated OpenAPI → automated tests → Compose/environment configuration → architecture documents → README feature claims. This prevents stale planning documentation from silently dictating a new project’s architecture.
