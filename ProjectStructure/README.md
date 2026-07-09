# Foodstore Architecture Research

This folder is an implementation-focused map of the current Foodstore monorepo, produced from the source tree, runtime configuration, controllers, services, and database context on 2026-07-10.

## How to use these notes

Read the files in this order when creating a fresh project inspired by this repository:

1. [01-system-overview.md](01-system-overview.md) — boundaries, responsibilities, and runtime topology.
2. [02-backend-clean-architecture.md](02-backend-clean-architecture.md) — the .NET API’s layers and extension points.
3. [03-client-applications.md](03-client-applications.md) — POS/KDS, admin, and landing-site structures.
4. [04-data-domain-and-integrations.md](04-data-domain-and-integrations.md) — domain model, storage, events, and integrations.
5. [05-fresh-project-blueprint.md](05-fresh-project-blueprint.md) — an intentional starting structure and implementation sequence.
6. [06-observations-and-risks.md](06-observations-and-risks.md) — gaps and inconsistencies to resolve before treating this as a template.

## Scope and evidence

This analysis covers the root Docker Compose file, all four deployable applications, API composition/DI, the EF Core `StoreDbContext`, controllers, client API layers, routes, package manifests, environment template, and root documentation. Generated build outputs and dependency folders were excluded.

The documents distinguish between **current implementation** and **documented intent** whenever they conflict. They are architecture notes, not a claim that every endpoint or feature has been integration-tested.
