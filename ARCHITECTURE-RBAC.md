# 🔐 Role & Permission System

This project uses **ASP.NET Core Identity**, **Roles**, **Permissions**, and **Claims** to control what users can access.

Instead of giving every user the same access, each user only sees the features they are allowed to use.

The goal was to build a simple and flexible permission system that can easily grow as new modules are added.

---

# Why I Used Roles and Permissions

Using only roles like **Admin** and **User** works for small projects.

But in a real application, one user may need to:

- View Products
- Manage Products
- View Inventory
- Manage Inventory
- Manage Users

If I created a separate role for every combination, there would be too many roles.

Instead I used:

- **Role** → tells what type of user it is.
- **Permission** → tells which features that user can use.

This keeps the system simple and easy to manage.

---

# Roles

There are three roles in this project.

| Role          | Description                                                            |
|---------------|------------------------------------------------------------------------|
| SuperAdmin    | Platform owner. Can access everything.                                 |
| TenantAdmin   | Company administrator. Can manage everything inside their own company. |
| User          | Regular staff member. Only has the permissions assigned to them.       |

Both **SuperAdmin** and **TenantAdmin** automatically have full access.

Only **User** accounts need individual permissions.

---

# Permission Table

Each feature in the application has a permission.

Example:

```csharp
public class Permission
{
    public int Id { get; set; }

    public string Key { get; set; }

    public string DisplayName { get; set; }

    public string GroupName { get; set; }

    public string IconClass { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
```

Example permission keys:

```
inv.dashboard.view

inv.products.view

inv.products.manage

inv.inventory.manage

inv.users.manage
```

Permissions are assigned using a **UserPermission** table.

```
ApplicationUser
       │
       │
UserPermission
       │
       │
Permission
```

One user can have many permissions.

One permission can belong to many users.

---

# Using Claims

I did not want to check the database every time a user opens a page.

Instead, when the user signs in:

- SuperAdmin receives

```
perm = *
```

- TenantAdmin receives

```
perm = *
```

- Regular users receive only the permissions they have.

Example:

```
tenant_id = 5

perm = inv.dashboard.view

perm = inv.products.view

perm = inv.inventory.manage
```

These claims are stored in the login cookie.

Later, when the user opens another page, the application reads the claims instead of checking the database again.

This makes permission checking faster.

---

# Protecting Controllers

Controllers are protected using policies.

Example:

```csharp
[Authorize(Policy = "perm:inv.products.manage")]
public IActionResult ManageProducts()
{
    return View();
}
```

When this page is opened:

- If the user has

```
perm = *
```

they are allowed.

or

```
perm = inv.products.manage
```

they are also allowed.

Otherwise access is denied.

---

# How Permission Checking Works

```
User Login
      │
      ▼
Read User Permissions
      │
      ▼
Create Claims
      │
      ▼
Store Claims in Cookie
      │
      ▼
User Opens a Page
      │
      ▼
Read Claims
      │
      ▼
Allow or Deny Access
```

---

# Role Management

Tenant Admin can manage user permissions from the **Role Management** page.

## Screenshot

> **Place the screenshot below**

```md
![Role Management](./Images/role-management.png)
```

The page has three sections.

### 1. All Users

Shows every user in the current company.

Users can be selected using the checkboxes.

### 2. Selected Users

Shows only the users that were selected.

Clicking a user loads their permissions.

### 3. Permissions

Permissions are grouped by category, such as:

- Dashboard
- Catalog
- Inventory
- Sales
- Reports

Tenant Admin can check or uncheck permissions and click **Save Permissions**.

Changes are saved using AJAX without refreshing the page.

---

# Updating Permissions

If a user's permissions are changed while they are already logged in, the login cookie still contains the old permissions.

To refresh it, I call:

```csharp
await _userManager.UpdateSecurityStampAsync(user);
```

ASP.NET Identity automatically creates a new login cookie after the security stamp changes.

This allows permission updates to be applied without asking the user to manually log out.

---

# Tenant Security

Permissions decide **what** a user can do.

Tenant security decides **which company's data** they can access.

Every database query is automatically filtered using the user's Tenant Id.

Because of this:

- Users only see data from their own company.
- Tenant Admin cannot access another company's data.
- Changing the URL cannot access another tenant's records.

---

# Adding a New Permission

Adding a new permission is simple.

### Step 1

Add the permission inside

```
PermissionSeeder.cs
```

### Step 2

Update the seed version.

Example

```
permissions:v1
```

↓

```
permissions:v2
```

### Step 3

Run the application.

The new permission will automatically:

- Be added to the database
- Appear in Role Management
- Be available for authorization

---

# Current Status

The authentication and permission system is complete.

Business pages like:

- Products
- Inventory
- Sales
- Purchasing

are currently simple placeholder pages because I wanted to finish the login, role management, and permission system first before building the remaining modules.

---

# Technologies Used

- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- Policy-Based Authorization
- Claims-Based Authentication
- AJAX
- Bootstrap

---

# What I Learned

While building this feature I learned:

- ASP.NET Core Identity
- Roles and Permissions
- Claims
- Policy-Based Authorization
- Custom Authorization Policy Provider
- Custom Authorization Handler
- Security Stamp
- EF Core Global Query Filters
- Multi-Tenant Data Filtering