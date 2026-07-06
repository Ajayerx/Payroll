# PayrollApp Architecture Guide
## Patterns & Guidance Derived from SoftGoWay (SGWProject)

> **Purpose**: This document maps the proven patterns from the existing SoftGoWay project to the new PayrollApp (React.js + ASP.NET Core 9), providing concrete guidance for database design, stored procedures, backend architecture, and frontend patterns.

---

## 1. Database Schema Patterns

### 1.1 Naming Conventions (from SoftGoWay)

| Pattern | SoftGoWay Usage | Recommended for PayrollApp |
|---------|-----------------|---------------------------|
| `tbl*` | Internal/employee modules (`tblUser`, `tblAttendanceSession`) | Use directly without prefix: `Users`, `Employees`, `Payrolls` (EF Core convention) |
| `DNS_*` | Shared reference data (`DNS_Company`, `DNS_Role`) | Use category prefix: `Settings_*`, `Lookup_*`, `Ref_*` |
| Audit columns | `CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`, `IsActive`, `IsDeleted` | Add to ALL tables |
| PK type | `UNIQUEIDENTIFIER` with `DEFAULT NEWID()` | Use `int` identity (simpler for EF Core + better index perf) — **BUT** consider GUID for distributed scenarios |

### 1.2 Required Audit Columns (Add to Every Table)

```sql
ALTER TABLE [TableName] ADD
    CreatedBy    INT              NULL,       -- FK to Users.Id
    CreatedDate  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy    INT              NULL,       -- FK to Users.Id
    UpdatedDate  DATETIME2        NULL,
    IsActive     BIT              NOT NULL DEFAULT 1,
    IsDeleted    BIT              NOT NULL DEFAULT 0;
```

**SoftGoWay pattern**: Every table follows this. The `IsDeleted` soft-delete is universal — no physical deletes anywhere.

### 1.3 SoftGoWay Schema Patterns Applied to PayrollApp

Here's how SoftGoWay's table categories map to PayrollApp:

| SoftGoWay Table | Pattern | PayrollApp Equivalent | Notes |
|-----------------|---------|----------------------|-------|
| `tblUser` | Core identity | `Users` | Already exists, good |
| `tblUserRoleMapping` | Many-to-many | `UserRoles` (if needed) | Current enum approach works for now |
| `tblRefreshToken` | Token storage | `RefreshTokens` | **Add this** — currently stored inline on User |
| `DNS_Company` | Parent entity | `CompanySettings` | Already exists as singleton |
| `DNS_Country/State/District` | Geo lookup hierarchy | `LookupCountries`, `LookupStates` | Consider adding for employee addresses |
| `tblAttendanceSession` | Time tracking | `AttendanceRecords` | Future feature |
| `tblBreakSession` | Child detail | `BreakLogs` | Future feature |
| `LeaveRequest` | Request/approval | `LeaveRequests` | Already has `LeaveTypes`, add `LeaveRequests` |
| `Holiday` | Reference data | `Holidays` | Future feature |
| `tblEvaluation*` | Performance | `PerformanceReviews` | Future feature |
| `tblNotification` | System notifications | `Notifications` | Future feature |
| `tbl_email_templates` | Email templates | `EmailTemplates` | Future feature |
| `tblAuditLog` | Change tracking | `AuditLogs` | **Add this** — critical for payroll |

### 1.4 Recommended New Tables for PayrollApp

```sql
-- 1. Refresh token store (move out of Users table)
CREATE TABLE RefreshTokens (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL REFERENCES Users(Id),
    Token        NVARCHAR(500) NOT NULL,
    ExpiryDate   DATETIME2 NOT NULL,
    IsRevoked    BIT NOT NULL DEFAULT 0,
    CreatedDate  DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedDate  DATETIME2 NULL
);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);

-- 2. Audit log (track all changes)
CREATE TABLE AuditLogs (
    Id           BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityName   NVARCHAR(100) NOT NULL,    -- 'Employee', 'Payroll', etc.
    EntityId     INT NOT NULL,
    Action       NVARCHAR(20) NOT NULL,     -- 'CREATE', 'UPDATE', 'DELETE'
    OldValues    NVARCHAR(MAX) NULL,        -- JSON snapshot
    NewValues    NVARCHAR(MAX) NULL,        -- JSON snapshot
    ChangedBy    INT NULL REFERENCES Users(Id),
    ChangedDate  DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IPAddress    NVARCHAR(50) NULL
);
CREATE INDEX IX_AuditLogs_Entity ON AuditLogs(EntityName, EntityId);

-- 3. Leave requests (you already have LeaveTypes)
CREATE TABLE LeaveRequests (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId     INT NOT NULL REFERENCES Employees(Id),
    LeaveTypeId    INT NOT NULL REFERENCES LeaveTypes(Id),
    FromDate       DATE NOT NULL,
    ToDate         DATE NOT NULL,
    TotalDays      DECIMAL(4,1) NOT NULL,
    Reason         NVARCHAR(500) NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, Approved, Rejected
    ApprovedBy     INT NULL REFERENCES Users(Id),
    ApprovedDate   DATETIME2 NULL,
    CreatedDate    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate    DATETIME2 NULL
);
CREATE INDEX IX_LeaveRequests_Employee ON LeaveRequests(EmployeeId);
```

---

## 2. Stored Procedure Patterns

### 2.1 SoftGoWay SP Naming Convention

| Pattern | Example | When to Use |
|---------|---------|-------------|
| `sp_Create*` | `sp_CreateCompany` | INSERT operations |
| `sp_Update*` | `sp_UpdateCompany` | UPDATE operations |
| `sp_Delete*` | `sp_DeleteCompany` | Soft delete (SET IsDeleted=1) |
| `sp_Get*` (by ID) | `sp_GetCompanyById` | Single record detail |
| `sp_*List` | `sp_CompanyList` | Listing/filtering |
| `sp_Get*Dropdown` | `sp_GetRoleDropdown` | Dropdown data for UI |

### 2.2 Standard SoftGoWay SP Templates

#### CREATE Pattern (`sp_CreatePayroll` for PayrollApp)

```sql
CREATE PROCEDURE sp_CreatePayroll
    @EmployeeId      INT,
    @PayrollMonthId  INT,
    @GrossSalary     DECIMAL(18,2),
    @TaxDeduction    DECIMAL(18,2),
    @OtherDeductions DECIMAL(18,2),
    @NetSalary       DECIMAL(18,2),
    @Status          NVARCHAR(20),
    @CreatedBy       INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewId INT;

    INSERT INTO Payrolls
        (EmployeeId, PayrollMonthId, GrossSalary, TaxDeduction,
         OtherDeductions, NetSalary, Status, ProcessedDate,
         CreatedBy, CreatedDate)
    VALUES
        (@EmployeeId, @PayrollMonthId, @GrossSalary, @TaxDeduction,
         @OtherDeductions, @NetSalary, @Status, GETUTCDATE(),
         @CreatedBy, GETUTCDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- Return the new record
    SELECT * FROM Payrolls WHERE Id = @NewId;
END;
```

#### GET Pattern (`sp_GetPayrollById` for PayrollApp)

```sql
CREATE PROCEDURE sp_GetPayrollById
    @PayrollId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id, p.EmployeeId,
        e.EmployeeCode, e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department, e.Designation,
        p.PayrollMonthId, pm.Month, pm.Year,
        p.GrossSalary, p.TaxDeduction, p.OtherDeductions,
        p.NetSalary, p.Status, p.ProcessedDate,
        u.Username AS ProcessedByName,
        p.CreatedDate
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    LEFT JOIN Users u ON u.Id = p.CreatedBy
    WHERE p.Id = @PayrollId;
END;
```

#### LIST Pattern (`sp_PayrollList` for PayrollApp)

```sql
CREATE PROCEDURE sp_PayrollList
    @Month    INT = NULL,
    @Year     INT = NULL,
    @Status   NVARCHAR(20) = NULL,
    @Search   NVARCHAR(100) = NULL,
    @PageNum  INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Skip INT = (@PageNum - 1) * @PageSize;

    -- Base result set
    SELECT
        p.Id, p.EmployeeId,
        e.EmployeeCode,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department,
        pm.Month, pm.Year,
        p.GrossSalary, p.NetSalary,
        p.Status, p.ProcessedDate
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    WHERE (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
      AND (@Month IS NULL OR pm.Month = @Month)
      AND (@Year IS NULL OR pm.Year = @Year)
      AND (@Status IS NULL OR p.Status = @Status)
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%')
    ORDER BY pm.Year DESC, pm.Month DESC, e.EmployeeCode
    OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Total count for pagination
    SELECT COUNT(*)
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    WHERE (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
      AND (@Month IS NULL OR pm.Month = @Month)
      AND (@Year IS NULL OR pm.Year = @Year)
      AND (@Status IS NULL OR p.Status = @Status)
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%');
END;
```

#### UPDATE Pattern (`sp_UpdatePayroll` for PayrollApp)

```sql
CREATE PROCEDURE sp_UpdatePayroll
    @PayrollId       INT,
    @GrossSalary     DECIMAL(18,2) = NULL,
    @TaxDeduction    DECIMAL(18,2) = NULL,
    @OtherDeductions DECIMAL(18,2) = NULL,
    @NetSalary       DECIMAL(18,2) = NULL,
    @Status          NVARCHAR(20) = NULL,
    @UpdatedBy       INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Payrolls
    SET
        GrossSalary     = ISNULL(@GrossSalary, GrossSalary),
        TaxDeduction    = ISNULL(@TaxDeduction, TaxDeduction),
        OtherDeductions = ISNULL(@OtherDeductions, OtherDeductions),
        NetSalary       = ISNULL(@NetSalary, NetSalary),
        Status          = ISNULL(@Status, Status),
        UpdatedBy       = @UpdatedBy,
        UpdatedDate     = GETUTCDATE()
    WHERE Id = @PayrollId AND (IsDeleted = 0 OR IsDeleted IS NULL);
END;
```

#### DELETE Pattern (Soft Delete — `sp_DeletePayroll`)

```sql
CREATE PROCEDURE sp_DeletePayroll
    @PayrollId INT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Payrolls
    SET IsDeleted = 1,
        IsActive  = 0,
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETUTCDATE()
    WHERE Id = @PayrollId;
END;
```

### 2.3 When to Use SPs vs EF Core

| Scenario | Approach | SoftGoWay Reference |
|----------|----------|---------------------|
| Simple CRUD (single table) | EF Core LINQ | Controllers use `db.Users.Where(...)` |
| Complex joins / reporting | Stored Procedure | `sp_CompanyList`, `sp_GetCompanyById` |
| Paginated lists with filters | Stored Procedure | `ProjectService.GetProjectList()` using inline SQL |
| Business logic (payroll calc) | Stored Procedure or C# Service | SoftGoWay does this in C# service layer |
| Aggregation / stats | Stored Procedure | `ProjectService` stats query |

**Recommendation**: Keep the current EF Core approach for simple CRUD. Add stored procedures for:
- Payroll processing (complex calc + transaction)
- Reports (salary register, tax summary)
- List endpoints with dynamic filtering (currently using raw SQL in service layer)

---

## 3. Backend Architecture Patterns

### 3.1 Layered Architecture Comparison

```
SoftGoWay (MVC + Web API)                PayrollApp (ASP.NET Core 9)
┌──────────────────────┐                 ┌──────────────────────┐
│  Controller (MVC)    │                 │  Controller (API)    │
│  Web API Controller  │                 │  [ApiController]     │
├──────────────────────┤                 ├──────────────────────┤
│  Services (Business) │                 │  Services (Business) │
│  - Static classes    │                 │  - Interface-based   │
│  - Singleton DI      │                 │  - Scoped DI         │
├──────────────────────┤                 ├──────────────────────┤
│  DbContext (EF)      │                 │  DbContext (EF Core) │
│  ADO.NET (SPs)       │                 │  ADO.NET (SPs)       │
├──────────────────────┤                 ├──────────────────────┤
│  SQL Server          │                 │  SQL Server          │
└──────────────────────┘                 └──────────────────────┘
```

### 3.2 Patterns to Adopt from SoftGoWay

#### A. Constants Class (like `Helpers\Constants.cs`)

```csharp
// PayrollApp/Backend/PayrollApi/Constants/Roles.cs
namespace PayrollApi.Constants;

public static class Roles
{
    public const string Admin     = "Admin";
    public const string HRManager = "HRManager";
    public const string Employee  = "Employee";

    public static readonly string[] All = { Admin, HRManager, Employee };
    public static readonly string[] AdminAndHR = { Admin, HRManager };
}

/// <summary>
/// Maps to JWT claim types — no server-side sessions involved.
/// The new project uses JWT exclusively.
/// </summary>
public static class ClaimKeys
{
    public const string UserId    = "userId";
    public const string Role      = "role";
    public const string UserName  = "userName";
    public const string UserEmail = "userEmail";
}
```

#### B. Base Controller (like `Controllers\BaseController.cs`)

```csharp
// PayrollApp/Backend/PayrollApi/Controllers/BaseApiController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PayrollApi.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    protected string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role);

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email);

    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}
```

Then make all controllers inherit from it:

```csharp
[Route("api/v1/employees")]
[Authorize(Roles = "Admin,HRManager")]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> Get(int id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            return Ok(employee);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
```

#### C. Service Interface Pattern (already done well — keep it)

Your current approach is good: `IAuthService` -> `AuthService`. Continue this pattern.

**Add from SoftGoWay**:
- Consider adding a `BaseService` with common audit trail logic
- Add caching service (like `MemoryCacheService`) for lookup data

```csharp
// PayrollApp/Backend/PayrollApi/Services/Interfaces/ICacheService.cs
public interface ICacheService
{
    T GetOrSet<T>(string key, Func<T> factory, int minutes = 10);
    void Remove(string key);
    void Clear();
}
```

#### D. Audit Trail (like ProjectService's JSON snapshot pattern)

From SoftGoWay's `ProjectService.UpdateProject()`, it takes a JSON snapshot before/after changes using `FOR JSON PATH`. Apply this to PayrollApp:

```csharp
// In a base service or middleware
private async Task LogAuditAsync<T>(
    string entityName,
    int entityId,
    string action,
    T oldEntity,
    T newEntity,
    int changedBy)
{
    var audit = new AuditLog
    {
        EntityName = entityName,
        EntityId = entityId,
        Action = action,
        OldValues = JsonSerializer.Serialize(oldEntity),
        NewValues = JsonSerializer.Serialize(newEntity),
        ChangedBy = changedBy,
        ChangedDate = DateTime.UtcNow
    };
    _context.AuditLogs.Add(audit);
    await _context.SaveChangesAsync();
}
```

### 3.3 SoftGoWay Patterns to AVOID in PayrollApp

| Pattern | SoftGoWay | Why Avoid in PayrollApp |
|---------|-----------|------------------------|
| Static service classes | `AuthenticationService` is static | Prevents mocking, testing, DI. You already use interface-based DI — keep it |
| Direct `HttpContext` in services | Used in `AuthHelper` | Pass dependencies via constructor injection |
| ADO.NET mixed with EF in controllers | `Database.SqlQuery<T>()` in controllers | Keep data access in service layer |
| Session-based auth | `Session["UserGuid"]`, Forms Auth cookie | ❌ **Don't use at all** — PayrollApp uses JWT exclusively. Zero server-side session |
| Autofac + manual DI | Hybrid approach | Stick with built-in DI (already done) |

---

## 4. Frontend Patterns from SoftGoWay

### 4.1 What SoftGoWay Does (Razor + jQuery)

SoftGoWay uses server-rendered Razor views with jQuery for interactivity. PayrollApp is a React SPA, so the patterns differ significantly. However, some concepts transfer:

| Concept | SoftGoWay (jQuery) | PayrollApp (React) |
|---------|-------------------|-------------------|
| AJAX calls | `$.ajax({...})` | `axios` service modules |
| Form validation | jQuery Validate + Unobtrusive | `react-hook-form` + Yup/Zod |
| UI updates | DOM manipulation | Redux state -> re-render |
| Routing | MVC routes | `react-router-dom` |
| Auth guard | `[RoleAuthorize]` attribute | `<ProtectedRoute roles={...}/>` component |

### 4.2 What's Already Good in PayrollApp Frontend

- ✅ Redux Toolkit with `createAsyncThunk` — clean async state management
- ✅ Axios interceptor pattern with token refresh queue — excellent
- ✅ Service modules as plain objects — simple and testable
- ✅ `ProtectedRoute` component for auth guard
- ✅ Custom hooks (`useAuth`, `useEmployees`) — clean abstraction
- ✅ Role-based route protection

### 4.3 Improvements from SoftGoWay Patterns

#### A. Constants File (like `Helpers\Constants.cs`)

Your current `constants/index.js` is good. Consider expanding:

```javascript
// frontend/src/constants/index.js
export const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5175/api/v1';
export const SESSION_TIMEOUT = 30 * 60 * 1000; // 30 minutes

export const ROLES = {
  ADMIN: 'Admin',
  HR_MANAGER: 'HRManager',
  EMPLOYEE: 'Employee',
};

export const ROLES_LABELS = {
  [ROLES.ADMIN]: 'Administrator',
  [ROLES.HR_MANAGER]: 'HR Manager',
  [ROLES.EMPLOYEE]: 'Employee',
};

export const PAYROLL_STATUS = {
  DRAFT: 'Draft',
  PROCESSED: 'Processed',
  PAID: 'Paid',
};

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZES: [10, 20, 50, 100],
};

export const ENTITY_NAMES = {
  EMPLOYEE: 'Employee',
  PAYROLL: 'Payroll',
  SALARY_COMPONENT: 'SalaryComponent',
};
```

#### B. Shared Component Pattern

SoftGoWay has reusable partial views (`_Footer.cshtml`, `_ServiceCategoriesMenu.cshtml`). Your current `Common/` components (`DataTable.js`, `PageHeader.js`, etc.) are good. Continue expanding:

Add from the jQuery patterns you'd see in SoftGoWay:
- `ConfirmDialog.js` — already there (for delete confirmations)
- `StatusBadge.js` — already there
- Add `Dropdown.js` — reusable lookup dropdown
- Add `SearchBar.js` — reusable search with debounce
- Add `Pagination.js` — for paginated lists

#### C. API Error Handling

SoftGoWay MVC controllers return JSON with `{ redirect: "/Auth/Login" }` for AJAX 401s. Your Axios interceptor already handles this well. One addition:

```javascript
// In axios.js interceptor — add a toast/notification for errors
import { toast } from 'react-toastify';

api.interceptors.response.use(
  response => response,
  async error => {
    const message = error.response?.data?.message || error.message;
    
    // Don't toast for 401 (handled by redirect)
    if (error.response?.status !== 401) {
      toast.error(message);
    }
    
    // ... existing token refresh logic ...
  }
);
```

---

## 5. Data Flow: End-to-End Pattern

### 5.1 SoftGoWay Flow (Read)

```
Browser
  │
  ├─► MVC Route ──► Controller Action
  │                    │
  │                    ├─► Service.GetList()
  │                    │       │
  │                    │       ├─► SP: sp_CompanyList
  │                    │       │       │
  │                    │       │       └─► SQL Server
  │                    │       │
  │                    │       └─► Mapped to ViewModel
  │                    │
  │                    └─► Returns View(model)
  │
  └─► Razor View renders HTML + jQuery enhancements
```

### 5.2 PayrollApp Flow (Read — Current Pattern)

```
Browser
  │
  ├─► React Component
  │       │
  │       ├─► Custom Hook (useEmployees)
  │       │       │
  │       │       └─► Redux Thunk (fetchEmployees)
  │       │               │
  │       │               └─► employeeService.getAll(params)
  │       │                       │
  │       │                       └─► axios GET /api/v1/employees
  │       │                               │
  │       │                               └─► Controller.GetAll()
  │       │                                       │
  │       │                                       └─► EmployeeService.GetAllAsync()
  │       │                                               │
  │       │                                               └─► EF Core: _context.Employees
  │       │                                                       │
  │       │                                                       └─► SQL Server
  │       │
  │       └─► Renders UI from Redux state
  │
  └─► Material UI Components
```

### 5.3 PayrollApp Flow — Add SP for Complex Queries

```
Browser
  │
  ├─► React Component
  │       │
  │       └─► Redux Thunk (fetchPayrollReport)
  │               │
  │               └─► reportService.getSalaryRegister(params)
  │                       │
  │                       └─► axios GET /api/v1/reports/salary-register
  │                               │
  │                               └─► ReportsController.GetSalaryRegister()
  │                                       │
  │                                       └─► ReportService.GetSalaryRegisterAsync()
  │                                               │
  │                                               └─► ADO.NET: SqlCommand("sp_SalaryRegister")
  │                                                       │
  │                                                       └─► SQL Server
```

---

## 6. Authentication & Authorization

> **PayrollApp uses JWT exclusively.** No server-side sessions, no Forms Authentication cookies, no session state. All auth state lives in the JWT token (claims) and the refresh token (database). The frontend stores tokens in `localStorage` and attaches them via Axios interceptor.

### 6.1 Comparison

| Aspect | SoftGoWay | PayrollApp | Recommendation |
|--------|-----------|------------|----------------|
| Auth mechanism | Forms Auth + JWT + Session | JWT Bearer only | ✅ Keep JWT only |
| Token refresh | `tblRefreshToken` table | Stored on `Users.RefreshToken` | **Move to separate `RefreshTokens` table** |
| Password hashing | `Microsoft.AspNet.Identity.Core.PasswordHasher` | `BCrypt.Net-Next` | ✅ Keep BCrypt |
| Role enforcement | `[RoleAuthorize]` attribute (MVC), `[JwtRoleAuthorize]` (API) | `[Authorize(Roles="...")]` | ✅ Current is good |
| Token expiry | Forms Auth cookie (60 min) | JWT expiry (60 min) + refresh token rotation | ✅ Keep JWT-only with refresh |
| SSO | `tblSSOLoginToken` with 1-min expiry | Not implemented | Add if needed |
| OTP login | 6-digit OTP in session | Not implemented | Add if needed |

### 6.2 Refresh Token Enhancement

Current PayrollApp stores refresh token on `Users` table (single token per user). SoftGoWay has a proper `tblRefreshToken` table supporting multiple tokens and rotation. **Implement this:**

```csharp
// In AuthService
public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
{
    var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

    if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
        throw new UnauthorizedAccessException("Invalid or expired refresh token");

    // Revoke old token
    storedToken.IsRevoked = true;
    storedToken.RevokedDate = DateTime.UtcNow;

    // Generate new tokens
    var user = await _context.Users.FindAsync(storedToken.UserId);
    var newAccessToken = GenerateJwtToken(user);
    var newRefreshToken = GenerateRefreshToken();

    _context.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiryDate = DateTime.UtcNow.AddDays(7)
    });

    await _context.SaveChangesAsync();

    return new AuthResponse
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken
    };
}
```

---

## 7. SoftGoWay Patterns Worth Porting to PayrollApp

### 7.1 Role-Based Layouts

SoftGoWay has a separate `_Layout*.cshtml` per role (`_LayoutAdmin.cshtml`, `_LayoutDeveloper.cshtml`, etc.). You're already doing this in React via route-level layouts:

```javascript
// App.js — already done with Layout wrapper inside ProtectedRoute
<Routes>
  <Route path="/login" element={<LoginPage />} />
  <Route element={<ProtectedRoute roles={ROLES.ADMIN_AND_HR} />}>
    <Route element={<Layout />}>
      <Route path="/employees" element={<EmployeeListPage />} />
      ...
    </Route>
  </Route>
  <Route element={<ProtectedRoute roles={[ROLES.EMPLOYEE]} />}>
    <Route element={<EmployeeLayout />}>
      <Route path="/my-salary" element={<MySalaryPage />} />
    </Route>
  </Route>
</Routes>
```

Consider creating role-specific layouts (like `AdminLayout`, `EmployeeLayout`) with different sidebar menus, just like SoftGoWay's per-role layouts.

### 7.2 Dropdown Caching

SoftGoWay's `ProjectService` uses `MemoryCacheService` to cache dropdown data (company list, category list) for 10 minutes. Apply this to PayrollApp:

```csharp
// PayrollApp/Backend/PayrollApi/Services/LookupCacheService.cs
public class LookupCacheService : ILookupCacheService
{
    private readonly IMemoryCache _cache;
    private readonly PayrollDbContext _context;

    public LookupCacheService(IMemoryCache cache, PayrollDbContext context)
    {
        _cache = cache;
        _context = context;
    }

    public async Task<List<SelectListItem>> GetSalaryComponents()
    {
        return await _cache.GetOrCreateAsync("SalaryComponents", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.SalaryComponents
                .Where(sc => sc.IsActive)
                .Select(sc => new SelectListItem
                {
                    Value = sc.Id.ToString(),
                    Text = sc.Name
                })
                .ToListAsync();
        });
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<ILookupCacheService, LookupCacheService>();
```

### 7.3 SignalR for Real-Time (ChatHub Pattern)

SoftGoWay has a `ChatHub` using SignalR. For PayrollApp, you might need real-time notifications:

```csharp
// PayrollApp/Backend/PayrollApi/Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;

namespace PayrollApi.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task SendNotification(string userId, string title, string message)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddSignalR();
// In pipeline:
app.MapHub<NotificationHub>("/hubs/notifications");
```

### 7.4 Background Jobs (Hangfire Pattern)

SoftGoWay uses Hangfire for `SendBirthdayEmails`. For PayrollApp, consider adding Hangfire for:

- **Payroll auto-processing** on scheduled dates
- **Email notifications** for payslips
- **Report generation** (async export)

```csharp
// Program.cs
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

// RecurringJob.AddOrUpdate<IPayrollService>(
//     "process-monthly-payroll",
//     s => s.AutoProcessMonthlyPayroll(),
//     "0 0 1 * *"); // 1st of every month
```

---

## 8. File Organization Comparison

### SoftGoWay (ASP.NET MVC 5)
```
SoftGoWay/
├── Controllers/        63 controllers
├── Models/             EF entities + 99 ViewModels
├── Services/           12 service files
├── Helpers/            7 helper files
├── Filters/            3 attribute filters
├── Hubs/               1 SignalR hub
├── Views/              57 view directories
├── Scripts/            jQuery + plugins
├── Content/            CSS + themes
├── DbScript/           ~100 SQL files (dated folders)
└── App_Start/          Config files
```

### PayrollApp (Current)
```
PayrollApp/
├── backend/PayrollApi/
│   ├── Controllers/    6 controllers
│   ├── Models/         Entities + DTOs + Enums
│   ├── Services/       6 service files
│   ├── Data/           DbContext + Migrations
│   ├── Middleware/     2 middleware files
│   ├── Validators/     6 validator files
│   ├── Utils/          2 utility files
│   ├── Program.cs      Startup config
│   └── appsettings.json
└── frontend/src/
    ├── components/     Feature + common components
    ├── pages/          Route-level page components
    ├── services/       Axios service modules
    ├── store/          Redux slices
    ├── hooks/          Custom hooks
    ├── constants/      Config constants
    └── utils/          Utility functions
```

### Recommended Additions to PayrollApp

| Directory | Purpose | SoftGoWay Reference |
|-----------|---------|---------------------|
| `backend/PayrollApi/Constants/` | Role constants, entity names | `Helpers\Constants.cs` |
| `backend/PayrollApi/Hubs/` | SignalR hubs | `Hubs\ChatHub.cs` |
| `backend/PayrollApi/Middleware/` | Already started — add `AuditMiddleware` | `Filters\RoleAuthorizeAttribute` |
| `backend/PayrollApi/Attributes/` | Custom authorization filters | `Filters\JwtRoleAuthorizeAttribute.cs` |
| `backend/PayrollApi/DbScripts/` | Stored procedures (dated folders) | `DbScript\` |
| `frontend/src/constants/` | Already exists — expand with enums/labels | `Helpers\Constants.cs` |

---

## 9. Quick Wins: What to Implement Next

### Priority 1 (High Impact, Low Effort)

1. **Add audit columns to all tables** (`CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`, `IsActive`, `IsDeleted`)
2. **Move refresh tokens to separate table** (`RefreshTokens`)
3. **Add `BaseApiController`** with `CurrentUserId`, `CurrentRole` properties
4. **Create constants file** for roles, payroll statuses, entity names

### Priority 2 (Medium Impact)

5. **Add `AuditLogs` table** + service for tracking changes
6. **Create stored procedures** for complex queries:
   - `sp_PayrollList` (paginated with search/filter)
   - `sp_SalaryRegister` (report with joins)
   - `sp_ProcessPayroll` (transactional payroll calc)
7. **Add caching service** for dropdown/lookup data
8. **Implement role-based layouts** in React (Admin/HR vs Employee)

### Priority 3 (Future)

9. **Add Hangfire** for scheduled payroll processing
10. **Add SignalR** for real-time notifications
11. **Add leave management** (`LeaveRequests` table + UI)
12. **Add document management** (employee document uploads like SoftGoWay's `CandidateDocument`)
13. **Add email templates** system (`tbl_email_templates`)

---

## 10. Key Files Reference

| File | Purpose | Path |
|------|---------|------|
| DbContext | EF Core configuration | `backend/PayrollApi/Data/PayrollDbContext.cs` |
| App Startup | DI, middleware, auth | `backend/PayrollApi/Program.cs` |
| Main config | Connection string, JWT, CORS | `backend/PayrollApi/appsettings.json` |
| API endpoints | Controller examples | `backend/PayrollApi/Controllers/` |
| DTO definitions | Request/response models | `backend/PayrollApi/Models/DTOs/` |
| Entity models | EF Core entities | `backend/PayrollApi/Models/Entities/` |
| Business logic | Service implementations | `backend/PayrollApi/Services/` |
| Frontend routing | Route definitions | `frontend/src/App.js` |
| Redux store | State management | `frontend/src/store/index.js` |
| Axios client | HTTP + interceptors | `frontend/src/services/axios.js` |
| Auth guard | Route protection | `frontend/src/components/Auth/ProtectedRoute.js` |
| API constants | Base URL, roles, etc. | `frontend/src/constants/index.js` |

---

*This document was generated by analyzing the SoftGoWay project (ASP.NET MVC 5 + EF6 + SQL Server) and mapping its proven patterns to the PayrollApp project (React 19 + ASP.NET Core 9 + EF Core + SQL Server).*
