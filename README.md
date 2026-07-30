# SaaS Inventory Management

A **multi-tenant SaaS Inventory Management System** built with **ASP.NET Core** that enables multiple organizations (tenants) to securely manage their inventory, warehouses, suppliers, customers, purchases, sales, and stock operations from a single application.

This project is designed using **enterprise-level architecture and best practices** to simulate a real-world SaaS business application.

> 🚧 **Project Status:** Under Active Development

---

# Features

## Enterprise Features ⭐

* Multi-Tenant SaaS Architecture ✅ (core isolation layer implemented)
* Tenant Isolation — read-side (EF Core global query filters) ✅
* Tenant Isolation — write-side (INSERT/UPDATE guards) ✅
* Tenant Registration ⏳
* Tenant-Based Data Filtering ✅
* ASP.NET Core Identity ✅
* Cookie Authentication ✅
* Role-Based Authorization ⏳
* Database-Driven Permission-Based Authorization ⏳
* Audit Logging ⏳
* Background Jobs (Hangfire) ⏳

## Inventory Management

* Product Management
* Category Management
* Unit Management
* Warehouse Management
* Inventory Tracking
* Stock Movement History

## Sales & Purchasing

* Supplier Management
* Customer Management
* Purchase Orders
* Sales Orders
* Purchase Returns
* Sales Returns

## Stock Operations

* Stock Transfer
* Stock Adjustment
* Low Stock Alerts

## Dashboard & Reporting

* Dashboard
* Business Reports
* Notifications

---

# Multi-Tenant Architecture

This application uses a **Shared Database, Shared Schema** multi-tenancy model — all tenants share one database, isolated via tenant-scoped EF Core global query filters, write guards, and claims-based tenant resolution.

Implemented so far:

* `Tenant` model and `ITenantOwned` interface for marking tenant-scoped entities
* `TenantProvider` — resolves and caches the current request's `TenantId` / SuperAdmin status from claims
* `TenantClaimsPrincipalFactory` — stamps a `tenant_id` claim onto the auth cookie at login
* `TenantMiddleware` — rejects authenticated requests with no resolvable tenant (403)
* Reflection-based automatic global query filters on every `ITenantOwned` entity
* `ChangeTracker` write guards — enforce `TenantId` on insert, block cross-tenant `TenantId` changes on update
* Startup-time validation (`EnsureNoUnprotectedTenantEntities`) that fails fast if an entity has a `TenantId` property but doesn't implement `ITenantOwned`

📄 See [`ARCHITECTURE.md`](ARCHITECTURE.md) for the full design — request pipeline diagram, `ITenantOwned` pattern, and tenant write-guard rules.

---

# Architecture

This project follows **Clean Architecture** with enterprise design patterns to ensure scalability, maintainability, and testability.

* Clean Architecture
* Repository Pattern
* Generic Repository
* Specific Repository
* Unit of Work Pattern
* Dependency Injection
* Service Layer
* DTO Pattern
* Middleware Pipeline
* Global Exception Handling
* FluentValidation

---

# Tech Stack

## Backend

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity

## Authentication

* Cookie Authentication

## Frontend

* Razor Views
* jQuery
* DevExtreme

## Background Processing

* Hangfire

## Logging

* ASP.NET Core Logging (Serilog)
* Audit Logging

---

# Project Structure
SaaSInventoryManagement
│
├── Controllers/
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Extensions/                                    # Moved to root level
│   ├── ModelBuilderTenantExtensions.cs            # applies global tenant query filters,
│   │                                            # validates no unprotected TenantId entities
│   └── ChangeTrackerTenantExtensions.cs          # enforces tenant write guards on insert/update
│
├── Middleware/
│   └── TenantMiddleware.cs                       # blocks requests with no resolvable tenant
│
├── Models/
│   ├── Base/
│   │   └── ITenantOwned.cs                       # marker interface for tenant-scoped entities
│   ├── Identity/
│   │   └── ApplicationUser.cs                    # extends IdentityUser with FirstName/LastName/TenantId
│   └── Tenant.cs                                 # tenant (company) entity
│
├── Services/
│   ├── Interfaces_/
│   │   └── ITenantProvider.cs                    # contract for resolving current TenantId / SuperAdmin
│   ├── TenantProvider.cs                         # reads & caches tenant_id claim per request
│   └── TenantClaimsPrincipalFactory.cs           # stamps tenant_id claim onto auth cookie at login
│
├── Migrations/
├── Views/
├── wwwroot/                           # full multi-tenancy design doc
└── Program.cs

---

# Planned Features

The following features will be added in future releases:

* Activity Logging
* Online Payment Integration
* Multi-language Support
* REST API
* JWT Authentication

---

# Getting Started

## Prerequisites

* .NET 9 SDK (or your target version)
* SQL Server
* Visual Studio 2022

## Installation

Clone the repository:

```bash
git clone https://github.com/Basant948/SaaSInventoryManagement.git
```

Navigate to the project:

```bash
cd SaaSInventoryManagement
```

Update your database connection string inside:

```text
appsettings.json
```

Run Entity Framework migrations:

```bash
Update-Database
```

Run the application:

```bash
dotnet run
```

---

# Roadmap

* ✅ Multi-Tenant SaaS Architecture (core isolation layer)
* ✅ Authentication (ASP.NET Core Identity + cookie auth)
* ⏳ Role-Based & Permission-Based Authorization
* ⏳ Inventory Management
* ⏳ Sales & Purchasing
* ⏳ Stock Operations
* ⏳ Dashboard & Reports
* ⏳ Background Jobs
* ⏳ SignalR Real-Time Notifications
* ⏳ QR Code Payment Integration
* ⏳ Online Payment Integration
* ⏳ REST API
* ⏳ JWT Authentication
* ⏳ Export to Excel & PDF
* ⏳ Business Dashboard Analytics

---

# Author

**Basant Ritu Rajbanshi**

Backend Developer (.NET)

GitHub: https://github.com/Basant948

---

# License

This project is built for learning, portfolio, and professional development purposes.