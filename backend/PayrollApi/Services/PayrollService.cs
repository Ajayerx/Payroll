using System.Text;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;

namespace PayrollApi.Services;

public class PayrollService : IPayrollService
{
    private readonly PayrollDbContext _context;
    private readonly ISalaryCalculationService _salaryCalculation;

    public PayrollService(PayrollDbContext context, ISalaryCalculationService salaryCalculation)
    {
        _context = context;
        _salaryCalculation = salaryCalculation;
    }

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

        var earningsRows = string.Join("", earnings.Select(e =>
            $"<tr><td style='padding:6px 12px;border:1px solid #ddd'>{e.SalaryComponent.Name}</td><td style='padding:6px 12px;border:1px solid #ddd;text-align:right'>{e.Amount:N2}</td></tr>"));
        var deductionRows = string.Join("", deductions.Select(d =>
            $"<tr><td style='padding:6px 12px;border:1px solid #ddd'>{d.SalaryComponent.Name}</td><td style='padding:6px 12px;border:1px solid #ddd;text-align:right'>({d.Amount:N2})</td></tr>"));

        var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>Salary Slip</title></head><body>
<div style='max-width:700px;margin:30px auto;font-family:Arial,sans-serif;border:2px solid #1a237e;border-radius:8px;padding:30px'>
<div style='text-align:center;border-bottom:2px solid #1a237e;padding-bottom:20px;margin-bottom:20px'>
<h1 style='color:#1a237e;margin:0;font-size:24px'>PAYROLL SOLUTIONS INC.</h1>
<p style='color:#666;margin:5px 0 0'>Salary Slip for {payroll.PayrollMonth.Month}/{payroll.PayrollMonth.Year}</p>
</div>
<table style='width:100%;border-collapse:collapse;margin-bottom:20px'>
<tr><td style='padding:4px 8px;color:#666;width:120px'>Employee</td><td style='padding:4px 8px;font-weight:600'>{payroll.Employee.FirstName} {payroll.Employee.LastName}</td></tr>
<tr><td style='padding:4px 8px;color:#666'>Code</td><td style='padding:4px 8px'>{payroll.Employee.EmployeeCode}</td></tr>
<tr><td style='padding:4px 8px;color:#666'>Department</td><td style='padding:4px 8px'>{payroll.Employee.Department}</td></tr>
<tr><td style='padding:4px 8px;color:#666'>Status</td><td style='padding:4px 8px'>{payroll.Status}</td></tr>
</table>
<table style='width:100%;border-collapse:collapse;margin-bottom:15px'>
<tr style='background:#1a237e;color:#fff'><th style='padding:8px 12px;text-align:left'>Component</th><th style='padding:8px 12px;text-align:right'>Amount</th></tr>
{earningsRows}
<tr style='background:#f5f5f5'><td style='padding:8px 12px;border:1px solid #ddd;font-weight:600'>Gross Salary</td><td style='padding:8px 12px;border:1px solid #ddd;text-align:right;font-weight:600'>{payroll.GrossSalary:N2}</td></tr>
{deductionRows}
<tr style='background:#f5f5f5'><td style='padding:8px 12px;border:1px solid #ddd;font-weight:600'>Tax Deduction</td><td style='padding:8px 12px;border:1px solid #ddd;text-align:right;color:#d32f2f'>({payroll.TaxDeduction:N2})</td></tr>
</table>
<div style='background:#e8f5e9;padding:12px 20px;border-radius:6px;text-align:center'>
<span style='color:#333;font-size:14px'>Net Payable</span>
<span style='color:#1a237e;font-size:22px;font-weight:700;margin-left:15px'>&#8377; {payroll.NetSalary:N2}</span>
</div>
<p style='text-align:center;color:#999;font-size:11px;margin-top:25px'>This is a computer-generated document</p>
</div></body></html>";

        return Encoding.UTF8.GetBytes(html);
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
        var result = new List<PayrollDto>();

        foreach (var empId in request.EmployeeIds)
        {
            var payrollDto = await ProcessAsync(new ProcessPayrollRequest
            {
                EmployeeIds = [empId],
                Month = request.Month,
                Year = request.Year
            });
            result.Add(payrollDto);
        }

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
