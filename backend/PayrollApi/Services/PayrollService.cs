using System.Text;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Constants;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PayrollApi.Services;

public class PayrollService : IPayrollService
{
    private readonly PayrollDbContext _context;
    private readonly ISalaryCalculationService _salaryCalculation;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PayrollService(PayrollDbContext context, ISalaryCalculationService salaryCalculation,
        IAuditService auditService, INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _salaryCalculation = salaryCalculation;
        _auditService = auditService;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

    public async Task<PayrollListResponse> GetAllAsync(int? month, int? year, string? status, int page, int pageSize)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .AsQueryable();

        if (month.HasValue)
            query = query.Where(p => p.PayrollMonth.Month == month.Value);
        if (year.HasValue)
            query = query.Where(p => p.PayrollMonth.Year == year.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status.ToString() == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PayrollDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeCode = p.Employee.EmployeeCode,
                EmployeeName = p.Employee.FirstName + " " + p.Employee.LastName,
                Department = p.Employee.Department ?? "",
                Month = p.PayrollMonth.Month,
                Year = p.PayrollMonth.Year,
                GrossSalary = p.GrossSalary,
                TaxDeduction = p.TaxDeduction,
                OtherDeductions = p.OtherDeductions,
                NetSalary = p.NetSalary,
                Status = p.Status.ToString(),
                ProcessedDate = p.ProcessedDate,
                Remarks = p.Remarks
            })
            .ToListAsync();

        return new PayrollListResponse { Items = items, Total = total };
    }

    public async Task<PayrollDto> GetByIdAsync(Guid id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .Include(p => p.PayrollDetails)
                .ThenInclude(pd => pd.SalaryComponent)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Payroll with ID {id} not found");

        return new PayrollDto
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeCode = payroll.Employee.EmployeeCode,
            EmployeeName = $"{payroll.Employee.FirstName} {payroll.Employee.LastName}",
            Department = payroll.Employee.Department ?? "",
            Month = payroll.PayrollMonth.Month,
            Year = payroll.PayrollMonth.Year,
            GrossSalary = payroll.GrossSalary,
            TaxDeduction = payroll.TaxDeduction,
            OtherDeductions = payroll.OtherDeductions,
            NetSalary = payroll.NetSalary,
            Status = payroll.Status.ToString(),
            ProcessedDate = payroll.ProcessedDate,
            Remarks = payroll.Remarks
        };
    }

    public async Task<PayrollDto> ProcessAsync(ProcessPayrollRequest request)
    {
        var payrollMonth = await GetOrCreatePayrollMonth(request.Month, request.Year);

        var employees = await _context.Employees
            .Where(e => request.EmployeeIds.Contains(e.Id) && e.IsActive)
            .Include(e => e.SalaryStructures)
                .ThenInclude(ss => ss.SalaryComponent)
            .Include(e => e.Deductions)
            .ToListAsync();

        PayrollDto? lastDto = null;

        foreach (var employee in employees)
        {
            var existingPayroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id && p.PayrollMonthId == payrollMonth.Id);

            if (existingPayroll != null)
                continue;

            var breakdown = await _salaryCalculation.CalculateAsync(employee.Id, request.Month, request.Year);
            var taxDeduction = breakdown.Deductions
                .FirstOrDefault(d => d.ComponentName.Contains("Income Tax") || d.ComponentName.Contains("TDS"))
                ?.Amount ?? 0m;
            var otherDeductions = breakdown.TotalDeductions - taxDeduction;

            var payroll = new Payroll
            {
                EmployeeId = employee.Id,
                PayrollMonthId = payrollMonth.Id,
                GrossSalary = breakdown.GrossEarnings,
                TaxDeduction = taxDeduction,
                OtherDeductions = otherDeductions,
                NetSalary = breakdown.NetSalary,
                Status = PayrollStatus.Draft,
                ProcessedDate = DateTime.UtcNow,
                Remarks = request.Remarks
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            foreach (var earning in breakdown.Earnings)
            {
                var component = await _context.SalaryComponents
                    .FirstOrDefaultAsync(sc => sc.Name == earning.ComponentName && sc.Type == SalaryComponentType.Earning)
                    ?? _context.SalaryComponents.Local
                        .FirstOrDefault(sc => sc.Name == earning.ComponentName && sc.Type == SalaryComponentType.Earning);

                if (component != null)
                {
                    _context.PayrollDetails.Add(new PayrollDetail
                    {
                        PayrollId = payroll.Id,
                        SalaryComponentId = component.Id,
                        Amount = earning.Amount
                    });
                }
            }

            foreach (var deduction in breakdown.Deductions)
            {
                var component = await _context.SalaryComponents
                    .FirstOrDefaultAsync(sc => sc.Name == deduction.ComponentName && sc.Type == SalaryComponentType.Deduction)
                    ?? _context.SalaryComponents.Local
                        .FirstOrDefault(sc => sc.Name == deduction.ComponentName && sc.Type == SalaryComponentType.Deduction);

                if (component != null)
                {
                    _context.PayrollDetails.Add(new PayrollDetail
                    {
                        PayrollId = payroll.Id,
                        SalaryComponentId = component.Id,
                        Amount = deduction.Amount
                    });
                }
            }

            lastDto = new PayrollDto
            {
                Id = payroll.Id,
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Department = employee.Department ?? "",
                Month = request.Month,
                Year = request.Year,
                GrossSalary = breakdown.GrossEarnings,
                TaxDeduction = taxDeduction,
                OtherDeductions = otherDeductions,
                NetSalary = breakdown.NetSalary,
                Status = PayrollStatus.Draft.ToString(),
                ProcessedDate = payroll.ProcessedDate,
                Remarks = request.Remarks
            };
        }

        await _context.SaveChangesAsync();

        foreach (var empId in request.EmployeeIds)
        {
            await _notificationService.CreateAndSendAsync(empId, new Models.DTOs.CreateNotificationRequest
            {
                Title = "Payroll Processed",
                Message = $"Your payroll for {request.Month}/{request.Year} has been processed.",
                Link = "/my-salary"
            });
        }

        await _auditService.LogAsync(EntityNames.Payroll, $"{request.Month}/{request.Year}", "Process",
            null, new { request.EmployeeIds, request.Month, request.Year }, CurrentUserId, null);

        return lastDto ?? new PayrollDto { Status = PayrollStatus.Draft.ToString() };
    }

    public async Task<PayrollDto> UpdateAsync(Guid id, UpdatePayrollRequest request)
    {
        var payroll = await _context.Payrolls.FindAsync(id)
            ?? throw new KeyNotFoundException($"Payroll with ID {id} not found");

        if (request.TaxDeduction.HasValue) payroll.TaxDeduction = request.TaxDeduction.Value;
        if (request.OtherDeductions.HasValue) payroll.OtherDeductions = request.OtherDeductions.Value;
        if (request.Remarks != null) payroll.Remarks = request.Remarks;
        if (request.Status != null) payroll.Status = Enum.Parse<PayrollStatus>(request.Status, true);

        payroll.NetSalary = payroll.GrossSalary - payroll.TaxDeduction - payroll.OtherDeductions;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Payroll, id.ToString(), "Update", null,
            new { payroll.TaxDeduction, payroll.OtherDeductions, payroll.NetSalary, payroll.Status }, CurrentUserId, null);

        return await GetByIdAsync(id);
    }

    public async Task<byte[]> GetSalarySlipAsync(Guid id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .Include(p => p.PayrollDetails)
                .ThenInclude(pd => pd.SalaryComponent)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Payroll with ID {id} not found");

        var earnings = payroll.PayrollDetails
            .Where(d => d.SalaryComponent.Type == SalaryComponentType.Earning)
            .ToList();
        var deductions = payroll.PayrollDetails
            .Where(d => d.SalaryComponent.Type == SalaryComponentType.Deduction)
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(style => style.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => c.Column(col =>
                {
                    col.Item().AlignCenter().Text("PAYROLL SOLUTIONS INC.")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken4);
                    col.Item().AlignCenter().Text($"Salary Slip for {payroll.PayrollMonth.Month}/{payroll.PayrollMonth.Year}")
                        .FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().LineHorizontal(2).LineColor(Colors.Blue.Darken4);
                }));

                page.Content().Element(c => c.Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn(2);
                        });
                        table.Cell().Text("Employee:").Bold().FontColor(Colors.Grey.Darken2);
                        table.Cell().Text($"{payroll.Employee.FirstName} {payroll.Employee.LastName}");
                        table.Cell().Text("Code:").Bold().FontColor(Colors.Grey.Darken2);
                        table.Cell().Text(payroll.Employee.EmployeeCode);
                        table.Cell().Text("Department:").Bold().FontColor(Colors.Grey.Darken2);
                        table.Cell().Text(payroll.Employee.Department ?? "");
                        table.Cell().Text("Status:").Bold().FontColor(Colors.Grey.Darken2);
                        table.Cell().Text(payroll.Status.ToString());
                    });

                    col.Item().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Component").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Amount").FontColor(Colors.White).Bold().AlignRight();
                        });

                        foreach (var e in earnings)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.SalaryComponent.Name);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{e.Amount:N2}").AlignRight();
                        }

                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Gross Salary").Bold();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{payroll.GrossSalary:N2}").AlignRight().Bold();

                        foreach (var d in deductions)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(d.SalaryComponent.Name);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"({d.Amount:N2})").AlignRight().FontColor(Colors.Red.Darken2);
                        }

                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Tax Deduction").Bold();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"({payroll.TaxDeduction:N2})").AlignRight().FontColor(Colors.Red.Darken2);
                    });

                    col.Item().Background(Colors.Green.Lighten4).Padding(12).AlignCenter().Row(row =>
                    {
                        row.AutoItem().Text("Net Payable: ").FontSize(12).Bold();
                        row.AutoItem().Text($"₹ {payroll.NetSalary:N2}").FontSize(16).Bold().FontColor(Colors.Blue.Darken4);
                    });
                }));

                page.Footer().AlignCenter().Text("This is a computer-generated document")
                    .FontSize(9).FontColor(Colors.Grey.Darken2);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ExportCsvAsync(int? month, int? year, string? status)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .AsQueryable();

        if (month.HasValue) query = query.Where(p => p.PayrollMonth.Month == month);
        if (year.HasValue) query = query.Where(p => p.PayrollMonth.Year == year);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status.ToString() == status);

        var data = await query
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new
            {
                p.Employee.EmployeeCode,
                EmployeeName = p.Employee.FirstName + " " + p.Employee.LastName,
                p.Employee.Department,
                Month = p.PayrollMonth.Month,
                Year = p.PayrollMonth.Year,
                p.GrossSalary,
                p.TaxDeduction,
                p.OtherDeductions,
                p.NetSalary,
                p.Status
            })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Employee Code,Employee Name,Department,Period,Gross Salary,Tax Deduction,Other Deductions,Net Salary,Status");
        foreach (var row in data)
        {
            sb.AppendLine($"{row.EmployeeCode},{row.EmployeeName},{row.Department},{row.Month}/{row.Year},{row.GrossSalary},{row.TaxDeduction},{row.OtherDeductions},{row.NetSalary},{row.Status}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportPdfAsync(Guid id)
    {
        return await GetSalarySlipAsync(id);
    }

    public async Task<string> GenerateSlipAsync(Guid id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Payroll with ID {id} not found");

        payroll.Status = PayrollStatus.Processed;
        await _context.SaveChangesAsync();

        return $"Slip generated for payroll ID {id}";
    }

    public async Task<PayrollListResponse> GetByMonthYearAsync(int month, int year)
    {
        return await GetAllAsync(month, year, null, 1, int.MaxValue);
    }

    public async Task<List<PayrollDto>> BulkProcessAsync(BulkProcessPayrollRequest request)
    {
        var payrollMonth = await _context.PayrollMonths
            .FirstOrDefaultAsync(pm => pm.Month == request.Month && pm.Year == request.Year)
            ?? throw new InvalidOperationException($"Payroll period {request.Month}/{request.Year} not found");

        var employees = await _context.Employees
            .Where(e => request.EmployeeIds.Contains(e.Id) && e.IsActive && !e.IsDeleted)
            .ToListAsync();

        var existingPayrolls = await _context.Payrolls
            .Where(p => request.EmployeeIds.Contains(p.EmployeeId) && p.PayrollMonthId == payrollMonth.Id)
            .ToDictionaryAsync(p => p.EmployeeId);

        var result = new List<PayrollDto>();

        foreach (var employee in employees)
        {
            var breakdown = await _salaryCalculation.CalculateAsync(employee.Id, request.Month, request.Year);
            var existingPayroll = existingPayrolls.GetValueOrDefault(employee.Id);

            var payroll = new Payroll
            {
                EmployeeId = employee.Id,
                PayrollMonthId = payrollMonth.Id,
                GrossSalary = breakdown.GrossEarnings,
                TaxDeduction = breakdown.Deductions.Where(d => d.ComponentName.Contains("Tax")).Sum(d => d.Amount),
                OtherDeductions = breakdown.TotalDeductions - breakdown.Deductions.Where(d => d.ComponentName.Contains("Tax")).Sum(d => d.Amount),
                NetSalary = breakdown.NetSalary,
                Status = PayrollStatus.Draft,
                CreatedDate = DateTime.UtcNow
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            foreach (var component in breakdown.Earnings.Concat(breakdown.Deductions))
            {
                var salaryComponent = await _context.SalaryComponents
                    .FirstOrDefaultAsync(sc => sc.Name == component.ComponentName);
                if (salaryComponent == null) continue;

                _context.PayrollDetails.Add(new PayrollDetail
                {
                    PayrollId = payroll.Id,
                    SalaryComponentId = salaryComponent.Id,
                    Amount = component.Amount
                });
            }

            result.Add(new PayrollDto
            {
                Id = payroll.Id,
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Department = employee.Department ?? "",
                Month = payrollMonth.Month,
                Year = payrollMonth.Year,
                GrossSalary = payroll.GrossSalary,
                TaxDeduction = payroll.TaxDeduction,
                OtherDeductions = payroll.OtherDeductions,
                NetSalary = payroll.NetSalary,
                Status = PayrollStatus.Draft.ToString(),
                ProcessedDate = null
            });
        }

        await _context.SaveChangesAsync();

        var firstDto = result.FirstOrDefault();
        if (firstDto != null)
        {
            foreach (var employee in employees)
            {
                await _notificationService.CreateAndSendAsync(employee.UserId, new CreateNotificationRequest
                {
                    Title = "Payroll Processed",
                    Message = $"Your payroll for {request.Month}/{request.Year} has been processed.",
                    Link = "/my-salary"
                });
            }
        }

        await _auditService.LogAsync(EntityNames.Payroll, $"{request.Month}/{request.Year}", "BulkProcess",
            null, new { request.EmployeeIds, request.Month, request.Year }, CurrentUserId, null);

        return result;
    }

    private async Task<PayrollMonth> GetOrCreatePayrollMonth(int month, int year)
    {
        var payrollMonth = await _context.PayrollMonths
            .FirstOrDefaultAsync(pm => pm.Month == month && pm.Year == year);

        if (payrollMonth == null)
        {
            payrollMonth = new PayrollMonth
            {
                Month = month,
                Year = year,
                StartDate = new DateTime(year, month, 1),
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                IsLocked = false,
                Status = PayrollStatus.Draft
            };
            _context.PayrollMonths.Add(payrollMonth);
            await _context.SaveChangesAsync();
        }

        return payrollMonth;
    }
}
