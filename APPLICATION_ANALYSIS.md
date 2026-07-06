# PayrollApp — Complete Application Analysis

## 1. Overview

**PayrollApp** is a full-stack payroll management system built with:
- **Frontend**: React 19 (Create React App), TypeScript, MUI v9, Redux Toolkit 2.12, Tailwind CSS 3.4
- **Backend**: ASP.NET Core 9 Web API, Entity Framework Core 9.0.4, SQL Server
- **Architecture**: RESTful API + JWT auth + SignalR real-time notifications + soft-delete pattern
- **Monetary**: All amounts stored as `decimal(18,2)`, values in INR

---

## 2. Project Structure

```
PayrollApp/
├── backend/PayrollApi/                 # ASP.NET Core 9 Web API
│   ├── Controllers/                    # API endpoint handlers (8 files)
│   ├── Services/                       # Business logic layer (13 files)
│   │   └── Interfaces/                 # Service abstractions (3 interfaces)
│   ├── Models/
│   │   ├── Entities/                   # EF Core entity models (11 files)
│   │   ├── DTOs/                       # Request/response models (8 files)
│   │   └── Enums/                      # Enum definitions (4 files)
│   ├── Middleware/                     # Exception & request logging (2 files)
│   ├── Hubs/                           # SignalR NotificationHub
│   ├── Validators/                     # FluentValidation rules (3 files)
│   ├── Utils/                          # TaxCalculator, PasswordPolicy
│   ├── Constants/                      # Role constants, claim keys
│   ├── Data/                           # DbContext + Seeder
│   ├── Migrations/                     # EF Core migrations (3 files)
│   ├── DbScripts/                      # Stored procedures SQL
│   ├── Program.cs                      # DI, middleware pipeline, config
│   ├── appsettings.json                # Connection strings, JWT, Serilog
│   └── PayrollApi.csproj               # .NET 9 project with dependencies
│
└── frontend/src/                       # React 19 SPA (53 source files)
    ├── components/
    │   ├── Auth/                       # Login, ProtectedRoute
    │   ├── Layout/                     # AdminLayout, EmployeeLayout, Sidebar, Header
    │   ├── Common/                     # DataTable, StatCard, StatusBadge, etc.
    │   ├── Dashboard/                  # Dashboard page components
    │   ├── Employee/                   # EmployeeList, EmployeeForm, EmployeeProfile
    │   ├── Payroll/                    # PayrollList, PayrollProcess
    │   ├── Leave/                      # LeaveList, LeaveForm
    │   ├── Reports/                    # Reports page
    │   ├── Settings/                   # Settings page
    │   └── ui/                         # Custom Tabs component
    ├── pages/                          # Page wrappers (11 files)
    ├── store/                          # Redux store config
    │   └── slices/                     # auth, employee, payroll, ui slices
    ├── hooks/                          # useAuth, useEmployees, usePayroll, useUI
    ├── services/                       # Axios API client + 7 service modules
    ├── utils/                          # helpers, formatters, validation
    ├── constants/                      # Roles, statuses, config values
    ├── types/                          # TypeScript type declarations
    └── styles/                         # Design tokens (colors, spacing, typography)
```

---

## 3. Database Schema (15 Tables)

All tables inherit from `BaseEntity` with audit columns: `CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`, `IsActive`, `IsDeleted`. Soft-delete is enforced everywhere.

| # | Table | PK | Key Columns | Foreign Keys |
|---|-------|----|-------------|--------------|
| 1 | **Users** | Guid (NEWSEQUENTIALID) | Email (unique), PasswordHash, FirstName, LastName, Role (int: Admin/HRManager/Employee), LastLoginAt | — |
| 2 | **Employees** | Guid | UserId, EmployeeCode (unique), FirstName, LastName, DOB, Gender, Phone, Email, Department, Designation, BankName, BankAccount, IFSC | UserId → Users |
| 3 | **SalaryComponents** | Guid | Name, Type (Earning=0/Deduction=1), IsVariable | — |
| 4 | **EmployeeSalaryStructures** | Guid | Amount, EffectiveDate | EmployeeId → Employees, SalaryComponentId → SalaryComponents |
| 5 | **PayrollMonths** | Guid | Month, Year, StartDate, EndDate, IsLocked, Status (int) | — |
| 6 | **Payrolls** | Guid | GrossSalary, TaxDeduction, OtherDeductions, NetSalary, Status, ProcessedDate, Remarks | EmployeeId → Employees, PayrollMonthId → PayrollMonths |
| 7 | **PayrollDetails** | Guid | Amount | PayrollId → Payrolls, SalaryComponentId → SalaryComponents |
| 8 | **Deductions** | Guid | Type (Loan=0/Advance=1), Amount, RemainingAmount, StartDate, EndDate | EmployeeId → Employees |
| 9 | **TaxConfigurations** | Guid | TaxSlab, TaxRate, EffectiveDate | EmployeeId → Employees |
| 10 | **TaxSlabs** | Guid | Name, FromAmount, ToAmount (nullable), Rate | — |
| 11 | **CompanySettings** | Guid | CompanyName, Address, Email, Phone, GSTIN, PAN, LogoUrl | — |
| 12 | **LeaveTypes** | Guid | Name, DaysPerYear (nullable), IsPaid | — |
| 13 | **LeaveRequests** | Guid | FromDate, ToDate, TotalDays, Reason, Status (Pending/Approved/Rejected/Cancelled), ApprovedDate | EmployeeId → Employees, LeaveTypeId → LeaveTypes |
| 14 | **RefreshTokens** | Guid | Token (unique), ExpiryDate, IsRevoked, RevokedDate | UserId → Users |
| 15 | **AuditLogs** | long (identity) | EntityName, EntityId, Action (Create/Update/Delete), OldValues (JSON), NewValues (JSON), ChangedBy, IpAddress | — |

**Key relationships:**
- User 1:1 Employee (each employee has one user account)
- Employee 1:N Payrolls, LeaveRequests, Deductions, TaxConfigurations, EmployeeSalaryStructures
- PayrollMonth 1:N Payrolls
- Payroll 1:N PayrollDetails
- SalaryComponent 1:N PayrollDetails, EmployeeSalaryStructures

**Stored Procedures** (6 in total, in `DbScripts/001_StoredProcedures.sql`):
- `sp_PayrollList`, `sp_GetPayrollById`, `sp_CreatePayroll`, `sp_UpdatePayroll`, `sp_DeletePayroll`
- `sp_SalaryRegister`, `sp_EmployeeList`

---

## 4. Backend API (42 Endpoints)

### AuthController — `/api/v1/auth`
| Method | Path | Access | Purpose |
|--------|------|--------|---------|
| POST | /login | Public | Login, returns JWT + refresh token |
| POST | /register | Public | Create user account |
| POST | /refresh-token | Public | Rotate refresh token |
| POST | /logout | Authenticated | Revoke refresh token |
| POST | /change-password | Authenticated | Change password |
| POST | /forgot-password | Public | Initiate password reset |

### EmployeesController — `/api/v1/employees`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | / | Admin, HRManager | Paginated list with search/filter |
| GET | /{id} | Admin, HRManager | Get single employee |
| POST | / | Admin, HRManager | Create employee |
| PUT | /{id} | Admin, HRManager | Update employee |
| DELETE | /{id} | Admin, HRManager | Soft-delete |
| GET | /search | Admin, HRManager | Search endpoint |
| POST | /bulk-import | Admin, HRManager | CSV bulk import |
| GET | /{id}/documents | Admin, HRManager | Get documents |
| POST | /{id}/documents | Admin, HRManager | Upload document |

### PayrollController — `/api/v1/payroll`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | / | Admin, HRManager | List with month/year/status filters |
| POST | /process | Admin, HRManager | Process payroll for employees |
| GET | /{id} | Admin, HRManager | Get by ID |
| PUT | /{id} | Admin, HRManager | Update |
| DELETE | /{id} | Admin, HRManager | Soft-delete |
| GET | /{id}/salary-slip | Admin, HRManager | Download salary slip |
| POST | /{id}/generate-slip | Admin, HRManager | Generate slip |
| GET | /month/{m}/year/{y} | Admin, HRManager | By month/year |
| POST | /bulk-process | Admin, HRManager | Batch process |
| GET | /export/csv | Admin, HRManager | Export CSV |
| GET | /{id}/export/pdf | Admin, HRManager | Export to PDF |

### LeavesController — `/api/v1/leaves`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | / | All | List with filters |
| GET | /{id} | All | Get by ID |
| POST | / | Employee | Create request |
| PUT | /{id} | Employee | Update own leave |
| PUT | /{id}/cancel | Employee | Cancel own leave |
| PUT | /{id}/approve | Admin, HRManager | Approve |
| PUT | /{id}/reject | Admin, HRManager | Reject |
| GET | /employee/{id} | All | By employee |
| GET | /types | All | Leave types |
| GET | /balance/{id} | All | Leave balance |

### SalaryController — `/api/v1/`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | /salary-components | Admin, HRManager | List components |
| POST | /salary-components | Admin, HRManager | Create component |
| PUT | /salary-components/{id} | Admin, HRManager | Update component |
| GET | /employees/{id}/salary-structure | Admin, HRManager | Get structure |
| PUT | /employees/{id}/salary-structure | Admin, HRManager | Update structure |
| GET | /employees/{id}/deductions | Admin, HRManager | Get deductions |
| POST | /employees/{id}/deductions | Admin, HRManager | Add deduction |

### ReportsController — `/api/v1/reports`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | /salary-register | Admin, HRManager | Salary register |
| GET | /tax-summary | Admin, HRManager | Tax summary |
| GET | /employee-earnings | Admin, HRManager | Earnings report |
| GET | /department-summary | Admin, HRManager | Department summary |
| POST | /export | Admin, HRManager | Export to CSV/PDF |

### SettingsController — `/api/v1/settings`
| Method | Path | Roles | Purpose |
|--------|------|-------|---------|
| GET | /company | Admin | Company settings |
| PUT | /company | Admin | Update company |
| GET | /tax-slabs | Admin | List tax slabs |
| POST | /tax-slabs | Admin | Create tax slab |
| GET | /leave-types | Admin | List leave types |
| POST | /leave-types | Admin | Create leave type |

---

## 5. Backend Architecture

### Middleware Pipeline
```
Request → RequestLoggingMiddleware → ExceptionMiddleware → 
  → JWT Auth → Authorization → Controllers → Response
```

### Authentication Flow
1. User logs in → server validates credentials (BCrypt) → returns JWT (60 min) + refresh token
2. Frontend stores tokens in localStorage
3. Axios interceptor attaches Bearer token to all requests
4. On 401, interceptor attempts token refresh (with queuing to prevent race conditions)
5. Refresh tokens are opaque, stored in `RefreshTokens` table, support revocation

### Key Design Decisions
- **No Repository pattern** — Services consume `PayrollDbContext` directly
- **Global exception handling** — `ExceptionMiddleware` maps 404/401/409/500
- **Request logging** — `RequestLoggingMiddleware` logs method, path, status, elapsed ms
- **SignalR** — `NotificationHub` pushes real-time notifications to `user_{userId}` groups
- **FluentValidation** — Registered globally via `AddValidatorsFromAssemblyContaining<>`
- **CORS** — Allows `http://localhost:3000`
- **Serilog** — Rolling daily file sinks at `logs/payroll-{yyyyMMdd}.log`

### Services Layer
| Service | Responsibility |
|---------|---------------|
| AuthService | BCrypt hashing, JWT generation, refresh token rotation |
| EmployeeService | CRUD, pagination, search, auto employee code generation |
| PayrollService | Payroll calculation, Excel/PDF export, salary slip generation |
| LeaveService | CRUD, approve/reject with audit, leave balance |
| SalaryService | Salary components, employee structure, deductions |
| ReportService | Salary register, tax summary, earnings, department summary |
| SettingsService | Company, tax slabs, leave types |
| AuditService | Generic audit logging (JSON serialization of old/new values) |
| CacheService | IMemoryCache wrapper (Get/Set/GetOrSet/Remove/Clear) |
| LookupCacheService | Cache salary components and leave types |

### Tax Calculations (Utils/TaxCalculator.cs)
- Income Tax — slab-based progressive taxation
- EPF — 12% of basic
- ESI — 0.75% of gross
- Professional Tax — Karnataka state rates

---

## 6. Frontend Architecture

### Route Map (14 routes)

| Path | Layout | Page | Role |
|------|--------|------|------|
| /login | None | Login | Public |
| / | — | Redirect → /dashboard | — |
| /dashboard | AdminLayout | Dashboard | Admin, HRManager |
| /employees | AdminLayout | EmployeeList | Admin, HRManager |
| /employees/new | AdminLayout | EmployeeForm | Admin, HRManager |
| /employees/:id | AdminLayout | EmployeeProfile | Admin, HRManager |
| /employees/:id/edit | AdminLayout | EmployeeForm | Admin, HRManager |
| /payroll | AdminLayout | PayrollList | Admin, HRManager |
| /payroll/process | AdminLayout | PayrollProcess | Admin, HRManager |
| /leaves | AdminLayout | LeaveList | All authenticated |
| /leaves/new | AdminLayout | LeaveForm | All authenticated |
| /reports | AdminLayout | Reports | Admin, HRManager |
| /settings | AdminLayout | Settings | Admin |
| /my-salary | EmployeeLayout | MySalary | Employee |

### Role-Based Layouts
- **AdminLayout**: Full sidebar menu (Dashboard, Employees, Payroll, Leave Management, Reports, Settings)
- **EmployeeLayout**: Limited menu (Dashboard, My Salary, Leave Management)

### State Management (Redux Toolkit)
| Slice | Key State | Async Thunks |
|-------|-----------|--------------|
| authSlice | user, accessToken, refreshToken, isAuthenticated | login, register, changePassword, logout, refreshToken |
| employeeSlice | items[], selected, pagination | fetchEmployees, fetchById, create, update |
| payrollSlice | items[], selected | fetchPayrolls, processPayroll |
| uiSlice | sidebarOpen, theme, selectedMonth, selectedYear | — |

### Axios Service Layer
- `services/axios.js` — Base axios instance with request interceptor (Bearer token) and response interceptor (401 auto-refresh with request queuing)
- 7 domain-specific service modules: auth, employee, payroll, leave, salary, report, settings

### Reusable Components (Common/)
| Component | Purpose |
|-----------|---------|
| DataTable | Generic sortable/selectable MUI table with pagination |
| PageHeader | Title + subtitle + breadcrumbs + action button |
| StatCard | Metric card with icon, color, hover effect |
| StatusBadge | Colored MUI Chip mapped via getStatusColor() |
| SearchBar | Debounced text input with search icon |
| Pagination | MuiPagination with "start-end of total" label |
| Loading | Spinner, Skeleton, TableSkeleton |
| ErrorBoundary | Class component with fallback UI |
| ConfirmDialog | Confirmation modal with severity color |
| EmptyState | Icon + message + optional CTA |

### Custom Hooks
| Hook | From Slice | Exposes |
|------|-----------|---------|
| useAuth | auth | login, logout, changePassword, hasRole |
| useEmployees | employee | fetchEmployees, fetchById, create, update |
| usePayroll | payroll | fetchPayrolls, processPayroll |
| useUI | ui | toggleSidebar, setTheme, setMonth, setYear |

---

## 7. Authentication & Authorization

| Concept | Implementation |
|---------|---------------|
| Password hashing | BCrypt (via BCrypt.Net-Next) |
| Access token | JWT, 60 min expiry, contains userId, email, role |
| Refresh token | Opaque GUID, stored in DB, revocable |
| Token storage | localStorage (frontend) |
| Auth interceptor | Axios request interceptor adds Bearer header |
| Token refresh | Axios response interceptor catches 401, queues requests, rotates token |
| Role guard | `ProtectedRoute` component checks `isAuthenticated` + optional role array |
| Password policy | Min 8 chars, upper + lower + digit + special character |
| Session timeout | 30 min inactivity (TOAST_CONFIG in constants) |

---

## 8. Configuration Summary

### Backend (appsettings.json)
- **Database**: `Server=DESKTOP-0QT1J2J\SQLEXPRESS;Database=PayrollDb;Trusted_Connection=True;TrustServerCertificate=True`
- **JWT**: Secret key, issuer, audience, 60-min expiry
- **CORS**: Allows `http://localhost:3000`
- **Serilog**: Rolling file sink, minimum level Information

### Frontend (package.json dependencies)
- **Core**: React 19, ReactDOM 19
- **UI**: MUI 9 (@mui/material, @mui/icons-material, @mui/x-date-pickers), Tailwind CSS 3.4
- **State**: Redux Toolkit 2.12, react-redux 9
- **Routing**: react-router-dom 7.6
- **Forms**: react-hook-form 7.55
- **Charts**: recharts 3.9
- **HTTP**: axios 1.9
- **Dates**: dayjs 1.11
- **Dev**: TypeScript 6.0, PostCSS, autoprefixer

---

## 9. Known Limitations / TODO

1. **Reports, Settings, MySalary pages** use static/demo placeholder data instead of live API calls
2. **PayrollProcess and PayrollList** have some static data mixed with API calls
3. **Only 2 test files** exist (App.test.js smoke test + setupTests.js)
4. **No Docker/containerization** for development or deployment
5. **No CI/CD pipeline** configuration
6. **Password reset** (`/forgot-password`) endpoint exists but no frontend UI
7. **Document upload** for employees has API endpoints but no frontend implementation
8. **Bulk import** endpoint exists but no frontend UI
9. **Application-level monitoring/health checks** not implemented
10. **No database migration strategy** documented for production deployments

---

## 10. Technology Stack Summary

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend Framework | React (CRA) | 19 |
| Type System | TypeScript | 6.0 |
| UI Library | MUI (Material-UI) | 9 |
| Styling | Tailwind CSS | 3.4 |
| State Management | Redux Toolkit | 2.12 |
| Forms | react-hook-form | 7.55 |
| HTTP Client | axios | 1.9 |
| Charts | recharts | 3.9 |
| Date Handling | dayjs | 1.11 |
| Backend Framework | ASP.NET Core | 9 |
| ORM | Entity Framework Core | 9.0.4 |
| Database | SQL Server | — |
| Auth | JWT (Bearer) + BCrypt | — |
| Validation | FluentValidation | 11.11 |
| Logging | Serilog | 4.2 |
| API Docs | Swagger (Swashbuckle) | 7.3 |
| Real-time | SignalR | — |
| PDF | QuestPDF | — |
