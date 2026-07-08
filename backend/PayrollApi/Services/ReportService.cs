using System.Text;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PayrollApi.Services;

public class ReportService : IReportService
{
    private readonly PayrollDbContext _context;

    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

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
            "excel" => GenerateExcel(data),
            "pdf" => GeneratePdf(data),
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

    private static byte[] GenerateExcel(List<SalaryRegisterDto> data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Salary Register");
        ws.Cells[1, 1].Value = "Employee Code";
        ws.Cells[1, 2].Value = "Name";
        ws.Cells[1, 3].Value = "Department";
        ws.Cells[1, 4].Value = "Designation";
        ws.Cells[1, 5].Value = "Gross";
        ws.Cells[1, 6].Value = "Tax";
        ws.Cells[1, 7].Value = "Deductions";
        ws.Cells[1, 8].Value = "Net";

        using (var r = ws.Cells[1, 1, 1, 8])
        {
            r.Style.Font.Bold = true;
            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(26, 35, 126));
            r.Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        for (int i = 0; i < data.Count; i++)
        {
            var row = i + 2;
            ws.Cells[row, 1].Value = data[i].EmployeeCode;
            ws.Cells[row, 2].Value = data[i].EmployeeName;
            ws.Cells[row, 3].Value = data[i].Department;
            ws.Cells[row, 4].Value = data[i].Designation;
            ws.Cells[row, 5].Value = data[i].GrossPay;
            ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Value = data[i].Tax;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 7].Value = data[i].OtherDeductions;
            ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 8].Value = data[i].NetPay;
            ws.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
        }

        var totalRow = data.Count + 2;
        ws.Cells[totalRow, 1].Value = "TOTAL";
        ws.Cells[totalRow, 1, totalRow, 4].Merge = true;
        ws.Cells[totalRow, 5].Value = data.Sum(d => d.GrossPay);
        ws.Cells[totalRow, 6].Value = data.Sum(d => d.Tax);
        ws.Cells[totalRow, 7].Value = data.Sum(d => d.OtherDeductions);
        ws.Cells[totalRow, 8].Value = data.Sum(d => d.NetPay);
        using (var r = ws.Cells[totalRow, 1, totalRow, 8])
        {
            r.Style.Font.Bold = true;
            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(232, 234, 246));
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    private static byte[] GeneratePdf(List<SalaryRegisterDto> data)
    {
        var totalGross = data.Sum(d => d.GrossPay);
        var totalTax = data.Sum(d => d.Tax);
        var totalDeductions = data.Sum(d => d.OtherDeductions);
        var totalNet = data.Sum(d => d.NetPay);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Element(c => c.Column(col =>
                {
                    col.Item().Text("Payroll Report").FontSize(18).Bold().FontColor(Colors.Blue.Darken4);
                    col.Item().Text($"Generated: {DateTime.UtcNow:dd-MMM-yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    col.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken4);
                }));

                page.Content().Element(c => c.Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Code").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Name").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Department").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Designation").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Gross").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Tax").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Deductions").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(Colors.Blue.Darken4).Padding(6).Text("Net").FontColor(Colors.White).Bold().AlignRight();
                    });

                    foreach (var item in data)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.EmployeeCode);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.EmployeeName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Department);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Designation);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.GrossPay:N2}").AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.Tax:N2}").AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.OtherDeductions:N2}").AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.NetPay:N2}").AlignRight().Bold();
                    }

                    table.Cell().ColumnSpan(4).Background(Colors.Grey.Lighten3).Padding(4).Text("TOTAL").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{totalGross:N2}").AlignRight().Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{totalTax:N2}").AlignRight().Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{totalDeductions:N2}").AlignRight().Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{totalNet:N2}").AlignRight().Bold();
                }));

                page.Footer().AlignCenter().Text("This is a computer-generated document").FontSize(8).FontColor(Colors.Grey.Darken2);
            });
        }).GeneratePdf();
    }
}
