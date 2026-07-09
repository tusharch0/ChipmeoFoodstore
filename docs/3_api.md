# API Reference v2

> RESTful API với response envelope chuẩn. Tất cả routes đều có prefix `/v2/`.

## Base URLs

| Environment | Base URL |
|---|---|
| **Docker (Traefik)** | `http://api.localhost/v2` |
| **Local dev** | `http://localhost:5142/v2` |

## Response Envelope

```json
// ✅ Success — single resource (HTTP 200)
{
  "data": { "id": "a1b2c3d4-...", "name": "Alice", "createdAt": "2026-01-15T10:30:00Z" },
  "error": null,
  "meta": { "requestId": "...", "timestamp": "2026-01-15T10:30:00Z" }
}

// ✅ Success — collection (HTTP 200)
{
  "data": [ ... ],
  "error": null,
  "meta": { "page": 1, "pageSize": 20, "totalCount": 87, "totalPages": 5 }
}

// ✅ Error (HTTP 4xx/5xx)
{
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Username and password are required",
    "details": [{ "field": "username", "message": "must not be empty" }]
  },
  "meta": { "requestId": "...", "timestamp": "..." }
}
```

### HTTP Status Codes
| Code | Meaning |
|---|---|
| 200 | Success |
| 201 | Created (POST) |
| 204 | No Content (DELETE) |
| 400 | Bad Request / Validation Error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 429 | Too Many Requests |
| 500 | Internal Server Error |

### Error Codes
| Code | Description |
|---|---|
| `VALIDATION_ERROR` | Input validation failed |
| `NOT_FOUND` | Resource not found |
| `INTERNAL_ERROR` | Server error |
| `UNAUTHORIZED` | Authentication required |
| `FORBIDDEN` | Insufficient permissions |

---

## Authentication

### Employee Login
```
POST /v2/api/auth/login
Content-Type: application/json

{ "username": "root", "password": "abc123" }
```

### Customer Login
```
POST /v2/api/customers/login
Content-Type: application/json

{ "username": "nguyenvana", "password": "customerpass" }
```

### Customer Registration
```
POST /v2/api/customers/register
Content-Type: application/json

{
  "username": "nguyenvana",
  "name": "Nguyen Van A",
  "email": "customer@example.com",
  "phone": "0901234567",
  "birthday": "1990-06-15",
  "password": "customerpass"
}
```

### Authentication Header
```
Authorization: Bearer <jwt_token>
```

---

## Health
| Method | Route | Description |
|---|---|---|
| `GET` | `/v2/api/health` | Server health check |

---

## POS Endpoints
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/pos/menu` | Full menu (categories + items + addons + combos + discounts) | Employee |
| `GET` | `/v2/api/pos/sources` | Active order sources/tables | Employee |
| `GET` | `/v2/api/pos/addons` | All add-ons | Employee |
| `POST` | `/v2/api/pos/orders` | Create new order | Employee |
| `PUT` | `/v2/api/pos/orders/{id}/status` | Update order status | Employee |
| `POST` | `/v2/api/pos/orders/{id}/payment` | Process payment | Employee |

---

## Kitchen Endpoints
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/kitchen/orders` | Get kitchen order queue | Employee |
| `PUT` | `/v2/api/kitchen/orders/{id}/start` | Start preparing | Employee |
| `PUT` | `/v2/api/kitchen/orders/{id}/complete` | Complete → served | Employee |

---

## Admin Endpoints

### Resources (CRUD)
| Resource | Route Prefix | Permission |
|---|---|---|
| Categories | `/v2/api/admin/categories` | `category.*` |
| Menu Items | `/v2/api/admin/menuitems` | `menu.*` |
| Addons | `/v2/api/admin/addons` | `addon.*` |
| Combos | `/v2/api/admin/combos` | `menu.*` |
| Discounts | `/v2/api/admin/discounts` | `discount.*` |
| Sources (tables) | `/v2/api/admin/sources` | `source.*` |
| Orders | `/v2/api/admin/orders` | `order.*` |
| Employees | `/v2/api/admin/employees` | `employee.*` |
| Roles | `/v2/api/admin/roles` | `role.*` |
| Role Permissions | `/v2/api/admin/roles/{roleId}/permissions` | `role.*` |
| Permissions | `/v2/api/admin/permissions` | `role.*` |
| Payment Settings | `/v2/api/admin/payment-settings` | `payment.*` |
| Customers | `/v2/api/admin/customers` | `customer.*` |

### Special Admin Endpoints
| Method | Route | Description |
|---|---|---|
| `GET` | `/v2/api/admin/dashboard/stats` | KPIs (revenue, orders, customers) |
| `GET` | `/v2/api/admin/dashboard/analytics` | Time-series data |
| `GET` | `/v2/api/admin/dashboard/forecast` | Sales forecast (7 days) |
| `GET` | `/v2/api/admin/dashboard/recommendations` | Combo recommendations |
| `GET` | `/v2/api/admin/orders/paged?page=1&pageSize=20` | Paginated orders |
| `GET` | `/v2/api/admin/orders/status/{status}` | Orders by status |
| `PUT` | `/v2/api/admin/orders/{id}/set-unpaid` | Revert payment |
| `GET` | `/v2/api/admin/orders/date-range?fromDate=...&toDate=...` | Orders by date range |

---

## CMS — Blog Posts
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/blog` | List all posts (filter by status, categorySlug, tagSlug) | Employee |
| `GET` | `/v2/api/blog/{id:guid}` | Get post by ID | Employee |
| `GET` | `/v2/api/blog/slug/{slug}` | Get post by slug | None |
| `POST` | `/v2/api/blog` | Create post | `blog.create` |
| `PUT` | `/v2/api/blog/{id:guid}` | Update post | `blog.update` |
| `DELETE` | `/v2/api/blog/{id:guid}` | Delete post | `blog.delete` |
| `PATCH` | `/v2/api/blog/{id:guid}/status` | Change post status | `blog.update` |
| `PATCH` | `/v2/api/blog/{id:guid}/schedule` | Schedule post | `blog.update` |
| `POST` | `/v2/api/blog/{slug}/view` | Increment view count | None |
| `GET` | `/v2/api/blog/dashboard/stats` | CMS dashboard stats | `dashboard.view` |
| `GET` | `/v2/api/blog/featured?limit=5` | Featured posts | None |
| `GET` | `/v2/api/blog/published?page=1&limit=10` | Paginated published posts | None |

### Blog Revisions
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/blog/{id:guid}/revisions` | List revisions | None |
| `POST` | `/v2/api/blog/{id:guid}/revisions` | Create revision snapshot | `blog.update` |
| `POST` | `/v2/api/blog/revisions/{revisionId:guid}/restore` | Restore revision | `blog.update` |

### Blog Categories
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/admin/blog-categories` | List all | `blog.view` |
| `POST` | `/v2/api/admin/blog-categories` | Create | `blog.create` |
| `PUT` | `/v2/api/admin/blog-categories/{id:guid}` | Update | `blog.update` |
| `DELETE` | `/v2/api/admin/blog-categories/{id:guid}` | Delete | `blog.delete` |

### Blog Tags
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/tags` | List all | None |
| `GET` | `/v2/api/tags/{id}` | Get by ID | None |
| `GET` | `/v2/api/tags/slug/{slug}` | Get by slug | None |
| `POST` | `/v2/api/tags` | Create | Employee |
| `PUT` | `/v2/api/tags/{id}` | Update | Employee |
| `DELETE` | `/v2/api/tags/{id}` | Delete | Employee |

### Blog Settings (key-value)
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/admin/blog-settings` | List all settings | `settings.view` |
| `GET` | `/v2/api/admin/blog-settings/{key}` | Get by key | `settings.view` |
| `PUT` | `/v2/api/admin/blog-settings/{key}` | Upsert setting | `settings.manage` |
| `DELETE` | `/v2/api/admin/blog-settings/{key}` | Delete setting | `settings.manage` |

### Blog Blocks (content blocks)
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/admin/blog-blocks/post/{postId:guid}` | List blocks for post | `blog.view` |
| `POST` | `/v2/api/admin/blog-blocks` | Create block | `blog.create` |
| `PUT` | `/v2/api/admin/blog-blocks/{blockId:guid}` | Update block | `blog.update` |
| `DELETE` | `/v2/api/admin/blog-blocks/{blockId:guid}` | Delete block | `blog.delete` |
| `PUT` | `/v2/api/admin/blog-blocks/reorder` | Reorder blocks | `blog.update` |

### CMS Public API (for Astro landing page)
| Method | Route | Description |
|---|---|---|
| `GET` | `/v2/api/public/articles` | List published articles (paginated) |
| `GET` | `/v2/api/public/articles/{slug}` | Get article by slug |
| `GET` | `/v2/api/public/featured` | Get featured articles |
| `GET` | `/v2/api/public/categories` | List categories |
| `GET` | `/v2/api/public/tags` | List tags |
| `POST` | `/v2/api/public/articles/{slug}/view` | Increment view count |

---

## CRM — Customers
| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/v2/api/customers/register` | Register (`{ username, name, email, phone, birthday?, password }`) | None |
| `POST` | `/v2/api/customers/login` | Login | None |
| `GET` | `/v2/api/customers/me` | Get profile | Customer |
| `PUT` | `/v2/api/customers/me` | Update profile | Customer |
| `GET` | `/v2/api/customers/lookup/{phone}` | POS phone lookup | Employee |
| `GET` | `/v2/api/customers` | List all (admin) | Employee |
| `GET` | `/v2/api/customers/{id}` | Get by ID | Employee |
| `POST` | `/v2/api/customers` | Create (admin) | Employee |
| `PUT` | `/v2/api/customers/{id}` | Update | Employee |
| `DELETE` | `/v2/api/customers/{id}` | Delete | Employee |
| `POST` | `/v2/api/admin/customers/{id}/add-points` | Add points + reason | `customer.edit` |
| `GET` | `/v2/api/admin/customers/{id}/orders` | Customer order history | `customer.view` |
| `GET` | `/v2/api/admin/customers/birthdays` | Upcoming birthdays (this week/month) | `customer.view` |
| `GET` | `/v2/api/admin/customers/leaderboard` | Points leaderboard | Employee |

---

## Reports
| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/v2/api/reports/dashboard-stats` | Dashboard stats | Employee |

---

## Media
| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/v2/api/media/upload` | Upload file (multipart) | Employee |
| `GET` | `/v2/api/media` | List media (paginated) | Employee |
| `DELETE` | `/v2/api/media/{id}` | Delete media | Employee |
| `GET` | `/v2/api/media/check-usage` | Check URL usage | Employee |
| `GET` | `/v2/api/media/unused` | Find unused media | Employee |
| `DELETE` | `/v2/api/media/cleanup` | Bulk delete unused | Employee |

### Media Upload
```
POST /v2/api/media/upload
Content-Type: multipart/form-data
Authorization: Bearer <token>

file: <binary>
folder: "menu-items" | "categories" | "combos" | "blog" | "avatars" | "misc"
```

---

## E-Invoice

> Hệ thống hóa đơn điện tử với multi-provider (Viettel, MISA). Tất cả endpoints yêu cầu permission `einvoice.*`.

**Base**: `/api/admin/e-invoice`

### Provider Management
| Method | Endpoint | Permission | Description |
|---|---|---|---|
| `GET` | `/providers` | `einvoice.providers` | List all providers |
| `POST` | `/providers` | `einvoice.providers` | Create provider |
| `PUT` | `/providers/{id}` | `einvoice.providers` | Update provider |
| `DELETE` | `/providers/{id}` | `einvoice.providers` | Delete provider |
| `POST` | `/providers/{id}/test` | `einvoice.providers` | Test provider connection |

### Invoice Operations
| Method | Endpoint | Permission | Description |
|---|---|---|---|
| `GET` | `/invoices` | `einvoice.view` | List invoices (paginated, filterable) |
| `GET` | `/invoices/{id}` | `einvoice.view` | Get invoice details |
| `POST` | `/invoices/issue` | `einvoice.issue` | Issue invoice for an order |
| `POST` | `/invoices/{id}/cancel` | `einvoice.cancel` | Cancel invoice |

### Settings
| Method | Endpoint | Permission | Description |
|---|---|---|---|
| `GET` | `/settings` | `einvoice.settings` | Get global e-invoice settings |
| `PUT` | `/settings` | `einvoice.settings` | Update settings |

### Dashboard
| Method | Endpoint | Permission | Description |
|---|---|---|---|
| `GET` | `/dashboard` | `einvoice.view` | Stats: total/draft/issued/failed/cancelled |

### Invoice Status Values
| Status | Description |
|---|---|
| `draft` | Created, not yet issued |
| `issued` | Successfully issued to provider |
| `failed` | Provider returned error |
| `cancelled` | Invoice cancelled |

---

## SignalR Hub

**Endpoint**: `/hubs/app`
**Protocol**: MessagePack (binary) with JSON fallback

### Server → Client Events
| Event | Payload | Description |
|---|---|---|
| `ReceiveOrderUpdate` | `{ id, status, ... }` | Order changed |
| `ReceiveTableUpdate` | `{ ... }` | Table/source changed |
| `ReceiveNewOrder` | `{ ... }` | New order for kitchen |
| `ReceiveSourceUpdate` | `{ ... }` | Source updated |

---

## Order Status Values
| Status | Description |
|---|---|
| `pending` | Created, awaiting payment |
| `confirmed` | Payment confirmed |
| `preparing` | Kitchen started preparing |
| `ready` | Ready to serve |
| `served` | Delivered to customer |
| `paid` | Payment completed |
| `cancelled` | Order cancelled |

## Payment Methods
| Value | Description |
|---|---|
| `cash` | Cash payment |
| `qr` | VietQR bank transfer |
| `zalopay` | ZaloPay wallet |
| `momo` | Momo wallet |
| `card` | Bank card (POS terminal) |
# Phase 5 multi-branch and finance endpoints

All endpoints below require JWT authentication and the indicated permission. Organization owners and platform operators can select an authorized active branch with the `X-Branch-ID` request header; branch users may select only their claimed branch.

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/v2/api/admin/organizations` | Authorized organizations and branches |
| POST | `/v2/api/admin/organizations/{organizationId}/branches` | Create branch |
| PUT | `/v2/api/admin/organizations/{organizationId}/branches/{branchId}` | Update identity, address, hours, VAT, kitchen route, and state |
| GET/PUT | `/v2/api/admin/organizations/{organizationId}/memberships[/{membershipId}]` | List/update organization roles (owner only) |
| GET | `/v2/api/reports/financial/{organizationId}` | Ledger-backed financial report using Kenyan date boundaries |
| GET | `/v2/api/reports/financial/{organizationId}/export` | Audited CSV report export |
| GET | `/v2/api/reports/financial/{organizationId}/export.pdf` | Audited PDF report export |
| GET | `/v2/api/admin/finance/wallet/{branchId}` | Branch wallet position |
| GET | `/v2/api/admin/finance/ledger/{branchId}` | Branch journal statement |
| GET | `/v2/api/admin/finance/ledger/{branchId}/export` | CSV statement with internal/provider references |
| GET | `/v2/api/admin/finance/operations/exceptions/{organizationId}` | Organization financial exception queue |
| GET | `/v2/api/admin/finance/operations/exceptions/unmatched` | Global unmatched-provider-event queue (platform only) |
| GET | `/v2/api/admin/orders/{orderId}/receipt` | Customer receipt and transaction references |
| GET | `/v2/api/admin/settlements/batch/{batchId}/export` | Settlement CSV with payout, intent, and provider references |

Responses include `X-Correlation-ID`; clients may supply the same header (maximum 128 characters) to correlate logs.
