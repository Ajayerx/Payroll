using System.Text;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Enums;

namespace PayrollApi.Services;

public class ReportService : IReportService
{
    private readonly PayrollDbContext _context;

    public ReportService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalaryRegisterDto>> GetSalaryRegisterAsync(SalaryRegisterRequest request)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .Include(p => p.PayrollDetails)
                .ThenInclude(pd => pd.SalaryComponent)
            .AsQueryable();

        if (request.Month.HasValue)
            query = query.Where(p => p.PayrollMonth.Month == request.Month);
        if (request.Year.HasValue)
            query = query.Where(p => p.PayrollMonth.Year == request.Year);
        if (!string.IsNullOrWhiteSpace(request.Department))
            query = query.Where(p => p.Employee.Department == request.Department);

        var payrolls = await query.ToListAsync();

        return payrolls.Select(p => new SalaryRegisterDto
        {
            EmployeeCode = p.Employee.EmployeeCode,
            EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
            Department = p.Employee.Department ?? "",
            Designation = p.Employee.Designation ?? "",
            Basic = p.PayrollDetails.Where(d => d.SalaryComponent.Name.Contains("Basic")).Sum(d => d.Amount),
            HRA = p.PayrollDetails.Where(d => d.SalaryComponent.Name.Contains("HRA")).Sum(d => d.Amount),
            DA = p.PayrollDetails.Where(d => d.SalaryComponent.Name.Contains("DA")).Sum(d => d.Amount),
            GrossPay = p.GrossSalary,
            Tax = p.TaxDeduction,
            OtherDeductions = p.OtherDeductions,
            NetPay = p.NetSalary
        }).ToList();
    }

    public async Task<object> GetTaxSummaryAsync(TaxSummaryRequest request)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .AsQueryable();

        if (request.Year.HasValue)
            query = query.Where(p => p.PayrollMonth.Year == request.Year);

        var payrolls = await query.ToListAsync();

        return new
        {
            TotalTaxDeducted = payrolls.Sum(p => p.TaxDeduction),
            AverageTax = payrolls.Count > 0 ? payrolls.Average(p => p.TaxDeduction) : 0,
            TaxedEmployees = payrolls.Where(p => p.TaxDeduction > 0).Select(p => p.EmployeeId).Distinct().Count(),
            TotalEmployees = payrolls.Select(p => p.EmployeeId).Distinct().Count(),
            Records = payrolls.Select(p => new
            {
                Employee = $"{p.Employee.FirstName} {p.Employee.LastName}",
                p.Employee.EmployeeCode,
                p.GrossSalary,
                p.TaxDeduction,
                p.NetSalary
            })
        };
    }

    public async Task<List<SalaryRegisterDto>> GetEmployeeEarningsAsync(EmployeeEarningsRequest request)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .Include(p => p.PayrollDetails)
                .ThenInclude(pd => pd.SalaryComponent)
            .AsQueryable();

        if (request.EmployeeId.HasValue)
            query = query.Where(p => p.EmployeeId == request.EmployeeId);
        if (request.Month.HasValue)
            query = query.Where(p => p.PayrollMonth.Month == request.Month);
        if (request.Year.HasValue)
            query = query.Where(p => p.PayrollMonth.Year == request.Year);

        var payrolls = await query.ToListAsync();

        return payrolls.Select(p => new SalaryRegisterDto
        {
            EmployeeCode = p.Employee.EmployeeCode,
            EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
            Department = p.Employee.Department ?? "",
            Designation = p.Employee.Designation ?? "",
            Basic = p.PayrollDetails.Where(d => d.SalaryComponent.Type == SalaryComponentType.Earning).Sum(d => d.Amount),
            GrossPay = p.GrossSalary,
            Tax = p.TaxDeduction,
            OtherDeductions = p.OtherDeductions,
            NetPay = p.NetSalary
        }).ToList();
    }

    public async Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync(DepartmentSummaryRequest request)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .AsQueryable();

        if (request.Month.HasValue)
            query = query.Where(p => p.PayrollMonth.Month == request.Month);
        if (request.Year.HasValue)
            query = query.Where(p => p.PayrollMonth.Year == request.Year);

        var payrolls = await query.ToListAsync();

        return payrolls
            .GroupBy(p => p.Employee.Department ?? "Unknown")
            .Select(g => new DepartmentSummaryDto
            {
                Department = g.Key,
                EmployeeCount = g.Select(p => p.EmployeeId).Distinct().Count(),
                TotalSalary = g.Sum(p => p.NetSalary),
                AverageSalary = g.Average(p => p.NetSalary)
            })
            .ToList();
    }

    public async Task<byte[]> ExportAsync(ExportRequest request, string format)
    {
        var data = await GetSalaryRegisterAsync(new SalaryRegisterRequest
        {
            Month = request.Month,
            Year = request.Year,
            Department = request.Department
        });

        return format.ToLower() switch
        {
            "csv" => GenerateCsv(data),
            "excel" => GenerateExcelHtml(data),
            "pdf" => GeneratePdfHtml(data),
            _ => GenerateCsv(data)
        };
    }

    private static byte[] GenerateCsv(List<SalaryRegisterDto> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Department,Designation,Gross,Tax,Deductions,Net");
        foreach (var item in data)
        {
            sb.AppendLine($"{item.EmployeeCode},{item.EmployeeName},{item.Department},{item.Designation},{item.GrossPay},{item.Tax},{item.OtherDeductions},{item.NetPay}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] GenerateExcelHtml(List<SalaryRegisterDto> data)
    {
        var rows = string.Join("", data.Select(d =>
            $"<tr><td>{d.EmployeeCode}</td><td>{d.EmployeeName}</td><td>{d.Department}</td><td>{d.Designation}</td><td align='right'>{d.GrossPay:N2}</td><td align='right'>{d.Tax:N2}</td><td align='right'>{d.OtherDeductions:N2}</td><td align='right'>{d.NetPay:N2}</td></tr>"));

        var html = $@"<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>
<head><meta charset='UTF-8'><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Report</x:Name></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head>
<body><table border='1' style='border-collapse:collapse;font-family:Arial;font-size:11px'>
<tr style='background:#1a237e;color:#fff'><th>Code</th><th>Name</th><th>Department</th><th>Designation</th><th>Gross</th><th>Tax</th><th>Deductions</th><th>Net</th></tr>
{rows}</table></body></html>";

        return Encoding.UTF8.GetBytes(html);
    }

    private static byte[] GeneratePdfHtml(List<SalaryRegisterDto> data)
    {
        var rows = string.Join("", data.Select(d =>
            $"<tr><td style='padding:6px 10px;border:1px solid #ddd'>{d.EmployeeCode}</td><td style='padding:6px 10px;border:1px solid #ddd'>{d.EmployeeName}</td><td style='padding:6px 10px;border:1px solid #ddd'>{d.Department}</td><td style='padding:6px 10px;border:1px solid #ddd'>{d.Designation}</td><td style='padding:6px 10px;border:1px solid #ddd;text-align:right'>&#8377;{d.GrossPay:N2}</td><td style='padding:6px 10px;border:1px solid #ddd;text-align:right'>&#8377;{d.Tax:N2}</td><td style='padding:6px 10px;border:1px solid #ddd;text-align:right'>&#8377;{d.OtherDeductions:N2}</td><td style='padding:6px 10px;border:1px solid #ddd;text-align:right'><strong>&#8377;{d.NetPay:N2}</strong></td></tr>"));

        var totalGross = data.Sum(d => d.GrossPay);
        var totalTax = data.Sum(d => d.Tax);
        var totalDeductions = data.Sum(d => d.OtherDeductions);
        var totalNet = data.Sum(d => d.NetPay);

        var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>Payroll Report</title>
<style>body{{font-family:Arial,sans-serif;margin:30px}}h1{{color:#1a237e;border-bottom:2px solid #1a237e;padding-bottom:10px}}
table{{width:100%;border-collapse:collapse;margin:20px 0}}th{{background:#1a237e;color:#fff;padding:8px 10px;text-align:left}}
td{{padding:6px 10px;border:1px solid #ddd}}.total{{background:#e8eaf6;font-weight:700}}
.footer{{text-align:center;color:#999;font-size:11px;margin-top:30px}}</style></head>
<body><h1>Payroll Report</h1>
<p style='color:#666'>Generated: {DateTime.UtcNow:dd-MMM-yyyy HH:mm}</p>
<table><thead><tr><th>Code</th><th>Name</th><th>Department</th><th>Designation</th><th>Gross</th><th>Tax</th><th>Deductions</th><th>Net</th></tr></thead><tbody>
{rows}
<tr class='total'><td colspan='4' style='text-align:right'>TOTAL</td><td style='text-align:right'>&#8377;{totalGross:N2}</td><td style='text-align:right'>&#8377;{totalTax:N2}</td><td style='text-align:right'>&#8377;{totalDeductions:N2}</td><td style='text-align:right'>&#8377;{totalNet:N2}</td></tr>
</tbody></table>
<p><strong>Summary:</strong> {data.Count} employees | Total Gross: &#8377;{totalGross:N2} | Total Net: &#8377;{totalNet:N2} | Total Tax: &#8377;{totalTax:N2}</p>
<p class='footer'>This is a computer-generated document &middot; Payroll Solutions Inc.</p></body></html>";

        return Encoding.UTF8.GetBytes(html);
    }
}
