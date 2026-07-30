# Multi-Tenant Architecture

This application is built as a **Shared Database, Shared Schema Multi-Tenant SaaS** application. Multiple companies (tenants) use the same application and the same database, while each tenant can only access its own data.

---

# Architecture Overview

```text
User Login
    │
    ▼
TenantClaimsPrincipalFactory
(Adds tenant_id claim to authentication cookie)
    │
    ▼
Authentication Cookie
(Name, Role, tenant_id)
    │
    ▼
New Request
    │
    ▼
Authentication Middleware
    │
    ▼
TenantMiddleware
(Validates tenant information)
    │
    ▼
Controllers
    │
    ▼
ApplicationDbContext
(Global Query Filters + Write Guards)
    │
    ▼
SQL Server
```

---

# Tenant Model

Every company using the application is represented by a **Tenant**.

Users belong to a single tenant (via `ApplicationUser.TenantId`), while the **SuperAdmin** does not belong to any tenant and can manage all tenants.

---

# ITenantOwned

Every entity that belongs to a tenant implements the `ITenantOwned` interface.

```csharp
public class Product : ITenantOwned
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TenantId { get; set; }   // Required by interface
}
```

This allows tenant query filters and write protections to be applied automatically.

---

# Tenant Claims & TenantProvider

During login, `TenantClaimsPrincipalFactory` adds a custom `tenant_id` claim to the authentication cookie.

`TenantProvider` (scoped per request) reads this claim once and provides:

- `TenantId`
- `IsSuperAdmin`

This value is reused by the middleware, `DbContext`, and write guards during the same request.

---

# TenantMiddleware

The middleware validates every authenticated request with the following rules:

- Anonymous users → continue normally.
- SuperAdmin → bypass tenant validation.
- Regular users → must have a valid `TenantId`.
- Requests from authenticated users with no tenant → rejected with **403 Forbidden**.

---

# Global Query Filters (Read Protection)

In `ApplicationDbContext.OnModelCreating()`, the extension method `ApplyTenantQueryFilters()` automatically applies a global query filter to every entity that implements `ITenantOwned`.

For normal users the filter becomes:

```sql
WHERE TenantId = CurrentTenantId
```

- Tenants are fully isolated from each other.
- SuperAdmin can see data across all tenants.
- No manual filtering needed in controllers or repositories.

---

# Tenant Write Protection

Write protection is handled in `ChangeTrackerTenantExtensions.ApplyTenantWriteGuards()` and is called automatically before every `SaveChanges()` / `SaveChangesAsync()`.

## INSERT

For any new entity implementing `ITenantOwned`, the current user's `TenantId` is automatically assigned.

SuperAdmin can create entities for any tenant (no auto-assignment).

## UPDATE

Changing the `TenantId` of an existing entity is blocked for non-SuperAdmin users.

This prevents data from being moved between tenants.

---

# Startup Tenant Safety Check

During model creation (`OnModelCreating`), the method `EnsureNoUnprotectedTenantEntities()` scans all entities and throws an exception if:

- An entity has a `TenantId` property, but
- Does not implement `ITenantOwned`, and
- Is not an Identity framework entity.

This prevents accidental creation of tenant-owned data that would not be protected by query filters.

---

# Reflection-Based Automatic Configuration

The system uses reflection to:

- Automatically discover all `ITenantOwned` entities.
- Apply global query filters.
- Enforce the tenant safety check at startup.

Adding a new tenant-owned entity only requires:

1. Create the model class.
2. Implement `ITenantOwned`.
3. Add `DbSet<TEntity>` in `ApplicationDbContext`.

No additional filter or guard configuration is needed.

---

# Security Summary

This project protects tenant isolation using multiple layers:

- ASP.NET Core Identity + custom `TenantClaimsPrincipalFactory`
- `TenantMiddleware` for early validation
- Scoped `TenantProvider`
- EF Core Global Query Filters (Read isolation)
- Automatic write guards on `SaveChanges` (Insert & Update protection)
- Startup validation to catch missing tenant protection

This multi-layered approach ensures strong read-side and write-side tenant isolation while reducing developer error.