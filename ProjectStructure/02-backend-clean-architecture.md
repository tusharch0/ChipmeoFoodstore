# 2. Backend Clean Architecture

## Physical structure

```text
foodstore-api/
├── FoodstoreApi.slnx
├── FoodstoreApi.Core/
│   ├── Entities/                    # Business records and navigation properties
│   ├── Entities/Identity/           # ApplicationUser, ApplicationRole
│   ├── Constants/                   # Permissions, order status, cache keys
│   └── Configuration/               # JwtSettings
├── FoodstoreApi.Usecase/
│   ├── DTOs/<Feature>/              # Requests and response shapes
│   ├── Interfaces/                  # Service + repository ports
│   ├── Services/                    # Use cases and provider implementations
│   ├── Utils/                       # Username/cache helpers
│   └── Extensions/                  # Use case DI registration
├── FoodstoreApi.Infrastructure/
│   ├── Data/                        # StoreDbContext + audit interceptor
│   ├── Repositories/                # EF Core adapters
│   ├── Handlers/                    # Media/S3 adapter
│   └── Extensions/                  # Infrastructure DI registration
└── FoodstoreApi.Web/
    ├── Controllers/                 # HTTP boundary per feature
    ├── Authorization/               # Dynamic permission policies
    ├── ApiResponse/                 # Standard response envelope
    ├── Middleware/                  # Exceptions + security headers
    ├── Hubs/                        # AppHub SignalR boundary
    ├── Seed/                        # Startup seed data
    ├── Conventions/                 # `/v2` route prefix
    └── Program.cs                   # Composition root
```

## Dependency direction and responsibilities

| Layer | May know about | Must not contain |
|---|---|---|
| Core | .NET primitives and entity contracts | HTTP, EF mapping attributes, repository implementations |
| Usecase | Core and abstractions | `HttpContext`, controller details, EF queries |
| Infrastructure | Core/Usecase contracts and external SDKs | HTTP policy decisions |
| Web | Usecase services and cross-cutting infrastructure setup | Transaction/business rules beyond request handling |

The implementation does not use a generic repository/unit-of-work abstraction. Instead, each feature has a dedicated repository interface in Usecase and a matching EF implementation in Infrastructure. This gives each aggregate a tailored query surface but creates one interface/repository/service set per feature.

## Composition root: `Program.cs`

Startup is the most important backend integration point. It:

1. Adds controllers with camel-cased JSON, enum serialization, and the versioning convention.
2. Registers FluentValidation from the Web assembly, Swagger, SignalR, and `HttpContextAccessor`.
3. Creates `StoreDbContext` with Npgsql and `AuditSaveChangesInterceptor`.
4. Configures distributed Redis cache with `foodstore:` prefix.
5. Creates an S3 client for RustFS from `S3:*` configuration.
6. Registers Usecase and Infrastructure service collections.
7. Adds ASP.NET Identity, JWT bearer auth, dynamic permission authorization, CORS, and two fixed-window rate-limit policies.
8. On startup, creates a Vietnamese ICU collation, runs EF migrations, and runs seed logic.
9. Configures middleware in this order: exception handling, security headers, CORS, rate limiting, authentication, authorization, controllers, then hub mapping.

### Consequences for a new project

* Do not hide database migration or default data provisioning in application startup in production without an explicit deployment policy. It is convenient locally but makes every API replica capable of schema mutation.
* Keep S3 and Redis setup outside feature services; features should use focused ports such as `IMediaService` or cache helpers.
* Dynamic permission policies are a good fit when permission codes come from data/claims, but they need thorough authorization tests.

## HTTP boundary

The Web project has controllers for auth, POS, kitchen, CRUD administration, media, reporting, CMS, CRM, and e-invoicing. Controllers call a service and normally return `ApiResult.Success`, `ApiResult.Paged`, errors, or a `CreatedAtAction` response. The global exception middleware returns the same general envelope for unhandled errors.

Important current route families:

| Family | External route prefix |
|---|---|
| Auth and customer self-service | `/v2/api/auth`, `/v2/api/customers` |
| POS | `/v2/api/pos` |
| Administrative CRUD | `/v2/api/admin/*` |
| Reports | `/v2/api/reports` |
| Blog/CMS as implemented | `/v2/api/blog`, `/v2/api/cms` |
| E-invoice | `/v2/api/admin/e-invoice` |
| SignalR | `/hubs/app` (outside `/v2`) |

## A feature slice

The expected vertical flow is:

```text
OrdersController
  → IOrderService / OrderService
  → IOrderRepository / OrderRepository
  → StoreDbContext
  → PostgreSQL
  ↘ IHubContext<AppHub> broadcasts an order event at the HTTP boundary
```

DTOs are grouped by feature under `Usecase/DTOs`. Entities remain mutable EF-friendly POCOs in Core. Persistence mapping is centralized in the single, large `StoreDbContext.OnModelCreating` method rather than separate configuration files.

## Extension points worth retaining

* `IEInvoiceProvider` + factory: provider type selects `MisaProvider` or `ViettelProvider`; this is the clearest example of a replaceable external-provider boundary.
* `IMediaService`: abstracts S3/RustFS upload and media operations from controllers.
* `AuditSaveChangesInterceptor`: applies auditing to entities implementing `IAuditableEntity`.
* `RequirePermissionAttribute`, `PermissionPolicyProvider`, and `PermissionAuthorizationHandler`: translate symbolic permissions into ASP.NET authorization.

## Recommended refinements for a fresh implementation

Use one feature folder per layer (`Orders`, `Catalog`, `Identity`, etc.) rather than one global `Services` and `Interfaces` folder. Keep the same inward dependency rule, but give each feature an application command/query surface and a test project. Split EF configuration by entity once the context grows beyond a few aggregates.
