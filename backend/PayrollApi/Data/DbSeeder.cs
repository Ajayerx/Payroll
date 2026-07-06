using System.Text;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;

namespace PayrollApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(PayrollDbContext db, string contentRootPath)
    {
        if (await db.CompanySettings.AnyAsync()) return;

        // ── Create Stored Procedures ─────────────────────────────────
        var spPath = Path.Combine(contentRootPath, "DbScripts", "001_StoredProcedures.sql");
        var spFile = new FileInfo(spPath);
        if (spFile.Exists)
        {
            var sql = await File.ReadAllTextAsync(spFile.FullName, Encoding.UTF8);
            foreach (var batch in sql.Split("GO", StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = batch.Trim();
                if (trimmed.Length > 0)
                {
                    try { await db.Database.ExecuteSqlRawAsync(trimmed); }
                    catch (Exception ex) { Console.WriteLine($"SP batch skipped: {ex.Message}"); }
                }
            }
        }

        var now = DateTime.UtcNow;
        var adminUserId = Guid.Parse("A1111111-1111-1111-1111-111111111111");
        var hrUserId = Guid.Parse("A2222222-2222-2222-2222-222222222222");
        var rajeshUserId = Guid.Parse("A3333333-3333-3333-3333-333333333333");
        var priyaUserId = Guid.Parse("A4444444-4444-4444-4444-444444444444");
        var amitUserId = Guid.Parse("A5555555-5555-5555-5555-555555555555");
        var snehaUserId = Guid.Parse("A6666666-6666-6666-6666-666666666666");

        // ── Users ──────────────────────────────────────────────────────
        var users = new List<User>
        {
            new()
            {
                Id = adminUserId, Email = "admin@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System", LastName = "Admin", Role = UserRole.Admin,
                IsActive = true, CreatedDate = now
            },
            new()
            {
                Id = hrUserId, Email = "hr@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Hr@12345"),
                FirstName = "Priya", LastName = "Verma", Role = UserRole.HRManager,
                IsActive = true, CreatedDate = now
            },
            new()
            {
                Id = rajeshUserId, Email = "rajesh.kumar@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
                FirstName = "Rajesh", LastName = "Kumar", Role = UserRole.Employee,
                IsActive = true, CreatedDate = now
            },
            new()
            {
                Id = priyaUserId, Email = "priya.sharma@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
                FirstName = "Priya", LastName = "Sharma", Role = UserRole.Employee,
                IsActive = true, CreatedDate = now
            },
            new()
            {
                Id = amitUserId, Email = "amit.verma@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
                FirstName = "Amit", LastName = "Verma", Role = UserRole.Employee,
                IsActive = true, CreatedDate = now
            },
            new()
            {
                Id = snehaUserId, Email = "sneha.patel@payroll.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
                FirstName = "Sneha", LastName = "Patel", Role = UserRole.Employee,
                IsActive = true, CreatedDate = now
            }
        };
        db.Users.AddRange(users);

        // ── Company Settings ──────────────────────────────────────────
        var companySetting = new CompanySetting
        {
            Id = Guid.Parse("AA111111-1111-1111-1111-111111111111"),
            CompanyName = "TechSolutions India Pvt Ltd",
            Address = "123, Tech Park, Electronic City, Bangalore - 560100",
            Email = "contact@techsolutions.com",
            Phone = "+91-80-41234567",
            Gstin = "29AABCT1234F1Z5",
            Pan = "AABCT1234F",
            CreatedBy = adminUserId, CreatedDate = now
        };
        db.CompanySettings.Add(companySetting);

        // ── Salary Components ─────────────────────────────────────────
        var basicPayId = Guid.Parse("C1111111-1111-1111-1111-111111111111");
        var hraId = Guid.Parse("C2222222-2222-2222-2222-222222222222");
        var daId = Guid.Parse("C3333333-3333-3333-3333-333333333333");
        var conveyanceId = Guid.Parse("C4444444-4444-4444-4444-444444444444");
        var medicalId = Guid.Parse("C5555555-5555-5555-5555-555555555555");
        var specialId = Guid.Parse("C6666666-6666-6666-6666-666666666666");
        var pfId = Guid.Parse("D1111111-1111-1111-1111-111111111111");
        var ptId = Guid.Parse("D2222222-2222-2222-2222-222222222222");
        var incomeTaxId = Guid.Parse("D3333333-3333-3333-3333-333333333333");

        var salaryComponents = new List<SalaryComponent>
        {
            new() { Id = basicPayId, Name = "Basic Pay", Type = SalaryComponentType.Earning, IsVariable = false, Description = "Base salary component", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = hraId, Name = "House Rent Allowance", Type = SalaryComponentType.Earning, IsVariable = false, Description = "HRA for rental accommodation", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = daId, Name = "Dearness Allowance", Type = SalaryComponentType.Earning, IsVariable = false, Description = "Cost of living adjustment", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = conveyanceId, Name = "Conveyance Allowance", Type = SalaryComponentType.Earning, IsVariable = false, Description = "Travel allowance", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = medicalId, Name = "Medical Allowance", Type = SalaryComponentType.Earning, IsVariable = false, Description = "Medical reimbursement", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = specialId, Name = "Special Allowance", Type = SalaryComponentType.Earning, IsVariable = true, Description = "Performance-linked variable pay", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = pfId, Name = "Provident Fund", Type = SalaryComponentType.Deduction, IsVariable = false, Description = "Employee PF contribution (12% of basic)", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = ptId, Name = "Professional Tax", Type = SalaryComponentType.Deduction, IsVariable = false, Description = "State professional tax", CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = incomeTaxId, Name = "Income Tax", Type = SalaryComponentType.Deduction, IsVariable = true, Description = "TDS on salary", CreatedBy = adminUserId, CreatedDate = now }
        };
        db.SalaryComponents.AddRange(salaryComponents);

        // ── Tax Slabs ────────────────────────────────────────────────
        var taxSlabs = new List<TaxSlab>
        {
            new() { Id = Guid.Parse("E1111111-1111-1111-1111-111111111111"), Name = "Nil Slab", FromAmount = 0, ToAmount = 250000, Rate = 0, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = Guid.Parse("E2222222-2222-2222-2222-222222222222"), Name = "5% Slab", FromAmount = 250001, ToAmount = 500000, Rate = 5, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = Guid.Parse("E3333333-3333-3333-3333-333333333333"), Name = "10% Slab", FromAmount = 500001, ToAmount = 1000000, Rate = 10, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = Guid.Parse("E4444444-4444-4444-4444-444444444444"), Name = "20% Slab", FromAmount = 1000001, ToAmount = null, Rate = 20, CreatedBy = adminUserId, CreatedDate = now }
        };
        db.TaxSlabs.AddRange(taxSlabs);

        // ── Leave Types ───────────────────────────────────────────────
        var annualLeaveId = Guid.Parse("F1111111-1111-1111-1111-111111111111");
        var sickLeaveId = Guid.Parse("F2222222-2222-2222-2222-222222222222");
        var casualLeaveId = Guid.Parse("F3333333-3333-3333-3333-333333333333");
        var maternityLeaveId = Guid.Parse("F4444444-4444-4444-4444-444444444444");
        var paternityLeaveId = Guid.Parse("F5555555-5555-5555-5555-555555555555");
        var compOffId = Guid.Parse("F6666666-6666-6666-6666-666666666666");

        var leaveTypes = new List<LeaveType>
        {
            new() { Id = annualLeaveId, Name = "Annual Leave", DaysPerYear = 15, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = sickLeaveId, Name = "Sick Leave", DaysPerYear = 12, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = casualLeaveId, Name = "Casual Leave", DaysPerYear = 10, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = maternityLeaveId, Name = "Maternity Leave", DaysPerYear = 180, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = paternityLeaveId, Name = "Paternity Leave", DaysPerYear = 15, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now },
            new() { Id = compOffId, Name = "Compensatory Off", DaysPerYear = null, IsPaid = true, CreatedBy = adminUserId, CreatedDate = now }
        };
        db.LeaveTypes.AddRange(leaveTypes);

        await db.SaveChangesAsync();

        // ── Employees ─────────────────────────────────────────────────
        var adminEmpId = Guid.Parse("B1111111-1111-1111-1111-111111111111");
        var rajeshEmpId = Guid.Parse("B2222222-2222-2222-2222-222222222222");
        var priyaEmpId = Guid.Parse("B3333333-3333-3333-3333-333333333333");
        var amitEmpId = Guid.Parse("B4444444-4444-4444-4444-444444444444");
        var snehaEmpId = Guid.Parse("B5555555-5555-5555-5555-555555555555");

        var employees = new List<Employee>
        {
            new()
            {
                Id = adminEmpId, UserId = adminUserId, EmployeeCode = "ADM001",
                FirstName = "System", LastName = "Admin",
                Email = "admin@payroll.com", Phone = "9876543210",
                DOB = new DateTime(1985, 5, 15), Gender = "Male",
                Address = "456, MG Road", City = "Bangalore", State = "Karnataka", Pincode = "560001",
                DateOfJoining = new DateTime(2020, 1, 1),
                Department = "Management", Designation = "Chief Executive Officer",
                BankName = "State Bank of India", BankAccount = "12345678901", IfscCode = "SBIN0001234",
                CreatedBy = adminUserId, CreatedDate = now
            },
            new()
            {
                Id = rajeshEmpId, UserId = rajeshUserId, EmployeeCode = "IT001",
                FirstName = "Rajesh", LastName = "Kumar",
                Email = "rajesh.kumar@payroll.com", Phone = "9876543211",
                DOB = new DateTime(1992, 8, 22), Gender = "Male",
                Address = "789, 2nd Cross, Indiranagar", City = "Bangalore", State = "Karnataka", Pincode = "560038",
                DateOfJoining = new DateTime(2022, 1, 15),
                Department = "Information Technology", Designation = "Senior Software Engineer",
                BankName = "HDFC Bank", BankAccount = "23456789012", IfscCode = "HDFC0005678",
                CreatedBy = adminUserId, CreatedDate = now
            },
            new()
            {
                Id = priyaEmpId, UserId = priyaUserId, EmployeeCode = "IT002",
                FirstName = "Priya", LastName = "Sharma",
                Email = "priya.sharma@payroll.com", Phone = "9876543212",
                DOB = new DateTime(1995, 12, 5), Gender = "Female",
                Address = "321, 5th Main, JP Nagar", City = "Bangalore", State = "Karnataka", Pincode = "560078",
                DateOfJoining = new DateTime(2023, 6, 1),
                Department = "Information Technology", Designation = "Software Engineer",
                BankName = "ICICI Bank", BankAccount = "34567890123", IfscCode = "ICIC0009012",
                CreatedBy = adminUserId, CreatedDate = now
            },
            new()
            {
                Id = amitEmpId, UserId = amitUserId, EmployeeCode = "HR001",
                FirstName = "Amit", LastName = "Verma",
                Email = "amit.verma@payroll.com", Phone = "9876543213",
                DOB = new DateTime(1990, 3, 18), Gender = "Male",
                Address = "654, 1st Stage, BTM Layout", City = "Bangalore", State = "Karnataka", Pincode = "560068",
                DateOfJoining = new DateTime(2021, 8, 15),
                Department = "Human Resources", Designation = "HR Manager",
                BankName = "Axis Bank", BankAccount = "45678901234", IfscCode = "UTIB0003456",
                CreatedBy = adminUserId, CreatedDate = now
            },
            new()
            {
                Id = snehaEmpId, UserId = snehaUserId, EmployeeCode = "ACC001",
                FirstName = "Sneha", LastName = "Patel",
                Email = "sneha.patel@payroll.com", Phone = "9876543214",
                DOB = new DateTime(1993, 7, 30), Gender = "Female",
                Address = "987, 3rd Phase, Whitefield", City = "Bangalore", State = "Karnataka", Pincode = "560066",
                DateOfJoining = new DateTime(2023, 1, 1),
                Department = "Accounts", Designation = "Accountant",
                BankName = "Kotak Mahindra", BankAccount = "56789012345", IfscCode = "KKBK0007890",
                CreatedBy = adminUserId, CreatedDate = now
            }
        };
        db.Employees.AddRange(employees);

        // ── Employee Salary Structures ────────────────────────────────
        // Admin: Gross = 150000
        // Rajesh: Gross = 90000
        // Priya: Gross = 65000
        // Amit: Gross = 85000
        // Sneha: Gross = 55000

        var grossAmounts = new Dictionary<Guid, decimal>
        {
            [adminEmpId] = 150000, [rajeshEmpId] = 90000, [priyaEmpId] = 65000,
            [amitEmpId] = 85000, [snehaEmpId] = 55000
        };

        var structures = new List<EmployeeSalaryStructure>();
        foreach (var (empId, gross) in grossAmounts)
        {
            structures.AddRange(new[]
            {
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = basicPayId, Amount = gross * 0.40m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = hraId, Amount = gross * 0.20m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = daId, Amount = gross * 0.10m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = conveyanceId, Amount = gross * 0.08m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = medicalId, Amount = gross * 0.07m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
                new EmployeeSalaryStructure { EmployeeId = empId, SalaryComponentId = specialId, Amount = gross * 0.15m, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now }
            });
        }
        db.EmployeeSalaryStructures.AddRange(structures);

        // ── Tax Configurations ────────────────────────────────────────
        var taxConfigs = new List<TaxConfiguration>
        {
            new() { EmployeeId = adminEmpId, TaxSlab = "20% Slab", TaxRate = 20, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
            new() { EmployeeId = rajeshEmpId, TaxSlab = "10% Slab", TaxRate = 10, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
            new() { EmployeeId = priyaEmpId, TaxSlab = "5% Slab", TaxRate = 5, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
            new() { EmployeeId = amitEmpId, TaxSlab = "10% Slab", TaxRate = 10, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now },
            new() { EmployeeId = snehaEmpId, TaxSlab = "5% Slab", TaxRate = 5, EffectiveDate = now, CreatedBy = adminUserId, CreatedDate = now }
        };
        db.TaxConfigurations.AddRange(taxConfigs);

        // ── Deductions ────────────────────────────────────────────────
        // PF = 12% of basic; PT = 200 for all
        var deductions = new List<Deduction>();
        foreach (var (empId, gross) in grossAmounts)
        {
            var pfAmt = gross * 0.40m * 0.12m;
            deductions.Add(new Deduction
            {
                EmployeeId = empId, Type = DeductionType.Loan, Amount = pfAmt,
                RemainingAmount = pfAmt * 11, StartDate = now.AddMonths(-6),
                CreatedBy = adminUserId, CreatedDate = now
            });
        }
        db.Deductions.AddRange(deductions);

        // ── Payroll Months ────────────────────────────────────────────
        var may2026Id = Guid.Parse("AB111111-1111-1111-1111-111111111111");
        var june2026Id = Guid.Parse("AB222222-2222-2222-2222-222222222222");

        var payrollMonths = new List<PayrollMonth>
        {
            new()
            {
                Id = may2026Id, Month = 5, Year = 2026,
                StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 5, 31),
                IsLocked = true, Status = PayrollStatus.Paid,
                CreatedBy = adminUserId, CreatedDate = now
            },
            new()
            {
                Id = june2026Id, Month = 6, Year = 2026,
                StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 30),
                IsLocked = false, Status = PayrollStatus.Draft,
                CreatedBy = adminUserId, CreatedDate = now
            }
        };
        db.PayrollMonths.AddRange(payrollMonths);

        await db.SaveChangesAsync();

        // ── Payroll + Payroll Details (May 2026) ──────────────────────
        foreach (var (empId, gross) in grossAmounts)
        {
            var basic = gross * 0.40m;
            var pf = basic * 0.12m;
            var pt = 200m;
            var tax = gross * 0.10m;
            var otherDeductions = pf + pt;
            var net = gross - tax - otherDeductions;

            var payroll = new Payroll
            {
                EmployeeId = empId, PayrollMonthId = may2026Id,
                GrossSalary = gross, TaxDeduction = tax,
                OtherDeductions = otherDeductions, NetSalary = net,
                Status = PayrollStatus.Paid,
                ProcessedDate = new DateTime(2026, 5, 28),
                Remarks = "May 2026 salary processed",
                CreatedBy = adminUserId, CreatedDate = now
            };
            db.Payrolls.Add(payroll);
            await db.SaveChangesAsync();

            var details = new List<PayrollDetail>
            {
                new() { PayrollId = payroll.Id, SalaryComponentId = basicPayId, Amount = gross * 0.40m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = hraId, Amount = gross * 0.20m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = daId, Amount = gross * 0.10m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = conveyanceId, Amount = gross * 0.08m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = medicalId, Amount = gross * 0.07m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = specialId, Amount = gross * 0.15m, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = pfId, Amount = pf, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = ptId, Amount = pt, CreatedBy = adminUserId, CreatedDate = now },
                new() { PayrollId = payroll.Id, SalaryComponentId = incomeTaxId, Amount = tax, CreatedBy = adminUserId, CreatedDate = now }
            };
            db.PayrollDetails.AddRange(details);
        }

        // ── Leave Requests ────────────────────────────────────────────
        var leaveRequests = new List<LeaveRequest>
        {
            new()
            {
                EmployeeId = rajeshEmpId, LeaveTypeId = casualLeaveId,
                FromDate = new DateTime(2026, 5, 4), ToDate = new DateTime(2026, 5, 5),
                TotalDays = 2, Reason = "Personal work",
                Status = "Approved", ApprovedBy = hrUserId,
                ApprovedDate = new DateTime(2026, 5, 2),
                CreatedBy = rajeshUserId, CreatedDate = now
            },
            new()
            {
                EmployeeId = priyaEmpId, LeaveTypeId = sickLeaveId,
                FromDate = new DateTime(2026, 5, 12), ToDate = new DateTime(2026, 5, 14),
                TotalDays = 3, Reason = "Viral fever",
                Status = "Approved", ApprovedBy = hrUserId,
                ApprovedDate = new DateTime(2026, 5, 11),
                CreatedBy = priyaUserId, CreatedDate = now
            },
            new()
            {
                EmployeeId = amitEmpId, LeaveTypeId = annualLeaveId,
                FromDate = new DateTime(2026, 6, 8), ToDate = new DateTime(2026, 6, 12),
                TotalDays = 5, Reason = "Family vacation",
                Status = "Pending",
                CreatedBy = amitUserId, CreatedDate = now
            },
            new()
            {
                EmployeeId = snehaEmpId, LeaveTypeId = casualLeaveId,
                FromDate = new DateTime(2026, 6, 18), ToDate = new DateTime(2026, 6, 18),
                TotalDays = 1, Reason = "Personal appointment",
                Status = "Approved", ApprovedBy = hrUserId,
                ApprovedDate = new DateTime(2026, 6, 16),
                CreatedBy = snehaUserId, CreatedDate = now
            },
            new()
            {
                EmployeeId = rajeshEmpId, LeaveTypeId = annualLeaveId,
                FromDate = new DateTime(2026, 7, 1), ToDate = new DateTime(2026, 7, 5),
                TotalDays = 5, Reason = "Planned leave",
                Status = "Pending",
                CreatedBy = rajeshUserId, CreatedDate = now
            }
        };
        db.LeaveRequests.AddRange(leaveRequests);

        await db.SaveChangesAsync();
    }
}
